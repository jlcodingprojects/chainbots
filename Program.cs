using System;

namespace Chainbots
{
    /// <summary>
    /// The main entry point for the Chainbots application.
    /// </summary>
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new Game1())
                game.Run();
        }
    }
}

