
using System.Collections.Generic;

namespace Ara3D
{
    public class UtilityPluginScript
    {
        PluginHost<IUtilityPlugin> Host = new PluginHost<IUtilityPlugin>();
        public string FileName { get; }
        public UtilityPluginScript(string fileName)
        {
            FileName = fileName;
        }
        public IList<string> Compile()
        {
            Host.Compile(FileName);
            return Host.Errors;
        }
        public bool Run()
        {
            if (!Host.CompiledSuccessfully)
                return false;
            Host.Plugin.Evaluate();
            return true;
        }        
    }
}
