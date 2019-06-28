using PoeHUD.EntitiesCache.CachedEntities;

namespace PoeHUD.EntitiesCache.CacheControllers
{
    public class EntityRemovedArgs<T> where T : CachedEntity
    {
        public EntityRemovedArgs(T entity, bool destroyed)
        {
            Entity = entity;
            Destroyed = destroyed;
        }

        public bool Destroyed { get; }
        public T Entity { get; }
    }
}