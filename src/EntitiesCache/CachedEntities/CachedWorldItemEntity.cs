using PoeHUD.Poe;
using PoeHUD.Poe.Components;

namespace PoeHUD.EntitiesCache.CachedEntities
{
    public class CachedWorldItemEntity : CachedStaticEntity
    {
        public CachedWorldItemEntity(Entity entity, uint scanNumber) : base(entity, scanNumber)
        {
        }
        
        public string Name { get; internal set; }
        public string IgnoreReason { get; internal set; }
        public Entity ItemEntity => Entity?.GetComponent<WorldItem>().ItemEntity;

        public override string ToString()
        {
            return $"[WorldItem: {Name}, HasEntity: {Entity != null}, Ignored: {Ignored}, EntAddress: {Entity?.Address:x}, Metadata: {Entity?.Metadata}, IgnoreReason: {IgnoreReason}]";
        }
    }
}
