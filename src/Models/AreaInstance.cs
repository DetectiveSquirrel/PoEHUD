using System;
using PoeHUD.EntitiesCache;
using PoeHUD.EntitiesCache.CacheControllers;
using PoeHUD.Poe.RemoteMemoryObjects;

namespace PoeHUD.Models
{
    public sealed class AreaInstance
    {
        internal readonly EntitiesAreaCache EntitiesCache;
        private DateTime _creationTime;
        private DataContainer _dataContainer;
        public DateTime TimeEntered = DateTime.Now;

        public AreaInstance(AreaTemplate area, uint hash, int realLevel)
        {
            Area = area;
            Hash = hash;
            RealLevel = realLevel;
            Name = area.Name;
            Act = area.Act;
            IsTown = area.IsTown;
            HasWaypoint = area.HasWaypoint;
            IsMap = area.IsMap;
            IsHideout = area.RawName.ToLower().Contains("hideout");
            _creationTime = DateTime.Now;
            Current = this;
            EntitiesCache = new EntitiesAreaCache();
            UpdateCachesSingleton();
        }

        public int RealLevel { get; }
        public string Name { get; }
        public int Act { get; }
        public bool IsTown { get; }
        public bool IsHideout { get; }
        public bool HasWaypoint { get; }
        public bool IsMap { get; }
        public uint Hash { get; }
        public bool IsCombatArea => !IsTown && !IsHideout;
        public AreaTemplate Area { get; }
        public static AreaInstance Current { get; private set; }
        public string DisplayName => string.Concat(Name, " (", RealLevel, ")");
        /// <summary>
        ///     For deleting old maps data from cache.
        /// </summary>
        internal TimeSpan TimeExistInCache => DateTime.Now - _creationTime;

        public override string ToString()
        {
            return $"{Name} ({RealLevel}) #{Hash}";
        }

        public static string GetTimeString(TimeSpan timeSpent)
        {
            var allsec = (int) timeSpent.TotalSeconds;
            var secs = allsec % 60;
            var mins = allsec / 60;
            var hours = mins / 60;
            mins = mins % 60;
            return string.Format(hours > 0 ? "{0}:{1:00}:{2:00}" : "{1}:{2:00}", hours, mins, secs);
        }

        internal void UsedFromCache()
        {
            Current = this;
            UpdateCachesSingleton();
            _creationTime = DateTime.Now;
        }

        private void UpdateCachesSingleton()
        {
            EntitiesAreaCache.Current = Current.EntitiesCache;
            MonstersController.Current = EntitiesAreaCache.Current.Monsters;
        }

        #region Data Container

        public T GetDataContainer<T>() where T : class
        {
            return _dataContainer?.GetDataContainer<T>();
        }

        public T GetOrCreateDataContainer<T>() where T : class, new()
        {
            if (_dataContainer == null)
                _dataContainer = new DataContainer(); //not sure, maybe save some memory?

            return _dataContainer.GetOrCreateDataContainer<T>();
        }

        public void AddDataContainer<T>(T container) where T : class
        {
            if (_dataContainer == null)
                _dataContainer = new DataContainer(); //not sure, maybe save some memory?

            _dataContainer.AddDataContainer(container);
        }

        #endregion
    }
}
