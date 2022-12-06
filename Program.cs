using System;

namespace Program {
    public class Program {
        public static void Main() {

            Console.WriteLine("Starting game...");

            var game = new SimpleRayCast.RayCastGame();
            game.Run();
            game.Dispose();

            Console.WriteLine("Game has been closed. Press [ENTER] to exit");
            Console.ReadLine();
            Environment.Exit(0);
        }
    }
}
//var game = new SimpleRayCast.RayCastGame();
//game.Run();
//Console.WriteLine("");
//game = null;
//Console.ReadLine();