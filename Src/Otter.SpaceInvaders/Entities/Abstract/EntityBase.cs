namespace Otter.SpaceInvaders.Entities.Abstract
{
    public abstract class EntityBase : Entity
    {
        public override void Render()
        {
            base.Render();

            if (GemDebug.ShowHitboxes && Collider != null)
            {
                Collider.Render();
            }
        }
    }
}