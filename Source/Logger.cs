using System.Runtime.CompilerServices;

namespace SimpleLogger
{
    public class Logger
    {
        private readonly string pathToLogFile;
        private readonly object loggingLock = new();
        private readonly long sizeOfLogFileInMb;
        private const int Mebibyte = 1048576;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="nameOfLoggerFile">Name of your logger file</param>
        /// <param name="sizeOfLogFile">Size of log file in MB</param>
        public Logger(string nameOfLoggerFile = "logFile.log", int sizeOfLogFile = 10)
        {
            var pathToLogDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
            if (!Directory.Exists(pathToLogDir))
            {
                Directory.CreateDirectory(pathToLogDir);
            }
            this.pathToLogFile = Path.Combine(pathToLogDir, nameOfLoggerFile);
            this.sizeOfLogFileInMb = Mebibyte * sizeOfLogFile;
        }

        // self-explanatory
        public void LogInfo(string message,
                            [CallerMemberName] string callerName = "",
                            [CallerFilePath] string callerFilePath = "",
                            [CallerLineNumber] int lineNumber = 0)
        {
            message = BuildCallerInfo(callerName, callerFilePath, lineNumber) + message;
            LogRaw(message, "INFO ", ConsoleColor.White);
        }

        // self-explanatory
        public void LogDebug(string message,
                             [CallerMemberName] string callerName = "",
                             [CallerFilePath] string callerFilePath = "",
                             [CallerLineNumber] int lineNumber = 0)
        {
            message = BuildCallerInfo(callerName, callerFilePath, lineNumber) + message;
            LogRaw(message, "DEBUG", ConsoleColor.DarkGray);
        }

        // self-explanatory
        public void LogWarning(string message,
                               [CallerMemberName] string callerName = "",
                               [CallerFilePath] string callerFilePath = "",
                               [CallerLineNumber] int lineNumber = 0)
        {
            message = BuildCallerInfo(callerName, callerFilePath, lineNumber) + message;
            LogRaw(message, "WARN ", ConsoleColor.Yellow);
        }

        // self-explanatory
        public void LogError(string message,
                             [CallerMemberName] string callerName = "",
                             [CallerFilePath] string callerFilePath = "",
                             [CallerLineNumber] int lineNumber = 0)
        {
            message = BuildCallerInfo(callerName, callerFilePath, lineNumber) + message;
            LogRaw(message, "ERROR", ConsoleColor.Red);
        }

        /// <summary>
        /// Builds Info within CallerInformations.
        /// </summary>
        /// <returns></returns>
        private string BuildCallerInfo(string callerName, string callerFilePath, int lineNumber)
        {
            try
            {
                return $"in '{Path.GetFileNameWithoutExtension(callerFilePath)}.{callerName}()' at Line: '{lineNumber}' --- ";
            }
            catch (Exception e)
            {
                return $" -- N/A -- {Environment.NewLine} {e.Message}  {Environment.NewLine}";
            }
        }

        /// <summary>
        /// Self-explanatory.
        /// If log file becomes greater than 10 MB, it will be deleted.
        /// Unfortunately, no RollingFileAppender is currently implemented. So there is only one log file, which is overwritten
        /// again from a size of 10 MB (default).
        /// </summary>
        private void LogRaw(string message, string kindOf, ConsoleColor color)
        {
            try
            {
                lock (this.loggingLock)
                {
                    if (!File.Exists(this.pathToLogFile) || new FileInfo(this.pathToLogFile).Length >= this.sizeOfLogFileInMb)
                    {
                        // FielCreate overwrites the existing file when the specified size is reached.
                        using (File.Create(this.pathToLogFile))
                        {
                        }
                    }

                    // Yes, sWriter and the using block are indeed good and necessary to ensure that the filestream is closed cleanly
                    using TextWriter sWriter = File.AppendText(this.pathToLogFile);
                    var s = $"{kindOf} - {DateTime.Now} - Thread {Environment.CurrentManagedThreadId:00} {message}";
                    sWriter.WriteLine(s);
                    var colorB4 = Console.ForegroundColor;
                    Console.ForegroundColor = color;
                    Console.WriteLine(s);
                    Console.ForegroundColor = colorB4;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
#if Windows
                if (!EventLog.SourceExists("KeepAliveMyDriveService"))
                    EventLog.CreateEventSource("KeepAliveMyDriveService", "Application");

                EventLog.WriteEntry(
                    "KeepAliveMyDriveService",
                    $"Error when accessing on logfile.{Environment.NewLine}Exception Message: {ex.Message}",
                    EventLogEntryType.Error,
                    166,
                    666);
#endif
            }
        }
    }
}