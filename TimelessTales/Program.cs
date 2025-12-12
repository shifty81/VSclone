using System;
using TimelessTales.Core;

namespace TimelessTales
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            // Initialize logging system
            Logger.Initialize();
            Logger.Info("=== Timeless Tales Starting ===");
            Logger.Info($"Application Base Directory: {AppDomain.CurrentDomain.BaseDirectory}");
            Logger.Info($"Operating System: {Environment.OSVersion}");
            Logger.Info($".NET Version: {Environment.Version}");

            try
            {
                Logger.Info("Creating game instance...");
                using (var game = new TimelessTalesGame())
                {
                    Logger.Info("Starting game loop...");
                    game.Run();
                    Logger.Info("Game loop exited normally");
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal("Fatal error in main game loop", ex);
                Console.WriteLine("\n=== FATAL ERROR ===");
                Console.WriteLine($"The game has crashed. Please check the log file at:");
                Console.WriteLine($"{Logger.GetLogFilePath()}");
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
                Environment.Exit(1);
            }
            finally
            {
                Logger.Info("=== Timeless Tales Shutdown ===");
            }
        }
    }
}
