-- Box pos:[-33.1377,42.8053,0] isSelected:on width:13.642 length:32.2585 height:16.0751
-- Cone radius1:14.5439 radius2:5.62667 height:17.174 pos:[62.4749,-36.1697,0] isSelected:on

asm = dotNet.loadAssembly @"C:\dev\repos\AraGeometry\3dsMaxBridge\bin\Debug\3dsMaxBridge.dll"
ara = dotNetClass "Ara3D.Ara"
-- g = ara.MergeAllGeometry ()
host = dotNetObject "Ara3D.GeometryPluginHost"
host.Compile "using Autodesk.Max.MaxPlus;

namespace Ara3D
{
    public class SamplePlugin : IGeometryPlugin
    {		        
        public IGeometry Evaluate(IGeometryPluginHost host)
        {
            // TODO: this is where the magic happens 
					//var node = Ara.GetFirstSelectedNode();
					//Core.WriteLine(Ara.GetNodeInfo(node));			     				
            //node.GetTransformedGeometry().ToNode();
					Ara.MergeAllGeometry().ToNode();
					return null;
        }

    }
}"
if host.plugin == undefined then (
	print "Errors = " + host.ErrorText
)
else (
	host.plugin.Evaluate undefined
)