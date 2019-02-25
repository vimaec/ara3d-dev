using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;

namespace Ara3D
{
    public class ObjEmitter
    {
        readonly IndexedSet<ObjMaterial> materials = new IndexedSet<ObjMaterial>();
        public static ObjMaterial DefaultMaterial = new ObjMaterial("default", new Vector4(0.5f, 0.5f, 0.5f, 1.0f));

        public string MaterialName(ObjMaterial mat)
        {
            materials.Add(mat);
            return mat.Name.ToIdentifier() + "_" + materials[mat].ToString();
        }

        public string MaterialDefn(ObjMaterial mat)
        {
            var formatStr = "newmtl {0}\r\n Ka {1} {2} {3}\r\n Kd {1} {2} {3}\r\n d {4}";
            return string.Format(formatStr, MaterialName(mat), mat.Color.X, mat.Color.Y, mat.Color.Z, mat.Color.W);
        }

        // Note: you would set generateMtl false, if generating files for subsets of the files 
        public ObjEmitter(IEnumerable<ObjGeometry> geometries, string filePath, bool generateMtl = true)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            var mtlFile = Path.ChangeExtension(filePath, ".mtl");

            var objectContents = EmitObjectStrings(geometries, generateMtl ? mtlFile : null);
            File.WriteAllLines(filePath, objectContents);

            // Output the material file
            if (generateMtl)
            {
                var lines = materials.Keys.Select(MaterialDefn);
                File.WriteAllLines(mtlFile, lines);
            }
        }
        
        public IEnumerable<string> EmitObjectStrings(IEnumerable<ObjGeometry> geometries, string mtlFilePath)
        {
            var currentMat = DefaultMaterial;
            var useMtlLib = !string.IsNullOrEmpty(mtlFilePath);
 
            // Creates a lookup table of vertices            
            var vertices = geometries.SelectMany(g => g.Geometry.Vertices.ToEnumerable());
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute));
            var assemblyTitleAttribute = attributes.SingleOrDefault() as AssemblyTitleAttribute;
            var name = Assembly.GetExecutingAssembly().GetName().Name;
            var title = assemblyTitleAttribute?.Title ?? name;

            yield return $"# Generator = {title}";
            yield return $"# Version = {version}";
            yield return $"# Created = {DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture)}";
            yield return $"# GUID = {Guid.NewGuid()}";
            
            if (useMtlLib)
                yield return ($"mtllib {mtlFilePath}");

            // Write the vertices 
            foreach (var v in vertices)
                yield return ($"v {v.X} {v.Y} {v.Z}");

            var offset = 1;
            var currentName = "";
            foreach (var fg in geometries)
            {
                //Debug.Assert(fg.Geometry.IsValid());

                if (currentName != fg.Name)
                    yield return $"o {currentName = fg.Name}";

                // TODO: add comments for the face group
                
                if (useMtlLib && !fg.Material.Equals(currentMat))
                {
                    currentMat = fg.Material;
                    yield return ($"usemtl {MaterialName(currentMat)}");
                }

                var g = fg.Geometry;
                var sb = new StringBuilder();
                foreach (var e in g.GetFaces().ToEnumerable())
                {
                    sb.Append("f");
                    foreach (var p in e.ToEnumerable())
                        sb.Append(" ").Append(p+offset);
                    yield return sb.ToString();
                    sb.Clear();
                }

                offset += g.Vertices.Count;
            }
        }
    }
}
