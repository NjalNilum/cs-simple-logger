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
        public Logger(string nameOfLogger="logFile.txt", int sizeOfLogFile=10)
        {
            pathToLogFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, nameOfLogger);
            sizeOfLogFileInMb = Mebibyte * sizeOfLogFile;
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
            LogRaw(message, "DEBUG", ConsoleColor.Gray);
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
        /// 
        /// If log file becomes greater than 10 MB, it will be deleted.
        /// </summary>
        private void LogRaw(string message, string kindOf, ConsoleColor color)
        {
            try
            {
                lock (loggingLock)
                {
                    if (!File.Exists(pathToLogFile) || new FileInfo(pathToLogFile).Length >= this.sizeOfLogFileInMb)
                    {
                        using (File.Create(this.pathToLogFile)) { }
                    }

                    using TextWriter sWriter = File.AppendText(this.pathToLogFile);
                    var s = $"{kindOf} - {DateTime.Now} - Thread {Environment.CurrentManagedThreadId:00} {message}";
                    sWriter.WriteLine(s);
                    Console.ForegroundColor = color;
                    Console.WriteLine(s);
                    Console.ForegroundColor = ConsoleColor.White;
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