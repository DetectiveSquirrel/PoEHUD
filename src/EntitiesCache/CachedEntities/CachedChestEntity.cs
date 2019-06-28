using PoeHUD.Poe;
using PoeHUD.Poe.Components;

namespace PoeHUD.EntitiesCache.CachedEntities
{
    public class CachedChestEntity : CachedStaticEntity
    {
        public CachedChestEntity(Entity entity, uint scanNumber) : base(entity, scanNumber)
        {
        }

        public int RunToAttemps { get; set; }

        public bool IsOpened
        {
            get
            {
                if (!IsVisible)
                    return false;

                return Entity != null && (Entity.GetComponent<Chest>().IsOpened/* || Entity.HasStat(GameStat.ChestDelayDropsUntilDaemonsFinish, out var val)*/);
            }
        }

        public Chest GetChestComponent()
        {
            if (!IsVisible)
                return null;

            return Entity.GetComponent<Chest>();
        }

        public override string ToString()
        {
            return $"[Chest, HasEntity: {Entity != null}, IsOpened: {IsOpened}, Ignored: {Ignored}, RunToAttemps: {RunToAttemps}, EntAddress: {Entity?.Address:x}, Metadata: {Entity?.Metadata}]";
        }
    }
}
