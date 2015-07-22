using Otter.SpaceInvaders.Scenes;

namespace Otter.SpaceInvaders
{
    // TODO - Set up the game to be resolution independent
    // TODO - Set up the game to be run in fullscreen
    // TODO - A logging framework to the application
    // TODO - Integration an IOC container
    // TODO - Set up some sprites
    // TODO - Add in some background music
    // TODO - Add in some sound effects
    class Program
    {
        static void Main(string[] args)
        {
            var game = new Game("Otter Space Invaders", 1920, 1080);
            Core.SpaceInvadersGame = game;

            Core.PlayerSesson = game.AddSession("Player");

            Core.PlayerSesson.Controller.Left.AddKey(Key.Left);
            Core.PlayerSesson.Controller.Right.AddKey(Key.Right);
            Core.PlayerSesson.Controller.A.AddKey(Key.Space);

            Debugger.Instance.RegisterCommand("showhitboxes", "Toggles the visibility of hitboxes.", GemDebug.CmdHitboxes, CommandType.Bool);

            Core.InvasionScene = new InvasionScene();
            game.FirstScene = new MenuScene();

            game.Start();
        }
    }
}
