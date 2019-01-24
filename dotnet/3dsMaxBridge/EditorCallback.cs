using System.Linq;
using System.ServiceModel;

namespace Ara3D
{
    [CallbackBehavior(AutomaticSessionShutdown=false, IncludeExceptionDetailInFaults = true, ValidateMustUnderstand = false)] 
    public class EditorCallback : IEditorClientCallback
    {
        public string[] CompileAndRun(string text)
        {
            return API.EvalUtility(text).ToArray();
        }

        public string NewSnippet()
        {
            return API.NewScript();
        }
    }
}
