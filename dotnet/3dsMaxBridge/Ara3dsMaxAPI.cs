using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Autodesk.Max.MaxPlus;

namespace Ara3D
{
    /*
        asm = dotNet.loadAssembly @"C:\dev\repos\AraGeometry\3dsMaxBridge\bin\Debug\3dsMaxBridge.dll"
        bridgeClass = dotNetClass "Ara3D.AraGeometryBridge"         
    */
    public static class API
    {
        public static Vector3 ToVector(this Point3 pt)
        {
            return new Vector3(pt.X, pt.Y, pt.Z);
        }

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
        {
            return node.ToIGeometry()?.Transform(node.GetWorldTM().ToMatrix());
        }

        public static IGeometry MergeAllGeometry()
        {
            return GetAllNodes().Select(ToTransformedIGeometry).WhereNotNull().ToIArray().Merge();
        }

        public static IEnumerable<INode> GetDescendants(this INode node)
        {
            foreach (var child in node.Children)
                foreach (var d in GetDescendants(child))
                    yield return d;
            yield return node;
        }

        public static IEnumerable<INode> GetAllNodes()
        {
            return GetDescendants(Core.GetRootNode());
        }

        public static INode GetFirstSelectedNode()
        {
            return SelectionManager.GetCount() > 0 ? SelectionManager.GetNode(0) : null;
        }

        public static string GetNodeInfo(this INode node)
        {
            if (node == null) return "null";
            if (!node._IsValidWrapper()) return "Invalid node wrapper";
            var sb = new StringBuilder();
            sb.AppendFormat("Node {0} {1}", node.Name, node.AnimHandle).AppendLine();

            var g = node.ToTransformedIGeometry();
            if (g == null)
            {
                sb.AppendLine("no geometry");
            }
            else
            {
                sb.AppendLine(g.GetStats());
            }
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
                mesh.SetFaceVertexIndexes(i, triMesh.Indices[i * 3], triMesh.Indices[i * 3 + 1], triMesh.Indices[i * 3 + 2]);
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

        public static IList<string> EvalUtility(string code)
        {
            var host = new PluginHost<IUtilityPlugin>();
            host.Compile(code);
            if (host.CompiledSuccessfully)
                host.Plugin.Evaluate();
            return host.Errors;
        }
    
        public static EditorCallback EditorCallback = new EditorCallback();

        public static void ShowEditor()
        {
            // Launch the executable
            ServiceLauncher.LaunchProcess();

            // Connect to the service using our local callback
            ServiceLauncher.Connect(EditorCallback);
        }
    }
}
