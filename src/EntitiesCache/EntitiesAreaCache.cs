using System;
using System.Collections.Generic;
using PoeHUD.Controllers;
using PoeHUD.EntitiesCache.CacheControllers;
using PoeHUD.EntitiesCache.CachedEntities;
using PoeHUD.Poe;
using PoeHUD.Poe.Components;
using SharpDX;

namespace PoeHUD.EntitiesCache
{
    public class EntitiesAreaCache
    {
        public static EntitiesAreaCache Current { get; internal set; }
        internal event Action OnEntitiesClear = delegate { };
        private uint _scanNumber;

        public readonly Dictionary<uint, CachedEntity> AllEntities;
        public readonly EntityCollectionCacheController<CachedAreaTransition> AreaTransitions;
        public readonly EntityCollectionCacheController<CachedDangerEntity> DangerEntities;
        public readonly EntityCollectionCacheController<CachedChestEntity> Chests;
        public readonly EntityCollectionCacheController<CachedDelveVein> DelveVeins;
        public readonly EntityCollectionCacheController<CachedDoorEntity> Doors;
        public readonly EntityCollectionCacheController<CachedNpcEntity> Npc;
        public readonly EntityCollectionCacheController<CachedPortalEntity> Portals;
        public readonly EntityCollectionCacheController<CachedShrine> Shrines;
        public readonly EntityCollectionCacheController<CachedChestEntity> SpecialChests;
        public readonly EntityCollectionCacheController<CachedChestEntity> Strongboxes;
        public readonly EntityCollectionCacheController<CachedPlayerEntity> Players;
        public readonly EntityCollectionCacheController<CachedWorldItemEntity> WorldItems;
        public readonly MonstersController Monsters;

        public EntitiesAreaCache()
        {
            AllEntities = new Dictionary<uint, CachedEntity>();
            AreaTransitions = new EntityCollectionCacheController<CachedAreaTransition>(this);
            DangerEntities = new EntityCollectionCacheController<CachedDangerEntity>(this);
            Chests = new EntityCollectionCacheController<CachedChestEntity>(this);
            DelveVeins = new EntityCollectionCacheController<CachedDelveVein>(this);
            Doors = new EntityCollectionCacheController<CachedDoorEntity>(this);
            Monsters = new MonstersController(this);
            Npc = new EntityCollectionCacheController<CachedNpcEntity>(this);
            Portals = new EntityCollectionCacheController<CachedPortalEntity>(this);
            Shrines = new EntityCollectionCacheController<CachedShrine>(this);
            SpecialChests = new EntityCollectionCacheController<CachedChestEntity>(this);
            Strongboxes = new EntityCollectionCacheController<CachedChestEntity>(this);
            Players = new EntityCollectionCacheController<CachedPlayerEntity>(this);
            WorldItems = new EntityCollectionCacheController<CachedWorldItemEntity>(this);
        }

        public void ForceClearEntitiesCache()
        {
            AllEntities.Clear();
            OnEntitiesClear();
        }

        public void UpdateCache()
        {
            _scanNumber++;

            if (_scanNumber == uint.MaxValue)
                _scanNumber = 0;

            PlayerInfo.UpdateInfo();

            var game = GameController.Instance.Game;

            foreach (var keyValuePair in game.IngameState.Data.EntityList.EntitiesAsDictionary)
            {
                var entity = keyValuePair.Value;
                var id = entity.Id;
                var entityMetadataPath = entity.Metadata;

                if (string.IsNullOrEmpty(entityMetadataPath))
                    continue;

                //"Metadata/Effects/Spells/monsters_effects/elemental_beacon_v2/fire/OTs/beacon"
                //"Metadata/Effects/Spells/monsters_effects/elemental_beacon_v2/fire/OTs/beacon_aoe"
                //if (entityMetadataPath.StartsWith("Metadata/Effects/Spells/monsters_effects/elemental_beacon_v2/"))
                //{
                //    if (entityMetadataPath.Contains("/OTs/beacon_aoe"))
                //    {
                //        if (!DangerEntities.TryGetValue(id, out var mine))
                //        {
                //            mine = new CachedDangerEntity(entity, _scanNumber, false, false, 70);

                //            //LogWarning($"Found aoe mine: {mine.Metadata}");
                //            DangerEntities.TryAdd(id, mine);
                //        }
                //    }
                //}

                if ((id & 0x80000000L) != 0L || entityMetadataPath.StartsWith("Metadata/Monsters/Daemon"))
                    continue;

                if (AllEntities.TryGetValue(keyValuePair.Key, out var cachedEntity))
                {
                    //if (entity == null || entity.Address == 0 || !entity.IsValid || cachedEntity.ShouldRemove()) //TODO: Define what to do in this case. I think it shouldn't be removed to not be processed again and again
                    //{
                    //    //LogMessage($"Removing: {cachedEntity.Metadata}, Addr: {entity?.Address} Valid: {entity?.IsValid}, Type: {cachedEntity.GetType().Name}");
                    //    if (cachedEntity.ShouldRemove())
                    //    {
                    //        var newWItems = WorldItems.ToList();
                    //        newWItems.RemoveAll(x => x == cachedEntity);
                    //        WorldItems = new EntityCollectionCacheController<CachedWorldItemEntity>(newWItems);
                    //        _cachedEntities.Remove(keyValuePair.Key);
                    //    }
                    //}
                    //else
                    //{
                    //    cachedEntity.Appear(entity, _scanNumber);
                    //}

                    if (cachedEntity.ShouldRemove())
                    {
                        if (cachedEntity is CachedWorldItemEntity worldItem)
                        {
                            WorldItems.EntityDestroyed(worldItem);
                            AllEntities.Remove(keyValuePair.Key);
                        }
                        else
                            AllEntities.Remove(keyValuePair.Key);

                        //LogWarning($"Item type is not defined to remove: {cachedEntity.GetType().Name}");
                    }
                    else
                    {
                        cachedEntity.Appear(entity, _scanNumber);
                    }

                    continue;
                }

                //Metadata/Terrain/Leagues/Delve/Objects/DelveMineralVein
                //Metadata/Terrain/Leagues/Delve/Objects/DelveMineralChest
                if (entity.Path.StartsWith("Metadata/Terrain/Leagues/Delve/Objects/DelveMineral"))
                {
                    var newCachedEntity = new CachedDelveVein(entity, _scanNumber);
                    AllEntities.Add(newCachedEntity.Id, newCachedEntity);
                    DelveVeins.AddNewEntity(newCachedEntity);
                    continue;
                }

                var found = false;
                var components = entity.GetComponents();

                foreach (var componentsKey in components)
                {
                    try
                    {
                        if (componentsKey.Key == nameof(NPC))
                        {
                            var newCachedEntity = new CachedNpcEntity(entity, _scanNumber);
                            AllEntities.Add(newCachedEntity.Id, newCachedEntity);
                            Npc.AddNewEntity(newCachedEntity);

                            //LogMessage($"Found Npc: {entity.Metadata}");
                            found = true;
                            break;
                        }

                        if (componentsKey.Key == nameof(Chest))
                        {
                            var newCachedEntity = new CachedChestEntity(entity, _scanNumber);
                            AllEntities.Add(newCachedEntity.Id, newCachedEntity);

                            if (entityMetadataPath == "Metadata/Chests/ArmourRack1") //todo: make some config for this
                            {
                                found = true;
                                break;
                            }

                            if (entityMetadataPath == "Metadata/Chests/WeaponRack1") //todo: make some config for this
                            {
                                found = true;
                                break;
                            }

                            var chest = game.GetObject<Chest>(componentsKey.Value);

                            if (chest.IsOpened)
                            {
                                found = true;
                                break;
                            }

                            if (IsSpecialChest(entityMetadataPath) && ProcessSpecialChest(newCachedEntity, chest))
                            {
                                //LogWarning($"Found special chest: {newCachedEntity.Metadata}");
                                SpecialChests.AddNewEntity(newCachedEntity);
                            }
                            else if (chest.IsStrongbox && ProcessStrongbox(newCachedEntity, chest) ||
                                     entityMetadataPath.StartsWith("Metadata/Chests/VaultTreasurePile") ||
                                     entityMetadataPath.StartsWith(
                                         "Metadata/Chests/Abyss/AbyssFinalChest") || //for some cases it is not comes as IsStrongbox
                                     entityMetadataPath.StartsWith("Metadata/Chests/AbyssChest") //Abyss Hoard
                            )
                            {
                                //LogWarning($"Found strongbox: {newCachedEntity.Metadata}");
                                Strongboxes.AddNewEntity(newCachedEntity);
                            }
                            else if (ProcessSimpleChest(newCachedEntity, chest))
                            {
                                //LogWarning($"Found chest: {newCachedEntity.Metadata}");
                                Chests.AddNewEntity(newCachedEntity);
                            }

                            found = true;
                            break;
                        }

                        if (componentsKey.Key == nameof(Monster))
                        {
                            AddMonster(entityMetadataPath, entity, id);

                            found = true;
                            break;
                        }

                        if (componentsKey.Key == nameof(Player))
                        {
                            var player = game.GetObject<Player>(componentsKey.Value);
                            var newCachedEntity = new CachedPlayerEntity(entity, _scanNumber, player);
                            AllEntities.Add(newCachedEntity.Id, newCachedEntity);
                            Players.AddNewEntity(newCachedEntity);
                            found = true;
                            break;
                        }

                        if (componentsKey.Key == nameof(Shrine))
                        {
                            var newCachedEntity = new CachedShrine(entity, _scanNumber);
                            AllEntities.Add(newCachedEntity.Id, newCachedEntity);
                            Shrines.AddNewEntity(newCachedEntity);
                            found = true;
                            break;
                        }

                        if (componentsKey.Key == nameof(Portal))
                        {
                            var portal = game.GetObject<Portal>(componentsKey.Value);
                            var newCachedEntity = new CachedPortalEntity(entity, _scanNumber, portal);
                            AllEntities.Add(newCachedEntity.Id, newCachedEntity);
                            Portals.AddNewEntity(newCachedEntity);
                            found = true;
                            break;
                        }

                        if (componentsKey.Key == nameof(AreaTransition))
                        {
                            var areaTransition = game.GetObject<AreaTransition>(componentsKey.Value);
                            var newCachedEntity = new CachedAreaTransition(entity, _scanNumber, areaTransition);
                            AllEntities.Add(newCachedEntity.Id, newCachedEntity);
                            AreaTransitions.AddNewEntity(newCachedEntity);
                            found = true;
                            break;
                        }

                        if (componentsKey.Key == nameof(TriggerableBlockage))
                        {
                            ProcessTriggerableBlockage(entity);
                            found = true;
                            break;
                        }

                        if (componentsKey.Key == nameof(WorldItem))
                        {
                            var newCachedEntity = new CachedWorldItemEntity(entity, _scanNumber);
                            AllEntities.Add(newCachedEntity.Id, newCachedEntity);

                            if (FilterWorldItem(newCachedEntity))
                                WorldItems.AddNewEntity(newCachedEntity);

                            //LogMessage($"Found TriggerableBlockage: {entity.Metadata}");
                            found = true;
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        DebugPlug.DebugPlugin.LogMsg($"Entities controller: Error while processing entity: {entityMetadataPath}: {e}", 10,
                            Color.Magenta);
                    }
                }

                //if (entity.Path == "Metadata/Terrain/EndGame/MapShipGraveyardCagan/Objects/Teleport") //Whakawairua Tuahu map
                //{
                //    var newCachedEntity = new CachedAreaTransition(entity, _scanNumber, AreaTransition.AreaTransitionType.Local);
                //    CachedEntities.Add(newCachedEntity.Id, newCachedEntity);
                //    AreaTransitions.Add(newCachedEntity);
                //}
                //else 
                if (!found)
                {
                    //LogMessage($"Added OTHER entity: {entity.Metadata}");
                    var otherCachedEntity = new CachedStaticEntity(entity, _scanNumber);

                    //if (!AllEntities.ContainsKey(otherCachedEntity.Id)) //Sometimes shit happen...
                    //    AllEntities.Add(otherCachedEntity.Id, otherCachedEntity);
                    AllEntities[otherCachedEntity.Id] = otherCachedEntity;
                }
            }

            AreaTransitions.CheckVisibility(_scanNumber);
            Monsters.CheckVisibility(_scanNumber);
            DangerEntities.CheckVisibility(_scanNumber);
        }

        private void AddMonster(string entityMetadataPath, Entity entity, uint id)
        {
            //Metadata/Monsters/Anomaly/AnomalyFromDaemon@73 add? This is 
            if (entityMetadataPath == "Metadata/Monsters/VolatileCore/VolatileCore" ||
                entityMetadataPath == "Metadata/Monsters/Clone/WitchLightningClone" ||
                entityMetadataPath == "Metadata/Monsters/Clone/RangerLightningClone")
            {
                var volatileMonster = new CachedDangerEntity(entity, _scanNumber, true, true, 50);

                //LogWarning($"Found VOLATILE!!!: {volatileMonster.Metadata}");
                DangerEntities.AddNewEntity(volatileMonster);
                AllEntities.Add(volatileMonster.Id, volatileMonster);
            }
            else
            {
                var newCachedEntity = new CachedMonsterEntity(entity, _scanNumber);
                AllEntities.Add(newCachedEntity.Id, newCachedEntity);

                if (FilterMonster(newCachedEntity))
                {
                    //LogWarning($"+++Found monster: {newCachedEntity.Name}, ent null: {newCachedEntity.Entity == null}, Id: {newCachedEntity.Id}");
                    Monsters.AddNewEntity(newCachedEntity);
                }
            }
        }

        private void ProcessTriggerableBlockage(Entity entity)
        {
            var lockedDoor = entity.Metadata == "Metadata/Terrain/EndGame/MapVault/Objects/Vault_Door" ||
                             entity.Metadata == "Metadata/Terrain/Leagues/Incursion/Objects/ClosedDoorPresent"; //incursion

            if (!lockedDoor)
            {
                var targ = entity.GetComponent<Targetable>();

                if (targ.Address == 0 || !targ.isTargetable)
                    return;
            }

            //Metadata/Terrain/Labyrinth/Traps/LabyrinthCascadeSpikeTrap
            var newCachedEntity = new CachedDoorEntity(entity, _scanNumber);
            AllEntities.Add(newCachedEntity.Id, newCachedEntity);
            Doors.AddNewEntity(newCachedEntity);

            if (lockedDoor)
                newCachedEntity.IsLockingDoor = true;
        }

        #region WorldItems

        private static bool FilterWorldItem(CachedWorldItemEntity cachedWorldItem)
        {
            var worldItem = cachedWorldItem.Entity.GetComponent<WorldItem>();
            var entity = worldItem.ItemEntity;
            var baseComp = entity.GetComponent<Base>();
            var itemName = baseComp.Name;
            cachedWorldItem.Name = itemName;

            //TODO
            //if (FilterItemUtils.FilterPickItem(entity, itemName))
            //    return true;
            return false;

            cachedWorldItem.IgnoreReason = "Passed through all filters (filtered out)";
            return false;
        }

        #endregion

        #region Chests
        
        private static readonly HashSet<string> _specialChestMetadada = new HashSet<string>
        {
            "Metadata/Chests/BootyChest",
            "Metadata/Chests/NotSoBootyChest",
            "Metadata/Chests/VaultTreasurePile",
            "Metadata/Chests/GhostPirateBootyChest",
            "Metadata/Chests/StatueMakersTools",
            "Metadata/Chests/StrongBoxes/VaultsOfAtziriUniqueChest",
            "Metadata/Chests/CopperChestEpic3",
            "Metadata/Chests/TutorialSupportGemChest"
        };

        private static bool IsSpecialChest(string metadata)
        {
            if (_specialChestMetadada.Contains(metadata))
                return true;

            if (metadata.Contains("/Breach/"))
                return true;

            if (metadata.Contains("/PerandusChests/"))
                return true;

            if (metadata.Contains("IncursionChest"))
                return true;

            return false;
        }

        private bool ProcessSpecialChest(CachedChestEntity chestEntity, Chest c)
        {
            if (c.IsOpened || !chestEntity.Entity.GetComponent<Targetable>().isTargetable)
                return false;

            // Perandus chests are always locked for some reason
            if (c.IsLocked && !chestEntity.Metadata.Contains("/PerandusChests/"))
                return false;

            SpecialChests.AddNewEntity(chestEntity);
            return true;
        }

        private bool ProcessStrongbox(CachedChestEntity chestEntity, Chest box)
        {
            if (box.IsOpened || box.IsLocked || !chestEntity.Entity.GetComponent<Targetable>().isTargetable)
                return false;

            Strongboxes.AddNewEntity(chestEntity);
            return true;
        }

        private bool ProcessSimpleChest(CachedChestEntity chestEntity, Chest c)
        {
            if (c.IsOpened || c.IsLocked || c.OpenOnDamage || !chestEntity.Entity.GetComponent<Targetable>().isTargetable)
                return false;

            Chests.AddNewEntity(chestEntity);
            return true;
        }

        #endregion

        #region Monsters

        private static bool FilterMonster(CachedMonsterEntity cachedMonster)
        {
            var entity = cachedMonster.Entity;

            if (entity == null)
            {
                cachedMonster.InitialFilterOutReason = "entity == null";
                return false;
            }

            if (!entity.IsAlive)
            {
                cachedMonster.InitialFilterOutReason = "!IsAlive";
                return false;
            }

            //if (entity.CannotDieAura)
            //{
            //    if (!cachedMonster.FilterOutCannotDie)
            //    {
            //        cachedMonster.InitialFilterOutReason = "Killing the source of CannotDie!";
            //        return true;
            //    }

            //    cachedMonster.InitialFilterOutReason = "CannotDie";
            //    return false;
            //}

            //if (!entity.IsHostile)
            //{
            //    cachedMonster.InitialFilterOutReason = "!IsHostile";
            //    return false;
            //}

            //if (!entity.HasComponent<Monster>()) //Sometimes item Id is equal to monster.
            //{
            //    cachedMonster.InitialFilterOutReason = "!entity.HasComponent<Monster>()";
            //    return false;
            //}

            //Note: CannotBeDamaged causing emerging enemies to be ignored and they are not pass to Monsters list. 
            //TODO: DO NOT use IsTargetable as filter here too (but I'm not sure. Test this)
            //if (entity.CannotBeDamaged)
            //{
            //    cachedMonster.FilterOutReason = "CannotBeDamaged";
            //    return false;
            //}

            //if ((
            //        !entity.IsTargetable || 
            //        entity.CannotBeDamaged) && !entity.IsEmerging)
            //    return false;

            //TODO: Fixme
            //if (!IsInIncursion && entity.ExplicitAffixes.Any(a => a.InternalName.StartsWith("MonsterIncursion")))
            //    return;

            //if (HasImmunityAura(entity))
            //{
            //    cachedMonster.InitialFilterOutReason = "HasImmunityAura";
            //    return false;
            //}

            if (SkipThisMob(entity))
            {
                cachedMonster.InitialFilterOutReason = "SkipThisMob(list)";
                return false;
            }

            return true;
        }

        private static bool HasImmunityAura(Entity entity)
        {
            foreach (var aura in entity.GetComponent<Life>().Buffs)
            {
                var name = aura.Name;

                if (name == "cannot_be_damaged" ||
                    name == "bloodlines_necrovigil" ||
                    name == "god_mode" ||
                    name == "shrine_godmode")
                    return true;
            }

            return false;
        }

        private static bool SkipThisMob(Entity entity)
        {
            var m = entity.Metadata;

            //TODO: Add from config
            return m == "Metadata/Monsters/LeagueIncursion/VaalSaucerBoss" ||
                   m.Contains("DoedreStonePillar");
        }

        #endregion
    }
}
