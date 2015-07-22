using Otter.SpaceInvaders.Entities.Abstract;

namespace Otter.SpaceInvaders.Entities
{
    public class MediumInvader : InvaderBase
    {
        private const int Width = 90;

        private const int Height = 65;

        private const int Xoffset = 5;

        public MediumInvader(float x, float y) : base(x, y)
        {
            X += Xoffset;
            SetUpCollider(Width, Height);
            SetUpSpriteMap(InvaderImagePaths.MediumInvader);
        }

        protected override int Points
        {
            get { return 20; }
        }
    }
}