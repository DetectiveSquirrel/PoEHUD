using PoeHUD.Poe;

namespace PoeHUD.EntitiesCache.CachedEntities
{
    public class CachedShrine : CachedStaticEntity
    {
        public CachedShrine(Entity entity, uint scanNumber) : base(entity, scanNumber)
        {
        }

        public override string ToString()
        {
            return $"[CachedShrine, HasEntity: {Entity != null}, Ignored: {Ignored} EntAddress: {Entity?.Address:x}, Metadata: {Entity?.Metadata}]";
        }
    }
}
