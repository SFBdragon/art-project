using System;

namespace ArtProject
{
    public static class Program
    {
        [STAThread]
        private static void Main()
        {
            using (var game = new GameLoop())
                game.Run();
        }
    }
}
