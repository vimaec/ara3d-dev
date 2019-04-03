using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace devtool
{
    public class Program
    {
        public static readonly string RepoRoot = Path.Combine("..", "..", "..");

        public static string Cwd => Directory.GetCurrentDirectory();
        public static string ReposParentDir = Path.Combine(Cwd, "..");
        public static string[] RepoNames = {"ara3d-dev", "math3d", "revit-dev"};
        public static IEnumerable<string> RepoPaths = RepoNames.Select(n => Path.Combine(ReposParentDir, n));
        public static string VersionFile = "version.txt";

        public static void CheckDirs()
        {
            if (Path.GetFileName(Cwd) != "ara3d-dev")
                throw new Exception("Expected to be in the ara3d-dev root directory");

            foreach (var f in RepoPaths)
                if (!Directory.Exists(f))
                    throw new Exception($"Could not find repo {f}");
        }


        public static string GetVersionString()
        {
            return File.ReadAllText(VersionFile);
        }

        public static void RunDosCommand(string dir, string args)
        {
            var cmd = new Process();
            cmd.StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                WorkingDirectory = dir,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false,    
                Arguments = "/c " + args,
            };
            cmd.Start();
            cmd.WaitForExit();
            Console.WriteLine(cmd.StandardOutput.ReadToEnd());
        }

        public static void Main(string[] args)
        {
            // status check on everything             

            // FOr each repo:
            // Update the version string according 


            var vs = GetVersionString();
            var v = Version.Parse(vs);

            switch (args[1])
            {
                case "-major":
                    v = new Version(v.Major + 1, 0, 0);
                    break;
                case "-minor":
                    v = new Version(v.Major, v.Minor + 1, 0);
                    break;
                case "-patch":
                    v = new Version(v.Major, v.Minor, v.Revision + 1);
                    break;
                default:
                    throw new Exception("Expected -major|-minor|-patch as first argument");
            }

            var baseMsg = args.Length > 2 ? args[2] : "";

            // TODO: 
            // * write the version to each folder 
            
            // * git add . 
            // * git commit -m {commitMsg}
            // * git tag -a v{v} -m {tagMsg} 

            var commitMsg = $"";
            var tagMsg = $"";

            var command = $"git add . && git commit -m {commitMsg} && git tag -a v{v} -m {tagMsg} && git push";
        }
    }
}
