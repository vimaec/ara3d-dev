using Autodesk.Max.MaxPlus;

namespace Ara3D
{
    // Provides data to the IGeometryPlugin and calls it when necessary
    public class GeometryPluginHost : PluginHost<IGeometryPlugin>, IGeometryPluginHost
    {
        // The plug-in host parameters 
        public object[] Args { get; set; }
        public int Time { get; set; }
        public int LastTime { get; set; }
        public IGeometry LastValue { get; set; }

        public void UpdateMesh(int time, Mesh mesh, string script, object[] args)
        {
            Compile(script);
            if (Plugin != null)
            {
                Args = args;
                Time = time;
                LastValue = Plugin.Evaluate(this);
                LastTime = time;
                LastValue.CopyToMesh(mesh);
            }
            else
            {
                LastValue = null;
                LastTime = time;                
            }
        }
    }
}
