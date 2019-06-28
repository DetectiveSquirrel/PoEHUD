using SharpDX;

namespace PoeHUD.EntitiesCache.CachedEntities.PositionHelpers
{
    public class WalkablePosition
    {
        internal WalkablePosition()
        {
        }

        internal WalkablePosition(Vector2 gridPos, Vector3 worldPos)
        {
            GridPos = gridPos;
            WorldPos = worldPos;
        }

        public virtual Vector2 GridPos { get; }
        public virtual Vector3 WorldPos { get; }
        public float Distance => Vector2.Distance(GridPos, PlayerInfo.GridPos);
    }
}
