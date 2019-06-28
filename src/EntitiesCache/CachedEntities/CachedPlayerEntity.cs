using PoeHUD.EntitiesCache.CachedEntities.PositionHelpers;
using PoeHUD.Models.Enums;
using PoeHUD.Poe;
using PoeHUD.Poe.Components;

namespace PoeHUD.EntitiesCache.CachedEntities
{
    public class CachedPlayerEntity : CachedEntity
    {
        public readonly MonsterRarity Rarity;
        private string _name;

        public CachedPlayerEntity(Entity entity, uint scanNumber) : base(entity, scanNumber)
        {
            Position = new TransitionableEntityWalkablePosition(this);
            Rarity = entity.GetComponent<ObjectMagicProperties>().Rarity;
        }
        public string Name { get; }

        public override string ToString()
        {
            return $"Monster: {Name}, " +
                   $"IsActive: {Entity?.IsActive}, " +
                   $"Ignored: {Ignored}, " +
                   $"Dist: {Position.Distance}";
        }
    }
}
