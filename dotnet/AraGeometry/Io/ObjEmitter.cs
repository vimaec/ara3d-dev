using Ara3D;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public ObjEmitter(IEnumerable<ObjGeometry> geometries, string folder, string baseFileName, bool generateMtl = true)
        {
            var objFile = Path.Combine(folder, baseFileName + ".obj");
            var mtlFile = Path.Combine(folder, baseFileName + ".mtl");

            var objectContents = EmitObjectStrings(geometries, baseFileName);
            File.WriteAllLines(objFile, objectContents);

            // Output the material file
            if (generateMtl)
            {
                var lines = materials.Keys.Select(MaterialDefn);
                File.WriteAllLines(mtlFile, lines);
            }
        }

        public static void Write(IEnumerable<ObjGeometry> geometries, string folder, string baseFileName, bool generateMtl = true)
        {
            new ObjEmitter(geometries, folder, baseFileName, generateMtl);
        }

        public IEnumerable<string> EmitObjectStrings(IEnumerable<ObjGeometry> geometries, string baseFileName, bool useMtlLib = true)
        {
            var currentMat = DefaultMaterial;

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
            yield return $"# Filename = {baseFileName}.obj";
            
            if (useMtlLib)
                yield return ($"mtllib {baseFileName}.mtl");

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
                foreach (var e in g.Elements.ToEnumerable())
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
