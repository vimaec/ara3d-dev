using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;
using System.IO;

namespace Ara3D
{
    // TODO: load the compiled scripts into their own AppDomain for protection and to not leak memory 
    // TODO: get the configuration information from a file. 
    // TODO: generate debug information for the compiled scripts. 
    // https://stackoverflow.com/questions/1799373/how-can-i-prevent-compileassemblyfromsource-from-leaking-memory
    public static class Compiler
    {
        public static Assembly CompileFile(string file, IEnumerable<string> referencedAssemblies, List<string> errors, bool loadAssemblies)
        {
            return Compile(file, false, referencedAssemblies, errors, loadAssemblies);
        }

        public static Assembly CompileSource(string source, IEnumerable<string> referencedAssemblies, List<string> errors, bool loadAssemblies)
        {
            return Compile(source, true, referencedAssemblies, errors, loadAssemblies);
        }

        public static T ActivateClassImplementingInterface<T>(Assembly asm) 
        {
            var itype = typeof(T);
            if (!itype.IsInterface) throw new Exception("Expected an interface as a type argument");
            if (asm == null) throw new Exception("No compiled assembly was found");
            var types = asm.GetTypes();
            if (types.Length == 0) throw new Exception("No types found in the assembly");
            var candidates = types.Where(t => t.IsClass && t.GetInterfaces().Contains(itype)).ToArray();
            if (candidates.Length == 0) throw new Exception($"No classes found that implement the interface {itype.Name}");
            if (candidates.Length > 1) throw new Exception($"Multiple classes found that implement the interface {itype.Name}");
            var c = candidates[0];
            return (T)Activator.CreateInstance(c);
        }

        /// <summary>
        /// Compiles a C# script as a string or from a file. If "LoadAssemblies" is true, this call will try 
        /// to manually load each assembly so that it is truly available on execution.
        /// </summary>
        public static Assembly Compile(string codeOrFile, bool isCodeOrFile, IEnumerable<string> referencedAssemblies, List<string> outputErrors, bool loadAssemblies)
        {            
            try
            {
                if (loadAssemblies)
                {
                    foreach (var ra in referencedAssemblies)
                    {
                        try
                        {
                            Assembly.UnsafeLoadFrom(ra);
                        }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                        catch (Exception)
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                        {
                        }
                    }
                }

                var csc = new CSharpCodeProvider(new Dictionary<string, string> { { "CompilerVersion", "v4.0" } });
                var parameters = new CompilerParameters(referencedAssemblies.ToArray())
                {
                    GenerateInMemory = false, 
                    //GenerateExecutable = false,
                    TempFiles = new TempFileCollection(Path.GetTempPath(), true),
                    IncludeDebugInformation = true,
                };
                var results = isCodeOrFile
                    ? csc.CompileAssemblyFromSource(parameters, codeOrFile)
                    : csc.CompileAssemblyFromFile(parameters, codeOrFile);
                results.Errors.Cast<CompilerError>().ToList().ForEach(error => outputErrors.Add(error.ToString()));
                if (results.Errors.Count > 0) return null;
                return results.CompiledAssembly;
            }
            catch (Exception e)
            {
                outputErrors.Add(e.Message);
                return null;
            }
        }
    }
}
