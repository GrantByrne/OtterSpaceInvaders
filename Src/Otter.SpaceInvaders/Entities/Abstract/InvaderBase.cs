using Otter.SpaceInvaders.Entities.Domain;

namespace Otter.SpaceInvaders.Entities.Abstract
{
    public abstract class InvaderBase : EntityBase
    {
        private const float FireChance = .01f;

        protected Spritemap<string> Spritemap;

        protected BoxCollider HitBox;

        private bool _isDying;

        private int _dyingCountDown = 7;

        private const string ImagePath = @".\Assets\Graphics\Explosion256.png";

        private readonly Image _explosion = new Image(ImagePath);

        private readonly Sound _fireSound = new Sound(@".\Assets\Audio\laser3.wav");

        protected InvaderBase(float x, float y)
        {
            _explosion.X -= 15;

            X = x;
            Y = y;
        }

        public override void Update()
        {
            base.Update();

            if (_isDying)
            {
                if (_dyingCountDown <= 0)
                {
                    Core.InvasionScene.KillInvader(this);
                    Core.PlayerScore += Points;
                }
                else
                {
                    _dyingCountDown--;
                }
            }
            else if (Rand.Chance(FireChance) && !Core.InvasionScene.Hero.IsDying)
            {
                _fireSound.Play();
                var missile = new MissileEntity(X, Y, BulletDirection.Down);
                Core.InvasionScene.AddAlienMissile(missile);
            }
        }

        public void Die()
        {
            if (!_isDying)
            {
                _isDying = true;
                SetGraphic(_explosion);
                RemoveCollider(HitBox);
            }
        }

        protected void SetUpSpriteMap(string path)
        {
            var spriteMap = new Spritemap<string>(path, 256, 256);
            spriteMap.Add("Invading", new[] { 0, 1 }, new[] { 20f, 20f });
            spriteMap.Play("Invading");
            SetGraphic(spriteMap);
        }

        protected void SetUpCollider(int width, int height)
        {
            HitBox = new BoxCollider(width, height, (int)Tags.Invader);
            SetCollider(HitBox);
        }

        protected abstract int Points { get; }
    }
}