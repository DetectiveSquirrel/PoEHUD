using PoeHUD.EntitiesCache.CachedEntities.PositionHelpers;
using PoeHUD.EntitiesCache.Extensions;
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
        }

        public string InitialFilterOutReason { get; internal set; } //For debug
        public string PriorityFilterOutReason { get; internal set; } //For debug
        /// <summary>
        ///     Entity was destroyed/killed.
        /// </summary>
        public bool IsDead { get; set; }
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
