using System;
using System.Collections.Generic;

namespace PoeHUD.EntitiesCache.CacheControllers
{
    public class DataContainer
    {
        private readonly Dictionary<Guid, object> _dataContainerDictionary = new Dictionary<Guid, object>();
        public T GetDataContainer<T>() where T : class
        {
            //perfectly instead of GetHashCode from typeof(T), which takes unknown amount of time
            //I probably tried to use address of typeof(T) using unsafe..
            //It will be fast, but Im not 100% sure that address will be stable (fx in case loading and unloading assemblies)

            //UPD. I'll use typeof(T).Guid.GetHashCode() which seems is the fastest way (no InternalHelpers for calculating hashcode of Type, just fast bit operations)
            //and seems this is the same as Dictionary<GUID,xxx)... I'll go this way
            _dataContainerDictionary.TryGetValue(typeof(T).GUID, out var container);

            if (container is T typedContainer) //to avoid exceptions while direct cast
                return typedContainer;

            return null;
        }

        public T GetOrCreateDataContainer<T>() where T : class, new()
        {
            var container = GetDataContainer<T>();

            if (container == null)
            {
                container = new T();
                AddDataContainer(container);
            }

            return container;
        }

        public void AddDataContainer<T>(T container) where T : class
        {
            _dataContainerDictionary.Add(typeof(T).GUID, container);
        }
    }
}
