using PoeHUD.EntitiesCache.CachedEntities.PositionHelpers;
using PoeHUD.Poe;

namespace PoeHUD.EntitiesCache.CachedEntities
{
    public class CachedNpcEntity : CachedEntity
    {
        public CachedNpcEntity(Entity entity, uint scanNumber) : base(entity, scanNumber)
        {
            Position = new TransitionableEntityWalkablePosition(this);
        }

        public void SetShouldUpdatePosition(bool update)
        {
            if (update)
            {
                if (!(Position is TransitionableEntityWalkablePosition))
                    Position = new TransitionableEntityWalkablePosition(this);
            }
            else if (!(Position is StaticEntityWalkablePosition))
                Position = new StaticEntityWalkablePosition(Position.GridPos, Position.WorldPos);
        }
    }
}
