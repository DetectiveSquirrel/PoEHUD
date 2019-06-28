using PoeHUD.EntitiesCache.CachedEntities.PositionHelpers;
using PoeHUD.Poe;

namespace PoeHUD.EntitiesCache.CachedEntities
{
    public class CachedDangerEntity : CachedEntity
    {
        /// <summary>
        /// This is something like explosion radius (of volatile, etc)
        /// </summary>
        public float Radius { get; }
        private readonly bool _isMonster;

        public CachedDangerEntity(Entity entity, uint scanNumber, bool trackPosition, bool isMonster, float radius) : base(entity, scanNumber)
        {
            Radius = radius;
            _isMonster = isMonster;

            if (trackPosition)
            {
                Position = new TransitionableEntityWalkablePosition(this);
            }
            else
            {
                Position = new StaticEntityWalkablePosition(entity); 
            }
        }

        public override bool ShouldRemove()
        {
            return !IsVisible ||//TODO: I'm not sure about this !IsVisible. Should works fine without this
                   Entity == null ||
                   Entity.Address == 0 ||
                   !Entity.IsValid || (
                       _isMonster && !Entity.IsAlive);//this is for volatile magma.. it shouldn't be IsAlive, they just disappear after death
        }
    }
}
