using SharpDX;

namespace PoeHUD.EntitiesCache.CachedEntities.PositionHelpers
{
    public class TransitionableEntityWalkablePosition : WalkablePosition
    {
        private readonly IEntityHolder _entityHolder;
        private Vector2 _cachedGridPos;
        private Vector3 _cachedWorldPos;

        public TransitionableEntityWalkablePosition(IEntityHolder entityHolder)
        {
            _entityHolder = entityHolder;
            var entity = _entityHolder.GetEntity();
            _cachedGridPos = entity.PositionedComp.GridPos;
            _cachedWorldPos = entity.Pos;
        }

        public override Vector2 GridPos
        {
            get
            {
                var entity = _entityHolder.GetEntity();

                if (entity != null)
                    _cachedGridPos = entity.PositionedComp.GridPos;

                return _cachedGridPos;
            }
        }

        public override Vector3 WorldPos
        {
            get
            {
                var entity = _entityHolder.GetEntity();

                if (entity != null)
                    _cachedWorldPos = entity.Pos;

                return _cachedWorldPos;
            }
        }
    }
}
