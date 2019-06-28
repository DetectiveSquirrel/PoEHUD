using PoeHUD.EntitiesCache.CachedEntities.PositionHelpers;
using PoeHUD.EntitiesCache.Extensions;
using PoeHUD.Models.Attributes;
using PoeHUD.Models.Enums;
using PoeHUD.Poe;
using PoeHUD.Poe.Components;

namespace PoeHUD.EntitiesCache.CachedEntities
{
    public class CachedMonsterEntity : CachedEntity
    {
        public readonly MonsterRarity Rarity;
        private string _name;

        public CachedMonsterEntity(Entity entity, uint scanNumber) : base(entity, scanNumber)
        {
            Position = new TransitionableEntityWalkablePosition(this);
            Rarity = entity.GetComponent<ObjectMagicProperties>().Rarity;
            IsLegion = Metadata.Contains("LegionLeague");
            IsAlly = !entity.IsHostile;
            IsDead = !entity.IsAlive;
        }

        [LegionLeague]
        public bool IsLegion { get; }
        public string InitialFilterOutReason { get; internal set; } //For debug
        public string PriorityFilterOutReason { get; internal set; } //For debug
        /// <summary>
        /// Entity was destroyed/killed.
        /// </summary>
        public bool IsDead { get; internal set; }
        public bool IsAlive => !IsDead;//obviously, right?
        public bool IsAlly { get; internal set; }
        public bool IsHostile => !IsAlly;
        public bool FilterOutCannotDie =>
            Entity.CannotDieAura && Rarity != MonsterRarity.Rare && Metadata != "Metadata/Monsters/Totems/TotemAlliesCannotDie";
        public string Name => _name ?? (_name = Entity.GetMonsterName());

        public override string ToString()
        {
            return $"Monster: {Name}, " +
                   $"HasEntity: {Entity != null}, " +
                   $"IsActive: {Entity?.IsActive}, " +
                   $"Ignored: {Ignored}, " +
                   $"EntAddress: {Entity?.Address:x}, " +
                   $"FilterOutReason: {InitialFilterOutReason}, " +
                   $"PriorityFilterOutReason: {PriorityFilterOutReason}, " +
                   $"Dist: {Position.Distance}" +
                   $"Addr: {Entity?.Address:x}";
        }
    }
}
