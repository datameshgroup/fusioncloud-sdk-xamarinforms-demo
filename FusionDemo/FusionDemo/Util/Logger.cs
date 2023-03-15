using FusionDemo.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace FusionDemo.Util
{
    /// <summary>
    /// Shim for a logger object just for this test project  - replace with something useful!
    /// 
    /// This currently just dumps to logcat. Search for mono-stdout
    /// </summary>
    internal class Logger
    {
        private static readonly Lazy<Logger> lazy = new Lazy<Logger>(() => new Logger());
        public static Logger Instance { get { return lazy.Value; } }

        private Logger()
        {
        }

        public void Verbose(string message) => Console.WriteLine($"{DateTime.Now:YY-MM-dd HH:mm:ss} VERBOSE {message}");
        public void Error(string message) => Console.WriteLine($"{DateTime.Now:YY-MM-dd HH:mm:ss} ERROR   {message}");
        public void Error(Exception exception, string message) => Console.WriteLine($"{DateTime.Now:YY-MM-dd HH:mm:ss} ERROR   {message} {exception.Message} {exception.StackTrace}");
        public void Information(string message) => Console.WriteLine($"{DateTime.Now:YY-MM-dd HH:mm:ss} INFO    {message}");
        public void Log(Exception exception, string logType, string message)
        {
            string logString = $"{DateTime.Now:YY-MM-dd HH:mm:ss} {logType,-7} {message}";
            if (exception != null)
            {
                logString += $" {exception.Message} {exception.StackTrace}";
            }
            Console.WriteLine(logString);
        }
    }
}
