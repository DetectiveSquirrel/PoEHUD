using PoeHUD.EntitiesCache.CachedEntities.PositionHelpers;
using PoeHUD.Poe;

namespace PoeHUD.EntitiesCache.CachedEntities
{
    public class CachedStaticEntity : CachedEntity
    {
        public CachedStaticEntity(Entity entity, uint scanNumber)
            : base(entity, scanNumber, new StaticEntityWalkablePosition(entity))
        {
        }

        public override string ToString()
        {
            return $"[{GetType().Name}, HasEntity: {Entity != null}, EntAddress: {Entity?.Address:x}, Metadata: {Entity?.Metadata}]";
        }
    }
}