using PoeHUD.Poe;

namespace PoeHUD.EntitiesCache.CachedEntities
{
    public class CachedDelveVein : CachedStaticEntity
    {
        public CachedDelveVein(Entity entity, uint scanNumber) : base(entity, scanNumber)
        {
        }

        public bool IsUsed { get; set; }
    }
}
