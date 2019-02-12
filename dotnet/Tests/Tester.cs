    using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Ara3D.Tests
{
    public class Test
    {
        public string Name;
        public TimeSpan Elapsed;
        public bool Result;
        public Func<bool> Function;

        public override string ToString()
        {
            return $"{(Result ? "PASSED" : "FAILED")} test {Name} in {Elapsed.Milliseconds} msec";
        }
    }

    public class Tester
    {
        public List<Test> Tests = new List<Test>();
            
        public void AddTest(string name, Action test)
        {
            AddTest(name, () => { test(); return true; });
        }

        public void AddTest(string name, Func<bool> test)
        {
            Tests.Add(new Test { Name = name, Function = test });
        }

        public void RunTests()
        {
            var sw = new Stopwatch();

            foreach (var t in Tests)
            {
                sw.Start();
                t.Result = t.Function();
                Assert.IsTrue(t.Result, $"Test {t.Name} failed");
                sw.Stop();
                t.Elapsed = sw.Elapsed;

                // TODO: maybe push this to a text writer argument or something                 
                // TODO: maybe making logging optional (maybe we 
                Console.WriteLine(t);
            }
        }
    }
}
