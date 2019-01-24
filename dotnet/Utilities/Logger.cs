using System;
using System.Diagnostics;

namespace Ara3D
{
    public static class Logger
    {
        static Stopwatch stopwatch;
        static long last;

        public static void Init()
        {            
            stopwatch = new Stopwatch();
            stopwatch.Start();
            Log("Log started");
        }

        public static void Log(string text)
        {
            var current = stopwatch.ElapsedMilliseconds;
            var diff = current - last;
            Console.WriteLine($"{stopwatch.Elapsed}: {text} (elapsed msec = {diff})");
            last = current;
        }
    }
}
