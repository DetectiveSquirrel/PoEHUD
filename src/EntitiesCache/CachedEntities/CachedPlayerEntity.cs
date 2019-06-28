using PoeHUD.EntitiesCache.CachedEntities.PositionHelpers;
using PoeHUD.Poe;
using PoeHUD.Poe.Components;

namespace PoeHUD.EntitiesCache.CachedEntities
{
    public class CachedPlayerEntity : CachedEntity
    {
        public CachedPlayerEntity(Entity entity, uint scanNumber, Player player) : base(entity, scanNumber)
        {
            Position = new TransitionableEntityWalkablePosition(this);
            PlayerComponent = player;
            PlayerName = PlayerComponent.PlayerName;
        }

        public string PlayerName { get; }
        public Player PlayerComponent { get; private set; }

        /// Called once when entity back to visible range
        protected override void OnAppear()
        {
            PlayerComponent = Entity.GetComponent<Player>();
        }

        /// Called once when player go out of visible range
        protected internal override void OnDisappear()
        {
            PlayerComponent = null;
        }

        public override string ToString()
        {
            return $"PlayerName: {PlayerName}, " +
                   $"IsVisible: {IsVisible}, " +
                   $"Dist: {Position.Distance}";
        }
    }
}
