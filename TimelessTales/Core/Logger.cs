using System;
using System.IO;
using System.Text;

namespace TimelessTales.Core
{
    /// <summary>
    /// Logging system for capturing errors, warnings, and info during build and runtime
    /// </summary>
    public static class Logger
    {
        private static readonly object _lockObject = new object();
        private static string? _logFilePath;
        private static bool _isInitialized = false;

        public enum LogLevel
        {
            Info,
            Warning,
            Error,
            Fatal
        }

        /// <summary>
        /// Initialize the logger with a log file path
        /// </summary>
        public static void Initialize()
        {
            try
            {
                // Create logs directory if it doesn't exist
                string logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                Directory.CreateDirectory(logsDir);

                // Create log file with timestamp
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                _logFilePath = Path.Combine(logsDir, $"timeless_tales_{timestamp}.log");

                // Write header
                lock (_lockObject)
                {
                    File.WriteAllText(_logFilePath, $"=== Timeless Tales Log - {DateTime.Now:yyyy-MM-dd HH:mm:ss} ==={Environment.NewLine}{Environment.NewLine}");
                }

                _isInitialized = true;
                Info("Logger initialized successfully");
            }
            catch (Exception ex)
            {
                // If we can't initialize the logger, write to console
                Console.WriteLine($"Failed to initialize logger: {ex.Message}");
            }
        }

        /// <summary>
        /// Log an informational message
        /// </summary>
        public static void Info(string message)
        {
            Log(LogLevel.Info, message);
        }

        /// <summary>
        /// Log a warning message
        /// </summary>
        public static void Warning(string message)
        {
            Log(LogLevel.Warning, message);
        }

        /// <summary>
        /// Log an error message
        /// </summary>
        public static void Error(string message)
        {
            Log(LogLevel.Error, message);
        }

        /// <summary>
        /// Log an error with exception details
        /// </summary>
        public static void Error(string message, Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(message);
            sb.AppendLine($"Exception Type: {ex.GetType().Name}");
            sb.AppendLine($"Message: {ex.Message}");
            sb.AppendLine($"Stack Trace: {ex.StackTrace}");
            
            if (ex.InnerException != null)
            {
                sb.AppendLine($"Inner Exception: {ex.InnerException.Message}");
                sb.AppendLine($"Inner Stack Trace: {ex.InnerException.StackTrace}");
            }

            Log(LogLevel.Error, sb.ToString());
        }

        /// <summary>
        /// Log a fatal error (typically before crash)
        /// </summary>
        public static void Fatal(string message)
        {
            Log(LogLevel.Fatal, message);
        }

        /// <summary>
        /// Log a fatal error with exception details
        /// </summary>
        public static void Fatal(string message, Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(message);
            sb.AppendLine($"Exception Type: {ex.GetType().Name}");
            sb.AppendLine($"Message: {ex.Message}");
            sb.AppendLine($"Stack Trace: {ex.StackTrace}");
            
            if (ex.InnerException != null)
            {
                sb.AppendLine($"Inner Exception: {ex.InnerException.Message}");
                sb.AppendLine($"Inner Stack Trace: {ex.InnerException.StackTrace}");
            }

            Log(LogLevel.Fatal, sb.ToString());
        }

        /// <summary>
        /// Core logging method
        /// </summary>
        private static void Log(LogLevel level, string message)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string levelStr = level.ToString().ToUpper().PadRight(7);
                string logEntry = $"[{timestamp}] [{levelStr}] {message}{Environment.NewLine}";

                // Always write to console
                ConsoleColor originalColor = Console.ForegroundColor;
                Console.ForegroundColor = level switch
                {
                    LogLevel.Info => ConsoleColor.White,
                    LogLevel.Warning => ConsoleColor.Yellow,
                    LogLevel.Error => ConsoleColor.Red,
                    LogLevel.Fatal => ConsoleColor.DarkRed,
                    _ => ConsoleColor.White
                };
                Console.Write(logEntry);
                Console.ForegroundColor = originalColor;

                // Write to file if initialized
                if (_isInitialized && _logFilePath != null)
                {
                    lock (_lockObject)
                    {
                        File.AppendAllText(_logFilePath, logEntry);
                    }
                }
            }
            catch (Exception ex)
            {
                // Last resort - write to console if logging fails
                Console.WriteLine($"Logger failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Get the current log file path
        /// </summary>
        public static string? GetLogFilePath()
        {
            return _logFilePath;
        }
    }
}
