using Ara3D.DotNetUtilities;
using System;
using System.Collections.Generic;

namespace Ara3D
{
    public class EditorClient : SingletonRemotingObject<EditorClient>, IEditorClientCallback
    {
        public Dictionary<string, UtilityPluginScript> Scripts 
            = new Dictionary<string, UtilityPluginScript>();

        public UtilityPluginScript GetScript(string fileName) 
            => Scripts.GetOrCompute(fileName, f => new UtilityPluginScript(f));

        public IList<string> Compile(string fileName)
            => GetScript(fileName).Compile();       

        public bool Run(string fileName)
            => GetScript(fileName).Run();
        
        public string NewSnippet()
            => API.NewScript();
    }
}
