using Otter.SpaceInvaders.Entities.Abstract;
using Otter.SpaceInvaders.Entities.Domain;
using Otter.SpaceInvaders.Scenes;

namespace Otter.SpaceInvaders.Entities
{
    public class MissileEntity : EntityBase
    {
        private readonly BulletDirection _bulletDirection;

        private const int Speed = 5;

        private readonly BoxCollider _hitBox = new BoxCollider(16, 32, (int)Tags.Missle);

        public MissileEntity(float x, float y, BulletDirection bulletDirection)
        {
            _bulletDirection = bulletDirection;

            var projectilePath = (bulletDirection == BulletDirection.Up)
                ? @".\Assets\Graphics\PlayerProjectile256.png"
                : @".\Assets\Graphics\AlienProjectile256.png";
            var animation = new Spritemap<string>(projectilePath, 256, 256);

            animation.Add("firing", new[] { 0, 1 }, new[] { 5f, 5f });

            animation.Play("firing");
            SetGraphic(animation);
            SetCollider(_hitBox);

            animation.OriginX = 0;
            animation.OriginY = 0;

            X = x;
            Y = y;
        }

        public override void Update()
        {
            base.Update();

            switch (_bulletDirection)
            {
                case BulletDirection.Up:
                    Y -= Speed;
                    break;
                case BulletDirection.Down:
                    Y += Speed;
                    break;
            }

            var cPlayer = Collider.Collide(X, Y, (int)Tags.Hero);
            if (cPlayer != null && _bulletDirection == BulletDirection.Down)
            {
                Core.InvasionScene.RemoveAlienMissile(this);
                Core.InvasionScene.Hero.Die();
            }

            var cInvader = Collider.Collide(X, Y, (int)Tags.Invader);
            if (cInvader != null && _bulletDirection == BulletDirection.Up)
            {
                var invader = (InvaderBase)cInvader.Entity;
                invader.Die();
                Core.InvasionScene.RemovePlayerMissile();
            }
        }
    }
}