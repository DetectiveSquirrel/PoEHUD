using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using PoeHUD.Poe;

namespace PoeHUD.EntitiesCache.CachedEntities
{
    public class MonstersController
    {
        private readonly List<CachedMonsterEntity> _monsters = new List<CachedMonsterEntity>();
        public IReadOnlyCollection<CachedMonsterEntity> Monsters => _monsters.AsReadOnly();
        public ConcurrentDictionary<uint, CachedDangerEntity> DangerEntities = new ConcurrentDictionary<uint, CachedDangerEntity>();

        internal void Clear()
        {
            _monsters.Clear();
            DangerEntities.Clear();
        }

        internal void AddMonster(CachedMonsterEntity cachedMonster)
        {
            _monsters.Add(cachedMonster);
        }

        internal void AddDangerEntity(uint id, CachedDangerEntity cachedMonster)
        {
            DangerEntities.Add(id, cachedMonster);
        }

        private void UpdateDangerEntities()
        {
            var dangerToRemove = new List<uint>();

            foreach (var cachedDangerEntity in DangerEntities)
            {
                try
                {
                    if (cachedDangerEntity.Value.ShouldRemove())
                    {
                        dangerToRemove.Add(cachedDangerEntity.Key);

                        //LogWarning($"Disappear danger entity: {cachedDangerEntity.Value.Metadata}");
                    }
                }
                catch //(Exception e)
                {
                    dangerToRemove.Add(cachedDangerEntity.Key);
                }
            }

            dangerToRemove.ForEach(x => DangerEntities.TryRemove(x, out _));
        }

        private void UpdateMonsters()
        {
            lock (Monsters)
            {
                foreach (var cachedMob in Monsters.ToList())
                {
                    var ent = cachedMob.Entity;

                    if (ent != null && ent.IsValid)
                    {
                        if (!ent.IsAlive || !ent.IsHostile) // || cachedMob.FilterOutCannotDie  || HasImmunityAura(ent))
                        {
                            //LogWarning($"Removing monster: {cachedMob.Name}, IsAlive: {ent.IsAlive}, CannotDie: {ent.CannotDie}. HasAura: {HasImmunityAura(ent)}, Id: {cachedMob.Id}");

                            //TODO: WARNING: Test this carefully! With this it works stable and good, bug for optimization BossKillTask we will disable this for optimization purposes
                            //removeList.Add(cachedMob);
                            //_cachedEntities.Remove(cachedMob.Id);

                            cachedMob.IsDead = !ent.IsAlive;
                        }
                    }
                    else
                    {
                        //remove monsters that were close to us, but now are null (corpse exploded, shattered etc)
                        //optimal distance is debatable, but its not recommended to be higher than 100
                        if (cachedMob.Position.Distance <= 100)
                        {
                            //LogWarning($"Removing monster: {cachedMob.Name}, EntNull: {ent == null} Valid: {ent?.IsValid}, Id: {cachedMob.Id}");
                            _monsters.Remove(cachedMob);
                            EntitiesAreaCache.Current.AllEntities.Remove(cachedMob.Id);
                            cachedMob.IsDead = true;
                            cachedMob.IsVisible = false;
                        }
                    }
                }
            }
        }
    }
}
