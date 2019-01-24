asm = dotNet.loadAssembly @"C:\dev\repos\AraGeometry\3dsMaxBridge\bin\Debug\3dsMaxBridge.dll"
ara = dotNetClass "Ara3D.Ara"
-- g = ara.MergeAllGeometry ()
host = dotNetObject "Ara3D.GeometryPluginHost"
host.Compile "namespace Ara3D
{
    public class SamplePlugin : IGeometryPlugin
    {
        public IGeometry Evaluate(IGeometryPluginHost host)
        {
            // TODO: this is where the magic happens 
            //return QuadMesh.Empty;
			   var mesh = TriMesh.Empty;
				//var node = Ara.ToNode(mesh);
				Ara.MergeAllGeometry().ToNode();
				return mesh;
        }
    }
}"
if host.plugin == undefined then (
	print "Errors = " + host.ErrorText
)
else (
	host.plugin.Evaluate undefined
)