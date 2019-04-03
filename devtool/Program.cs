using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Ara3D;

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
                throw new Exception($"Expected to be in the ara3d-dev root directory not {Cwd}");

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
            CheckDirs();

            var vs = GetVersionString();
            var v = Version.Parse(vs);

            var typeOfCommit = args[0].Substring(1).ToUpperInvariant();

            switch (typeOfCommit)
            {
                case "MAJOR":
                    v = new Version(v.Major + 1, 0, 0);
                    break;
                case "MINOR":
                    v = new Version(v.Major, v.Minor + 1, 0);
                    break;
                case "PATCH":
                    v = new Version(v.Major, v.Minor, v.Revision + 1);
                    break;
                case "REV":
                    break;
                default:
                    throw new Exception("Expected -major|-minor|-patch|-rev as first argument");
           }

            var baseMsg = args.Length > 1 ? args[1].StripQuotes() : "";

            // TODO: 
            // * write the version to each folder 

            // * git add . 
            // * git commit -m {commitMsg}
            // * git tag -a v{v} -m {tagMsg} 
            // http://gitready.com/beginner/2009/02/03/tagging.html
            // https://stackoverflow.com/questions/5358336/how-to-list-all-tags-along-with-the-full-message-in-git

            var nowStamp = Util.GetTimeStamp();
            var commitMsg = $"[{typeOfCommit}] {v} {nowStamp} {baseMsg}";          

            // Revisions aren't tagged
            var command = (typeOfCommit != "REV")
                ? $"git add version.txt && git commit -m \"{commitMsg}\" && git tag -a {v} -m \"{commitMsg}\" && git push --tags"
                : $"git add . && git commit -m \"{commitMsg}\" && git push";

            foreach (var f in RepoPaths)
            {
                if (typeOfCommit != "REV")
                {
                    var fp = Path.Combine(f, "version.txt");
                    File.WriteAllText(fp, v.ToString());
                }

                RunDosCommand(f, command);
            }
        }
    }
}
