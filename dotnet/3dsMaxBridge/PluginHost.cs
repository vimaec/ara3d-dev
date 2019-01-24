    
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;

namespace Ara3D
{
    public class PluginHost<InterfaceType> 
    {
        public string Script { get; set; }
        public Assembly Assembly { get; set; }
        public InterfaceType Plugin { get; set; }
        public List<string> Errors { get; } = new List<string>();
        public string ErrorText => string.Join("\n", Errors);
        public bool CompiledSuccessfully => Plugin != null;

        public void Compile(string script)
        {
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

            var allAsmNames = asmShortNames.Concat(asmMaxShortNames);
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var asmNames = allAsmNames.Select((name) => Path.Combine(dir, name)).ToList();

            // TODO: 
            // "Geometry3D.dll", "ViperEngine.dll", "BulletSharp.dll", "RevitDB.dll", "ViperExtension.dll", "ViperGeometry3D.dll", 
            // "RevitAPI.dll", "SharpDX.dll", "ManagedOpenVDB.dll"

            // What is the full scope of the .NET API that 3ds Max has? 
            // Open every assembly, and reflect over it. 

            /*
            foreach (var asm in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
                asmNames.Add(asm.Name + ".dll");
            asmNames.Add(Assembly.GetExecutingAssembly().FullName + ".dll");

            foreach (var asm in Assembly.GetEntryAssembly().GetReferencedAssemblies())
                asmNames.Add(asm.Name + ".dll");
            asmNames.Add(Assembly.GetEntryAssembly().FullName + ".dll");

            foreach (var asm in Assembly.GetCallingAssembly().GetReferencedAssemblies())
                asmNames.Add(asm.Name + ".dll");
            asmNames.Add(Assembly.GetCallingAssembly().FullName + ".dll");
            */

            // Rremove duplicates
            asmNames = asmNames.Distinct().ToList();

            if (script == Script)
                return;
            Script = script;
            Assembly = null;
            Errors.Clear();
            Assembly = Compiler.CompileSource(script, asmNames, Errors, false);
            if (Assembly != null)
                Plugin = Compiler.ActivateClassImplementingInterface<InterfaceType>(Assembly);
        }
    }
}
