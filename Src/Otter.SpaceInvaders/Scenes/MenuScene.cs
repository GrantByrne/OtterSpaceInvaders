using Otter.SpaceInvaders.Entities;

namespace Otter.SpaceInvaders.Scenes
{
    public class MenuScene : Scene
    {
        private const string FontPath = @".\Assets\Fonts\8-BIT WONDER.TTF";

        private const string TitleString = "Otter Space Invaders";

        private const string StartGameString = "Press Space";

        private readonly Music _backgroundMusic = new Music(@".\Assets\Audio\S31-Unexpected Trouble.ogg");

        public MenuScene()
        {
            var backgroundImage = Image.CreateRectangle(Game.Instance.Width, Game.Instance.Height, new Color("1B3AA9"));

            AddGraphic(backgroundImage);

            var gameName = new Text(TitleString, FontPath, 90);
            var pressStart = new Text(StartGameString, FontPath, 60);

            gameName.CenterOrigin();
            pressStart.CenterOrigin();

            gameName.X = Game.Instance.HalfWidth;
            gameName.Y = Game.Instance.HalfHeight - 200;

            pressStart.X = Game.Instance.HalfWidth;
            pressStart.Y = Game.Instance.HalfHeight;

            AddGraphic(gameName);
            AddGraphic(pressStart);
        }

        public override void Begin()
        {
            base.Begin();

            _backgroundMusic.Play();
        }

        public override void Update()
        {
            base.Update();

            if (Core.PlayerSesson.Controller.A.Pressed)
            {
                Core.SpaceInvadersGame.RemoveScene();
                Core.SpaceInvadersGame.AddScene(Core.InvasionScene);
            }
        }

        public override void End()
        {
            base.End();

            _backgroundMusic.Stop();
        }
    }
}