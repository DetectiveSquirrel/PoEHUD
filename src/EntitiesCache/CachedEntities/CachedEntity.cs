using PoeHUD.EntitiesCache.CachedEntities.PositionHelpers;
using PoeHUD.Poe;
using PoeHUD.Poe.Components;

namespace PoeHUD.EntitiesCache.CachedEntities
{
    public abstract class CachedEntity : IEntityHolder
    {
        private uint _scanNumber;
        internal uint Id;

        internal CachedEntity(Entity entity, uint scanNumber, WalkablePosition position)
        {
            Id = entity.Id;
            Entity = entity;
            _scanNumber = scanNumber;
            Position = position;
            Metadata = entity.Metadata;
        }

        internal CachedEntity(Entity entity, uint scanNumber)
        {
            Id = entity.Id;
            Entity = entity;
            _scanNumber = scanNumber;
            Metadata = entity.Metadata;
        }

        public string Metadata { get; }
        public WalkablePosition Position { get; protected set; }
        public bool IsVisible { get; internal set; }
        public bool Ignored { get; internal set; }
        public Entity Entity { get; private set; }

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
            _scanNumber = scanNumber;
            IsVisible = true;
        }

        internal void CheckDisappear(uint scanNumber)
        {
            //if scan number is not equal this mean that this entity was not found while scanning, so it is out of range
            if (_scanNumber != scanNumber)
            {
                Entity = null;
                IsVisible = true;
            }
            else
                IsVisible = false;
        }

        public T GetComponent<T>() where T : Component, new()
        {
            if (!IsVisible)
                return null;
            return Entity.GetComponent<T>();
        }
    }
}
