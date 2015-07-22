using Otter.SpaceInvaders.Entities.Abstract;

namespace Otter.SpaceInvaders.Entities
{
    public class SmallInvader : InvaderBase
    {
        private const int Width = 65;

        private const int Height = 65;

        private const int Xoffset = 17;

        public SmallInvader(float x, float y) : base(x, y)
        {
            X += Xoffset;
            SetUpCollider(Width, Height);
            SetUpSpriteMap(InvaderImagePaths.SmallInvader);
        }

        protected override int Points
        {
            get { return 10; }
        }
    }
}