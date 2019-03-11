using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

// Inspired by Microsoft.Extensions.ILogger, but simpler
// https://msdn.microsoft.com/en-us/magazine/mt694089.aspx

namespace Ara3D
{
    public enum LogLevel
    {
        Debug = 1,
        Verbose = 2,
        Information = 3,
        Warning = 4,
        Error = 5,
        Critical = 6,
        None = int.MaxValue
    }

    public class Frame
    {
        public string MethodName { get; set; }
        public string FileName { get; set; }
        public int LineNumber { get; set; }

        public static Frame GetFrame(int depth)
        {
            var f = new StackTrace().GetFrame(depth + 1);
            return new Frame
            {
                MethodName = f.GetMethod().Name,
                FileName = f.GetFileName(),
                LineNumber = f.GetFileLineNumber(),
            };
        }
    }

    public interface ILogger
    {
        ILogger Log(string message = "", LogLevel level = LogLevel.None, int eventId = 0);
        void ExportLog(string path);
    }

    public class LogEvent
    {
        public ILogger Logger;
        public string Message;
        public LogLevel Level;
        public int Index;
        public DateTime When;
        public int EventId;

        public override string ToString()
        {
            return $"{Level.EnumName()} {Index} {When} {Message}";
        }
    }

    public class StdLogger : ILogger
    {
        public List<LogEvent> Events = new List<LogEvent>();
        public readonly bool EchoToConsole;

        public StdLogger(bool echoToConsole = false)
        {
            EchoToConsole = echoToConsole;
        }

        public ILogger Log(string message = "",  LogLevel level = LogLevel.None, int eventId = 0)
        {

            var e = new LogEvent
            {
                EventId = eventId,
                Index = Events.Count,
                Message = message,
                When = DateTime.Now
            };
            Events.Add(e);
            if (EchoToConsole)
                Console.WriteLine(e);
            #if DEBUG
            Debug.WriteLine(e);
            #endif
            return this;
        }

        public void ExportLog(string path)
        {
            File.WriteAllLines(path, Events.Select(e => e.ToString()));
        }
    }

    public class NullLogger : ILogger
    {
        public ILogger Log(string message = "", LogLevel level = LogLevel.None, int eventId = 0)
        {
            return this;
        }

        public void ExportLog(string path)
        {
        }
    }

    public static class Logger
    {
        // Normally you would put a logger in your application main class 
        public static readonly ILogger DefaultLogger = new StdLogger();
        public static readonly ILogger NullLogger = new NullLogger();

        /*
        public static ILogger Log(string message = "", object state = null, LogLevel level = LogLevel.None, int eventId = 0)
        {
            return DefaultLogger.Log(message, state, level, eventId);
        }        
        */

        public static ILogger LogFrame(this ILogger logger, int frameDepth = 1)
        {
            return logger.Log($"Current frame {Frame.GetFrame(frameDepth)}");
        }

        public static void Log(this ILogger logger, string message, Action action, LogLevel level = LogLevel.Debug, LogLevel exceptionLevel = LogLevel.Critical)
        {
            logger.Log(message, () =>
            {
                action();
                return true;
            }, level, exceptionLevel);
        }

        public static T Log<T>(this ILogger logger, string message, Func<T> func,  LogLevel level = LogLevel.Debug, LogLevel exceptionLevel = LogLevel.Critical, bool rethrow = false)
        {
            try
            {
                logger.Log("begin: " + message, level);
                return func();
            }
            catch (Exception e)
            {
                logger.Log($"Exception: {e.Message} at {e.StackTrace}", exceptionLevel);
                logger.LogFrame(2); // Want to log the frame of the calling function.
                if (rethrow)
                    throw;
            }
            finally
            {
                logger.Log("end: " + message, level);
            }

            return default;
        }
    }
}
