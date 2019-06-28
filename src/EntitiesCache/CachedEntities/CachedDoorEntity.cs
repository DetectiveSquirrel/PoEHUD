using PoeHUD.EntitiesCache.Extensions;
using PoeHUD.Poe;
using PoeHUD.Poe.Components;

namespace PoeHUD.EntitiesCache.CachedEntities
{
    public class CachedDoorEntity : CachedStaticEntity
    {
        public CachedDoorEntity(Entity entity, uint scanNumber) : base(entity, scanNumber)
        {
            IsLockingDoor = entity.GetComponent<Render>().Name == LocalizationConstants.LOCKED_DOOR;
        }

        public bool IsOpened => IsVisible && !GetTriggerableBlockageComponent().IsClosed;

        public TriggerableBlockage GetTriggerableBlockageComponent()
        {
            if (!IsVisible || Entity.Address == 0)
            {
                return null;
            }

            return Entity.GetComponent<TriggerableBlockage>();
        }

        public bool TriggerableBlockageProcessed { get; set; }
        public bool ObstacleAddedOnServer { get; set; }
        public bool IsLockingDoor { get; set; }

        public override string ToString()
        {
            return $"[Chest, HasEntity: {Entity != null}, IsOpened: {IsOpened}, IsLockingDoor: {IsLockingDoor}, ObstacleAddedOnServer: {ObstacleAddedOnServer}, EntAddress: {Entity?.Address:x}, Metadata: {Entity?.Metadata}]";
        }
    }
}
