using PoeHUD.EntitiesCache.CachedEntities.PositionHelpers;
using PoeHUD.Poe;
using PoeHUD.Poe.Components;
using PoeHUD.Poe.RemoteMemoryObjects;

namespace PoeHUD.EntitiesCache.CachedEntities
{
    public class CachedPortalEntity : CachedEntity
    {
        public CachedPortalEntity(Entity entity, uint scanNumber, Portal portalComponent) : base(entity, scanNumber)
        {
            Position = new TransitionableEntityWalkablePosition(this);
            IsPlayerPortal = /*Entity.IsTargetable &&*/
                             (
                                 Metadata == "Metadata/MiscellaneousObjects/PlayerPortal" ||
                                 Metadata == "Metadata/MiscellaneousObjects/MapReturnPortal"
                             );

            Area = portalComponent.Area;
        }

        public bool IsPlayerPortal { get; }
        public WorldArea Area { get; }

        public override string ToString()
        {
            return $"[StaticEntity, HasEntity: {Entity != null}, IsPlayerPortal: {IsPlayerPortal}, EntAddress: {Entity?.Address:x}, Metadata: {Entity?.Metadata}]";
        }
    }
}
