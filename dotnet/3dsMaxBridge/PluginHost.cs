
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;

namespace Ara3D
{
    public class PluginHost<InterfaceType> where InterfaceType : class
    {
        public Assembly Assembly { get; set; }
        public InterfaceType Plugin { get; set; }
        public List<string> Errors { get; } = new List<string>();
        public string ErrorText => string.Join("\n", Errors);
        public bool CompiledSuccessfully => Plugin != null;

        public void Compile(string scriptFile)
        {
            Plugin = null;

            if (!File.Exists(scriptFile))
                throw new System.Exception("File does not exist");

            // TODO: rename my DLLs to start with "Ara3D"
            var asmShortNames = new[] {
                "System.Numerics.dll",
                "Ara3D.3dsMaxBridge.dll",
                "Ara3D.Geometry.dll",
                "Ara3D.LinqArray.dll",
                "Ara3D.Math.dll",
                "Ara3D.DotNetUtilities.dll",
                "Ara3D.G3SharpBridge.dll",
                "geometry3Sharp.dll",
                "Newtonsoft.Json.dll",
            };

            // TODO: maybe iterate over all assemblies in the output folder? 

            var asmMaxShortNames = new[] {
                "Autodesk.Max.dll",
                "Geometry3D.dll",
                "CSharpUtilities.dll",
                "ImmutableArray.dll",
                "BulletSharp.dll",
                "ManagedOpenVDB.dll",
                "ManagedServices.dll",
                "MaxPlusDotNet.dll",
                "Autodesk.Max.dll",
                "MaxPlusDotNet.dll",
                "MonoGame.Framework.dll",
                "RevitAPI.dll",
                "RevitAPIIFC.dll",
                "UiViewModels.dll",
                "Viper3dsMaxBridge.dll",
                "ViperEngine.dll",
                "ViperExtension.dll",
                "ViperGeometry3D.dll",
            };

            var systemDlls = new[]
            {
                "System.dll",
                "System.Core.dll",
                "System.Data.dll",
                "System.Data.DataSetExtensions.dll"
            };

            var allAsmNames = asmShortNames.Concat(asmMaxShortNames);
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var asmNames = allAsmNames.Select((name) => Path.Combine(dir, name)).Concat(systemDlls).ToList();
           
            // Remove duplicates
            asmNames = asmNames.Distinct().ToList();

            Assembly = null;
            Errors.Clear();
            Assembly = Compiler.CompileFile(scriptFile, asmNames, Errors, true);
            if (Assembly != null)
                Plugin = Compiler.ActivateClassImplementingInterface<InterfaceType>(Assembly);
        }
    }
}
