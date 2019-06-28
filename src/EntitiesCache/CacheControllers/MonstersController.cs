using System;
using System.Collections.Generic;
using System.Linq;
using PoeHUD.EntitiesCache.CachedEntities;

namespace PoeHUD.EntitiesCache.CacheControllers
{
    public class MonstersController : EntityCollectionCacheController<CachedMonsterEntity>
    {
        public static MonstersController Current { get; internal set; }

        public IEnumerable<CachedMonsterEntity> VisibleAliveMonsters { get; }
        public IEnumerable<CachedMonsterEntity> VisibleAliveEnemyMonsters { get; }
        public IEnumerable<CachedMonsterEntity> OutOfRangeAliveMonsters { get; }

        internal MonstersController(EntitiesAreaCache entitiesCache) : base(entitiesCache)
        {
            VisibleAliveMonsters = new EntityCollectionFilter<CachedMonsterEntity>(_allEntities, x => x.IsVisible && !x.IsDead);
            OutOfRangeAliveMonsters = new EntityCollectionFilter<CachedMonsterEntity>(_allEntities, x => !x.IsVisible && !x.IsDead);
            VisibleAliveEnemyMonsters = new EntityCollectionFilter<CachedMonsterEntity>(_allEntities, x => !x.IsVisible && !x.IsDead && !x.IsAlly);
        }

        public event Action<MonsterDeathParams> OnMonsterDeath = delegate { };

        internal override void CheckVisibility(uint scanNumber)
        {
            base.CheckVisibility(scanNumber);
            UpdateMonsters();
        }

        internal override void AddNewEntity(CachedMonsterEntity cachedEntity)
        {
            base.AddNewEntity(cachedEntity);
            //probably this is not our kill
            if (cachedEntity.IsDead)
            {
                OnMonsterDeath(new MonsterDeathParams(cachedEntity, killedByOtherPlayer: true));
            }
        }

        private void UpdateMonsters()
        {
            foreach (var cachedMonsterEntity in VisibleAliveMonsters.ToList())
            {
                var ent = cachedMonsterEntity.Entity;

                if (ent != null && ent.IsValid)
                {
                    if (!ent.IsAlive)// || !ent.IsHostile) // || cachedMob.FilterOutCannotDie  || HasImmunityAura(ent))
                    {
                        //LogWarning($"Removing monster: {cachedMob.Name}, IsAlive: {ent.IsAlive}, CannotDie: {ent.CannotDie}. HasAura: {HasImmunityAura(ent)}, Id: {cachedMob.Id}");

                        //TODO: WARNING: Test this carefully! With this it works stable and good, bug for optimization BossKillTask we will disable this for optimization purposes
                        //removeList.Add(cachedMob);
                        //_cachedEntities.Remove(cachedMob.Id);

                        if (!ent.IsAlive)
                        {
                            cachedMonsterEntity.IsDead = true;
                            OnMonsterDeath(new MonsterDeathParams(cachedMonsterEntity, killedByOtherPlayer: false));//this is more likely we killed the enemy
                        }
                    }
                }
                else
                {
                    //remove monsters that were close to us, but now are null (corpse exploded, shattered etc)
                    //optimal distance is debatable, but its not recommended to be higher than 100
                    if (cachedMonsterEntity.Position.Distance <= 100)
                    {
                        //LogWarning($"Removing monster: {cachedMob.Name}, EntNull: {ent == null} Valid: {ent?.IsValid}, Id: {cachedMob.Id}");
                        
                        EntityDestroyed(cachedMonsterEntity);                 
                        cachedMonsterEntity.IsDead = true;
                        cachedMonsterEntity.IsVisible = false;
                    }
                }
            }
        }
    }

    public class MonsterDeathParams
    {
        public MonsterDeathParams(CachedMonsterEntity monsterEntity, bool killedByOtherPlayer)
        {
            MonsterEntity = monsterEntity;
            KilledByOtherPlayer = killedByOtherPlayer;
        }

        public CachedMonsterEntity MonsterEntity { get; }
        /// <summary>
        /// Probably killed by some other player
        /// </summary>
        public bool KilledByOtherPlayer { get; }
    }
}
