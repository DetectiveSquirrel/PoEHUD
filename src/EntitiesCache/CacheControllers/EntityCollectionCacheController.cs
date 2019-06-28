using System;
using System.Collections.Generic;
using PoeHUD.EntitiesCache.CachedEntities;
using PoeHUD.Plugins;

namespace PoeHUD.EntitiesCache.CacheControllers
{
    public class EntityCollectionCacheController<T> : IEntityAppearanceFeedback where T : CachedEntity
    {
        protected readonly List<T> _allEntities = new List<T>();

        internal EntityCollectionCacheController(EntitiesAreaCache entitiesCache)
        {
            VisibleEntities = new EntityCollectionFilter<T>(_allEntities, x => x.IsVisible);
            OutOfRangeEntities = new EntityCollectionFilter<T>(_allEntities, x => !x.IsVisible);
            AllEntities = _allEntities.AsReadOnly();
            entitiesCache.OnEntitiesClear += _allEntities.Clear;
        }

        public IEnumerable<T> VisibleEntities { get; }
        public IEnumerable<T> OutOfRangeEntities { get; }
        public IReadOnlyCollection<T> AllEntities { get; }

        void IEntityAppearanceFeedback.EntityAppear(CachedEntity cachedEntity)
        {
            var arg = new EntityAddedArgs<CachedEntity>(cachedEntity, false);
            SafeEventCall(OnEntityAdded, arg, nameof(OnEntityAdded));
        }

        public event Action<EntityAddedArgs<T>> OnEntityAdded = delegate { };
        public event Action<EntityRemovedArgs<T>> OnEntityRemoved = delegate { };

        internal virtual void CheckVisibility(uint scanNumber)
        {
            foreach (var cachedEntity in _allEntities)
            {
                //if scan number is not equal this mean that this entity was not found while scanning, so it is out of range
                if (cachedEntity.ScanNumber != scanNumber)
                {
                    cachedEntity.Entity = null;
                    cachedEntity.IsVisible = false;
                    cachedEntity.OnDisappear();
                    var arg = new EntityRemovedArgs<T>(cachedEntity, destroyed: false);
                    SafeEventCall(OnEntityRemoved, arg, nameof(OnEntityRemoved));
                }
            }
        }

        internal virtual void AddNewEntity(T cachedEntity)
        {
            cachedEntity.EntityControllerFeedback = this;
            _allEntities.Add(cachedEntity);
            var arg = new EntityAddedArgs<CachedEntity>(cachedEntity, isNewEntity: true);
            SafeEventCall(OnEntityAdded, arg, nameof(OnEntityAdded));
        }

        protected internal void EntityDestroyed(T cachedEntity)
        {
            var arg = new EntityRemovedArgs<T>(cachedEntity, destroyed: true);
            SafeEventCall(OnEntityRemoved, arg, nameof(OnEntityRemoved));
            _allEntities.Remove(cachedEntity);
            EntitiesAreaCache.Current.AllEntities.Remove(cachedEntity.Id);
        }

        private void SafeEventCall(MulticastDelegate func, object arg, string eventName)
        {
            foreach (var subscriber in func.GetInvocationList())
            {
                try
                {
                    subscriber.DynamicInvoke(arg);
                }
                catch (Exception e)
                {
                    BasePlugin.LogError($"Error while calling function subscribed to event {eventName}" +
                                        $" in object {subscriber.Target}, " +
                                        $"method: {subscriber.Method.Name}: {e}", 7);
                }
            }
        }
    }
}
