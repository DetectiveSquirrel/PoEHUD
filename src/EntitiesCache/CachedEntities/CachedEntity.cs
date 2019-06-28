using PoeHUD.EntitiesCache.CacheControllers;
using PoeHUD.EntitiesCache.CachedEntities.PositionHelpers;
using PoeHUD.Poe;

namespace PoeHUD.EntitiesCache.CachedEntities
{
    public abstract class CachedEntity : IEntityHolder
    {
        private DataContainer _dataContainer;
        internal uint Id;
        internal uint ScanNumber;

        internal CachedEntity(Entity entity, uint scanNumber, WalkablePosition position)
        {
            Id = entity.Id;
            Entity = entity;
            ScanNumber = scanNumber;
            Position = position;
            Metadata = entity.Metadata;
        }

        internal CachedEntity(Entity entity, uint scanNumber)
        {
            Id = entity.Id;
            Entity = entity;
            ScanNumber = scanNumber;
            Metadata = entity.Metadata;
        }

        internal IEntityAppearanceFeedback EntityControllerFeedback { get; set; }
        public string Metadata { get; }
        public WalkablePosition Position { get; protected set; }
        /// <summary>
        ///     if this value is true this class will not be added to some special collections (monsters, chests, etc)
        ///     ...but this class will be in AllEntities collection
        /// </summary>
        public bool IsVisible { get; internal set; }
        public bool Ignored { get; internal set; }
        public Entity Entity { get; internal set; }

        public Entity GetEntity()
        {
            return Entity;
        }

        public virtual bool ShouldRemove()
        {
            return false;
        }

        internal void Appear(Entity entity, uint scanNumber)
        {
            Entity = entity;
            ScanNumber = scanNumber;
            IsVisible = true;

            OnAppear();
            EntityControllerFeedback.EntityAppear(this);
        }

        protected virtual void OnAppear()
        {
        }

        protected internal virtual void OnDisappear()
        {
        }

        #region Components

        public T GetComponent<T>() where T : Component, new()
        {
            if (!IsVisible)
                return null;

            return Entity.GetComponent<T>();
        }

        public bool HasComponent<T>() where T : Component, new()
        {
            if (!IsVisible)
                return false;

            return Entity.HasComponent<T>();
        }

        #endregion

        #region Data Container

        public T GetDataContainer<T>() where T : class
        {
            return _dataContainer?.GetDataContainer<T>();
        }

        public void AddDataContainer<T>(T container) where T : class
        {
            if (_dataContainer == null)
            {
                _dataContainer = new DataContainer(); //not sure, maybe save some memory?
            }

            _dataContainer.AddDataContainer(container);
        }

        #endregion
    }
}
