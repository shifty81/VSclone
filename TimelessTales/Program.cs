using TimelessTales.Core;

namespace TimelessTales
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new TimelessTalesGame())
                game.Run();
        }
    }
}
