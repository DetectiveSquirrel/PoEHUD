using PoeHUD.Poe;
using PoeHUD.Poe.Components;
using PoeHUD.Poe.RemoteMemoryObjects;

namespace PoeHUD.EntitiesCache.CachedEntities
{
    public class CachedAreaTransition : CachedStaticEntity
    {
        public CachedAreaTransition(Entity entity, uint scanNumber, AreaTransition transitionComponent) : base(entity, scanNumber)
        {
            TransitionType = transitionComponent.TransitionType;
            Area = transitionComponent.WorldArea;
            Name = Area != null ? Area.Name : "NoName";
            IsSyndicateLab = entity.GetComponent<Render>().Name == "Syndicate Laboratory";
        }

        public bool IsSyndicateLab { get; }
        public AreaTransition.AreaTransitionType TransitionType { get; }
        public WorldArea Area { get; }
        public string Name { get; }

        public override string ToString()
        {
            return $"{nameof(CachedAreaTransition)}: " +
                   $"TransitionType: {TransitionType}, " +
                   $"Distance: {Position.Distance}, " +
                   $"Ignored: {Ignored}";
        }
    }
}
