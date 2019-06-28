namespace PoeHUD.EntitiesCache
{
    public class CachedEntitiesDataContainerController//TODO: Delete me
    {
        /// <summary>
        /// The uniq identifier for each plugin
        /// </summary>
        private static int UniqId;

        /// <summary>
        /// The current active plugin Id
        /// </summary>
        private static int ActivePluginId;

        private static int GetUniqIdForPlugin()
        {
            return UniqId++;
        }
    }
}
