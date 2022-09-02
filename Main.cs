using System;
using Gorillas_3D;

namespace Gorillas_3D_Windows
{
    // The main class.
    public static class MainEntry
    {
        // The main entry point for the application.
        [STAThread]
        static void Main()
        {
            GorillasGame game = new GorillasGame();
            game.Run();
        }
    }
}
