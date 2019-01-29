using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using Autodesk.Max.MaxPlus;
using Object = Autodesk.Max.MaxPlus.Object;

namespace Ara3D
{
    /*
        asm = dotNet.loadAssembly @"C:\dev\repos\AraGeometry\3dsMaxBridge\bin\Debug\3dsMaxBridge.dll"
        bridgeClass = dotNetClass "Ara3D.AraGeometryBridge"         
    */
    public static class API
    {
        public static Vector3 ToVector(this Point3 pt)
            => new Vector3(pt.X, pt.Y, pt.Z);        

        public static Matrix4x4 ToMatrix(this Matrix3 mat)
        {
            var row0 = mat.GetRow(0);
            var row1 = mat.GetRow(1);
            var row2 = mat.GetRow(2);
            var tr = mat.GetTranslation();
            return new Matrix4x4(
                row0.X, row0.Y, row0.Z, 0,
                row1.X, row1.Y, row1.Z, 0,
                row2.X, row2.Y, row2.Z, 1,
                tr.X, tr.Y, tr.Z, 1
                );
        }

        public static IGeometry ToTransformedIGeometry(this INode node)
            => node.ToIGeometry()?.Transform(node.GetWorldTM().ToMatrix());

        public static IGeometry AllGeometry
            => AllNodes.ToIGeometry();

        public static IEnumerable<INode> GetDescendants(this INode node)
        {
            foreach (var child in node.Children)
                foreach (var d in GetDescendants(child))
                    yield return d;
            yield return node;
        }

        public static IEnumerable<INode> AllNodes
            => GetDescendants(Core.GetRootNode());

        public static IArray<INode> SelectedNodes
            => SelectionManager.GetCount().Select(SelectionManager.GetNode);

        public static bool AreAnyNodesSelected
            => SelectedNodes.Count > 0;

        public static IEnumerable<INode> AllNodesOrJustSelected
            => AreAnyNodesSelected ? SelectedNodes.ToEnumerable() : AllNodes;

        public static IGeometry ToIGeometry(this IEnumerable<INode> nodes)
            => nodes.Select(ToTransformedIGeometry).WhereNotNull().ToIArray().Merge();

        public static IGeometry AllGeometryOrJustSelected
            => AllNodesOrJustSelected.ToIGeometry();

        public static INode FirstSelectedNode
            => SelectionManager.GetCount() > 0 ? SelectionManager.GetNode(0) : null;        

        public static string GetNodeInfo(this INode node)
        {
            if (node == null) return "null";
            if (!node._IsValidWrapper()) return "Invalid node wrapper";
            var sb = new StringBuilder();
            sb.AppendFormat("Node {0} {1}", node.Name, node.AnimHandle).AppendLine();
            var g = node.ToTransformedIGeometry();
            sb.AppendLine(g?.GetStats() ?? "no geometry");
            return sb.ToString();
        }

        public static IGeometry ToIGeometry(this INode node)
        {
            if (node == null || !node._IsValidWrapper()) return null;

            var orig = node.EvalWorldState().Getobj();
            if (!orig._IsValidWrapper())
                return null;

            var obj = orig;
            try
            {
                // Check if we have to convert the node to a TriObject (EditMesh)
                var triObjectID = ClassIds.TriMeshGeometry; // new Class_ID(0x0009, 0);
                if (!obj.IsSubClassOf(triObjectID))
                {
                    if (obj.CanConvertToType(ClassIds.TriMeshGeometry) == 0)
                        return null; 
                    obj = obj.ConvertToType(triObjectID).Validate();
                }
                var tri = TriObject._CastFrom(obj).Validate();
                var mesh = tri.GetMesh();
                return mesh.ToIGeometry();
            }
            finally
            { 
                // If we converted, we have to delete the TriObject we created, it is a temporary
                if (obj != orig)
                {
                    if (obj != null && obj._IsValidWrapper())
                        obj.DeleteMe();
                    obj._SetValue(System.IntPtr.Zero);
                }
            }
        }

        public static IGeometry ToIGeometry(this Mesh mesh)
        {
            if (!mesh._IsValidWrapper())
                return null;

            var numVerts = mesh.GetNumVertices();
            var numFaces = mesh.GetNumFaces();

            // Important: we have to copy the data by evaluating the arrays immediately 
            var verts = numVerts.Select(v => mesh.GetVertex(v).ToVector()).Evaluate();
            var indices = (numFaces * 3).Select(f => mesh.GetFaceVertexIndex(f / 3, f % 3)).Evaluate();

            return new TriMesh(verts, indices);
        }

        public static T Validate<T>(this T x) where T: Wrapper
        {
            if (x == null) throw new System.Exception("Null pointer exception for type " + typeof(T).Name);
            if (!x._IsValidWrapper()) throw new System.Exception("Not a valid wrapper of type " + typeof(T).Name);
            return x;
        }

        public static void CopyToNode(this IGeometry g, INode node)
        {
            // TODO: this muse be defined somewhere             
            var orig = node.Validate().EvalWorldState().Getobj().Validate();
            var obj = orig;

            // Check if we have to convert the node to a TriObject (EditMesh)
            if (!obj.IsSubClassOf(ClassIds.TriMeshGeometry))
                throw new System.Exception("Expected node to have a TriObject attached to it.");
            var tri = TriObject._CastFrom(obj).Validate();
            var mesh = tri.GetMesh();
            CopyToMesh(g, mesh);
        }

        public static void CopyToMesh(this IGeometry g, Mesh mesh)
        {
            var triMesh = g.ToTriMesh();
            var numVerts = g.Vertices.Count;
            var numFaces = triMesh.Elements.Count;
            mesh.SetNumVerts(numVerts);
            mesh.SetNumFaces(numFaces);

            for (var i=0; i < numVerts; ++i)
            {
                var vert = g.Vertices[i];
                mesh.SetVert(i, vert.X, vert.Y, vert.Z);
            }

            for (var i=0; i < numFaces; ++i)
            {
                var e = triMesh.Elements[i];
                mesh.SetFaceVertexIndexes(i, e[0], e[1], e[2]);
                // TODO: edge visibility
                // TODO: smoothing group, material id 
            }

            // TODO: map channels

            // TODO: normals

            // TODO: what invalidating do I actually need
            mesh.InvalidateGeomCache();
            mesh.InvalidateTopologyCache();
        }

        public static GeometryPluginHost CreatePluginInstance()
        {
            return new GeometryPluginHost();
        }

        public static INode CreateNode(this Object obj) 
        {
            return Factory.CreateNode(obj).Validate();
        }

        public static TriObject CreateTriObject()
        {
            return TriObject._CastFrom(Factory.CreateGeomObject(ClassIds.TriMeshGeometry)).Validate();
        }

        public static INode CreateNodeWithEmptyGeometry()
        {
            return CreateTriObject().CreateNode();
        }

        public static INode ToNode(this IGeometry g)
        {
            var tri = CreateTriObject().Validate();
            g.CopyToMesh(tri.GetMesh());
            return CreateNode(tri);
        }

        public static string NewScript()
        {
            return
 @"
using Autodesk.Max;
using Autodesk.Max.MaxPlus;
using System.Text;

namespace Ara3D
{
    public class SamplePlugin : IUtilityPlugin
    {
        public void Evaluate()
        {
            // TODO: this is where the magic happens 
        }
    }
}
";
        }

        public static Process ShowEditor()
        {
            // Launch the executable
            var process = ServiceLauncher.LaunchProcess();

            // Connect to the service using our local callback
            ServiceConfig.OpenClientChannel(EditorClient.Instance);

            return process;
        }

        public class MAXScriptConsoleWriter : TextWriter
        {
            public override Encoding Encoding => Encoding.Default;

            public override void Write(char value)
            {
                Core.Write(value.ToString())
;            }

            public override void Write(string s)
            {
                Core.Write(s);
            }

            public override void WriteLine()
            {
                Core.WriteLine("");
            }

            public override void WriteLine(string s)
            {
                base.WriteLine(s);
            }
        }

        public static void ConsoleToMAXScriptListener()
        {
            Console.SetOut(new MAXScriptConsoleWriter());
        }
    }
}
