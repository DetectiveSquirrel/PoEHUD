using PoeHUD.Models;
using PoeHUD.Poe;
using SharpDX;

namespace PoeHUD.EntitiesCache.CachedEntities.PositionHelpers
{
    public class StaticEntityWalkablePosition : WalkablePosition
    {
        public StaticEntityWalkablePosition(Entity entity) : base(entity.PositionedComp.GridPos, entity.Pos)
        {
        }

        public StaticEntityWalkablePosition(EntityWrapper entity) : base(entity.PositionedComp.GridPos, entity.Pos)
        {
        }

        public StaticEntityWalkablePosition(Vector2 gridPos, Vector3 worldPos) : base(gridPos, worldPos)
        {
        }
    }
}
