using PoeHUD.EntitiesCache.CachedEntities;

namespace PoeHUD.EntitiesCache.CacheControllers
{
    public interface IEntityAppearanceFeedback
    {
        void EntityAppear(CachedEntity cachedEntity);
    }
}