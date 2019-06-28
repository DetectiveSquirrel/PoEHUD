using System.Collections.Generic;
using System.Diagnostics;
using PoeHUD.EntitiesCache.CachedEntities;
using PoeHUD.Models;
using PoeHUD.Models.Enums;
using PoeHUD.Poe.Components;

namespace PoeHUD.Hud.Health
{
    public class HealthBar
    {
        private const int DPS_CHECK_TIME = 1000;
        private const int DPS_FAST_CHECK_TIME = 200;
        private const int DPS_POP_TIME = 2000;

        private static readonly List<string> IgnoreEntitiesList = new List<string>
        {
            "GoddessOfJustice",
            "MonsterFireTrap2",
            "MonsterBlastRainTrap",
            "Metadata/Monsters/Frog/FrogGod/SilverOrb",
            "Metadata/Monsters/Frog/FrogGod/SilverPool"
        };

        private readonly Stopwatch dpsStopwatch = Stopwatch.StartNew();
        private readonly bool isHostile;
        private int lastHp;

        public HealthBar(CachedMonsterEntity cachedEntity, HealthBarSettings settings)
        {
            CachedEntity = cachedEntity;

            // If ignored entity found, skip
            foreach (var _entity in IgnoreEntitiesList)
            {
                if (cachedEntity.Metadata.Contains(_entity))
                    return;
            }

            IsValid = true;

            if (cachedEntity.Entity.IsHostile)
            {
                isHostile = true;

                switch (cachedEntity.GetComponent<ObjectMagicProperties>().Rarity)
                {
                    case MonsterRarity.White:
                        Type = CreatureType.Normal;
                        Settings = settings.NormalEnemy;
                        break;

                    case MonsterRarity.Magic:
                        Type = CreatureType.Magic;
                        Settings = settings.MagicEnemy;
                        break;

                    case MonsterRarity.Rare:
                        Settings = settings.RareEnemy;
                        Type = CreatureType.Rare;
                        break;

                    case MonsterRarity.Unique:
                        Settings = settings.UniqueEnemy;
                        Type = CreatureType.Unique;
                        break;
                }
            }
            else
            {
                Type = CreatureType.Minion;
                Settings = settings.Minions;
            }

            Life = CachedEntity.GetComponent<Life>();
            lastHp = GetFullHp();
        }

        public HealthBar(CachedPlayerEntity cachedEntity, HealthBarSettings settings)
        {
            CachedEntity = cachedEntity;

            // If ignored entity found, skip
            foreach (var _entity in IgnoreEntitiesList)
            {
                if (cachedEntity.Metadata.Contains(_entity))
                    return;
            }

            Type = CreatureType.Player;
            Settings = settings.Players;
            IsValid = true;
            Life = CachedEntity.GetComponent<Life>();
            lastHp = GetFullHp();
        }

        public Life Life { get; }
        public CachedEntity CachedEntity { get; }
        public bool IsValid { get; }
        public UnitSettings Settings { get; }
        public CreatureType Type { get; }
        public LinkedList<int> DpsQueue { get; } = new LinkedList<int>();

        public bool IsShow(bool showEnemy)
        {
            return !isHostile ? Settings.Enable.Value : Settings.Enable && showEnemy && isHostile;
        }

        public bool IsLegionAndHidden(CachedEntity entity)
        {
            return entity.IsVisible && entity.Metadata.Contains("LegionLeague") && !entity.Entity.IsActive;
        }

        public void DpsRefresh()
        {
            var chechTime = DpsQueue.Count > 0 ? DPS_CHECK_TIME : DPS_FAST_CHECK_TIME;

            if (dpsStopwatch.ElapsedMilliseconds >= chechTime)
            {
                var hp = GetFullHp();

                if (hp > -1000000 && hp < 10000000 && lastHp != hp)
                {
                    DpsQueue.AddFirst(-(lastHp - hp));

                    if (DpsQueue.Count > Settings.FloatingCombatStackSize)
                    {
                        DpsQueue.RemoveLast();
                        dpsStopwatch.Restart();
                    }

                    lastHp = hp;
                }
            }
        }

        public void DpsDequeue()
        {
            if (dpsStopwatch.ElapsedMilliseconds >= DPS_POP_TIME)
            {
                if (DpsQueue.Count > 0)
                    DpsQueue.RemoveLast();

                dpsStopwatch.Restart();
            }
        }

        private int GetFullHp()
        {
            return Life.CurHP + Life.CurES;
        }
    }
}
