using Otter.SpaceInvaders.Entities.Abstract;

namespace Otter.SpaceInvaders.Entities
{
    public class LargeInvader : InvaderBase
    {
        private const int Width = 98;

        private const int Height = 65;

        public LargeInvader(float x, float y) : base(x, y)
        {
            SetUpCollider(Width, Height);
            SetUpSpriteMap(InvaderImagePaths.LargeInvader);
        }

        protected override int Points
        {
            get { return 40; }
        }
    }
}