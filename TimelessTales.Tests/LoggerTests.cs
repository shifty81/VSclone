using System;
using System.IO;
using Xunit;
using TimelessTales.Core;

namespace TimelessTales.Tests
{
    public class LoggerTests
    {
        // Static constructor to initialize logger once for all tests
        static LoggerTests()
        {
            Logger.Initialize();
        }

        [Fact]
        public void Logger_Initialize_CreatesLogFile()
        {
            // Assert
            string? logFilePath = Logger.GetLogFilePath();
            Assert.NotNull(logFilePath);
            Assert.True(File.Exists(logFilePath));
        }

        [Fact]
        public void Logger_Info_WritesToLog()
        {
            // Arrange
            string testMessage = $"Test info message {Guid.NewGuid()}";

            // Act
            Logger.Info(testMessage);

            // Assert
            string? logFilePath = Logger.GetLogFilePath();
            Assert.NotNull(logFilePath);
            string logContent = File.ReadAllText(logFilePath);
            Assert.Contains(testMessage, logContent);
            Assert.Contains("[INFO", logContent);
        }

        [Fact]
        public void Logger_Warning_WritesToLog()
        {
            // Arrange
            string testMessage = $"Test warning message {Guid.NewGuid()}";

            // Act
            Logger.Warning(testMessage);

            // Assert
            string? logFilePath = Logger.GetLogFilePath();
            Assert.NotNull(logFilePath);
            string logContent = File.ReadAllText(logFilePath);
            Assert.Contains(testMessage, logContent);
            Assert.Contains("[WARNING", logContent);
        }

        [Fact]
        public void Logger_Error_WritesToLog()
        {
            // Arrange
            string testMessage = $"Test error message {Guid.NewGuid()}";

            // Act
            Logger.Error(testMessage);

            // Assert
            string? logFilePath = Logger.GetLogFilePath();
            Assert.NotNull(logFilePath);
            string logContent = File.ReadAllText(logFilePath);
            Assert.Contains(testMessage, logContent);
            Assert.Contains("[ERROR", logContent);
        }

        [Fact]
        public void Logger_ErrorWithException_WritesExceptionDetails()
        {
            // Arrange
            string testMessage = $"Test exception error {Guid.NewGuid()}";
            var exception = new InvalidOperationException("Test exception");

            // Act
            Logger.Error(testMessage, exception);

            // Assert
            string? logFilePath = Logger.GetLogFilePath();
            Assert.NotNull(logFilePath);
            string logContent = File.ReadAllText(logFilePath);
            Assert.Contains(testMessage, logContent);
            Assert.Contains("Test exception", logContent);
            Assert.Contains("InvalidOperationException", logContent);
            Assert.Contains("[ERROR", logContent);
        }

        [Fact]
        public void Logger_Fatal_WritesToLog()
        {
            // Arrange
            string testMessage = $"Test fatal message {Guid.NewGuid()}";

            // Act
            Logger.Fatal(testMessage);

            // Assert
            string? logFilePath = Logger.GetLogFilePath();
            Assert.NotNull(logFilePath);
            string logContent = File.ReadAllText(logFilePath);
            Assert.Contains(testMessage, logContent);
            Assert.Contains("[FATAL", logContent);
        }

        [Fact]
        public void Logger_FatalWithException_WritesExceptionDetails()
        {
            // Arrange
            string testMessage = $"Test fatal exception {Guid.NewGuid()}";
            var exception = new Exception("Fatal test exception");

            // Act
            Logger.Fatal(testMessage, exception);

            // Assert
            string? logFilePath = Logger.GetLogFilePath();
            Assert.NotNull(logFilePath);
            string logContent = File.ReadAllText(logFilePath);
            Assert.Contains(testMessage, logContent);
            Assert.Contains("Fatal test exception", logContent);
            Assert.Contains("[FATAL", logContent);
        }
    }
}
