using Otter.SpaceInvaders.Entities.Abstract;
using Otter.SpaceInvaders.Entities.Domain;
using Otter.SpaceInvaders.Scenes;

namespace Otter.SpaceInvaders.Entities
{
    public class HeroEntity : EntityBase
    {
        private const int Width = 61;

        private const int Height = 40;

        private const int Speed = 5;

        private readonly Image _heroImage = new Image(@".\Assets\Graphics\Ship256.png");

        private readonly BoxCollider _hitBox = new BoxCollider(Width, Height, (int)Tags.Hero);

        private int _dyingCountDown;

        public bool IsDying = false;

        private const string DeathAnimationPath = @".\Assets\Graphics\PlayerDeath256.png";

        private readonly Spritemap<string> _deathSpriteMap; 

        private readonly Sound _deathSound = new Sound(@".\Assets\Audio\atari_boom.wav");

        private readonly Sound _fireSound = new Sound(@".\Assets\Audio\laser1.wav");

        public HeroEntity()
        {
            var deathTexture = new Texture(DeathAnimationPath);
            _deathSpriteMap = new Spritemap<string>(deathTexture, 256, 256);
            _deathSpriteMap.Add("Dying", new[] {0, 1}, new float[] {5, 5});
            _deathSpriteMap.Y -= 37;
            _deathSpriteMap.X -= 40;

            _heroImage.X -= 28;
            _heroImage.Y -= 20;
            
            Spawn();

            _hitBox.CenterOrigin();

            X = Game.Instance.HalfWidth;
            Y = Game.Instance.Height - 75;
        }

        public override void Update()
        {
            base.Update();

            if (!IsDying)
            {
                HandleMove();
                HandleFire();
            }
            else
            {
                if (_dyingCountDown <= 0)
                {
                    if (Core.PlayerLives < 0)
                    {
                        Core.PlayerLives = 3;
                        Core.PlayerScore = 0;
                        Game.Instance.RemoveScene();
                        Game.Instance.AddScene(new MenuScene());
                    }
                    
                    Reborn();
                }
                else
                {
                    _dyingCountDown--;
                }
            }
        }

        public void Die()
        {
            Core.PlayerLives--;
            _deathSound.Play();
            Core.InvasionScene.ClearMissiles();
            Core.InvasionScene.StopMusic();
            IsDying = true;
            _deathSpriteMap.Play("Dying");
            SetGraphic(_deathSpriteMap);
            RemoveCollider(Hitbox);
        }

        public void Reborn()
        {
            Core.InvasionScene.StartMusic();
            Spawn();
        }

        public void Spawn()
        {
            IsDying = false;

            SetGraphic(_heroImage);
            SetCollider(_hitBox);

            _dyingCountDown = 240;
        }

        private void HandleMove()
        {
            const int halfWidth = Width / 2;

            if (Core.PlayerSesson.Controller.Left.Down)
            {
                X -= Speed;
            }
            if (Core.PlayerSesson.Controller.Right.Down)
            {
                X += Speed;
            }

            X = Util.Clamp(X, halfWidth, Game.Instance.Width - halfWidth);
        }

        private void HandleFire()
        {
            if (Core.PlayerSesson.Controller.A.Pressed && Core.InvasionScene.CanFirePlayerMissile())
            {
                _fireSound.Play();
                var missile = new MissileEntity(X, Y, BulletDirection.Up);
                Core.InvasionScene.FirePlayerMissile(missile);
            }
        }
    }
}