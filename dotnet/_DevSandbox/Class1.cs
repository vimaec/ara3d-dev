using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace _3dsMaxSamples
{
    /// <summary>
    /// TODO: convert this into a sample.
    /// </summary>
    public class ReflectOverDlls
    {
        public static void OutputDllInfo(string file)
        {
            try
            {
                var asm = Assembly.ReflectionOnlyLoadFrom(file);
                Console.WriteLine($"File {file} is a .NET assembly");
            }
            catch (Exception e)
            {
                // This happens when we know that the API is not a .NET API
                //Console.WriteLine(e.Message); 
            }
        }

        public static void OutputTypes(Assembly asm)
        {
            foreach (var t in asm.GetTypes().OrderBy(t => t.FullName))
            {
                var kind = t.IsInterface ? "interface" : "class";
                Console.WriteLine($"  {kind} {t.FullName}");
                foreach (var c in t.GetConstructors())
                    Console.WriteLine($"    constructor {c}");
                foreach (var m in t.GetMethods())
                    Console.WriteLine($"    method {m}");
                foreach (var p in t.GetProperties())
                    Console.WriteLine($"    property {p}");
                foreach (var f in t.GetFields())
                    Console.WriteLine($"    field {f}");
            }
        }

        public static void Main()
        {
            var maxFolder = Environment.GetEnvironmentVariable("ADSK_3DSMAX_x64_2019");
            foreach (var f in Directory.GetFiles(maxFolder, "*.dll", SearchOption.AllDirectories)
                .OrderBy(f => f)
                .AsParallel())
            {
                OutputDllInfo(f);
            }
        }
    }
}
