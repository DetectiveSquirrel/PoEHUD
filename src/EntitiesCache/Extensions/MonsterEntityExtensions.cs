using PoeHUD.Controllers;
using PoeHUD.Poe;

namespace PoeHUD.EntitiesCache.Extensions
{
    public static class MonsterEntityExtensions
    {
        public static string GetMonsterName(this Entity entity)
        {
            var path = entity.Metadata;
            var splitIndex = path.IndexOf("@");

            if (splitIndex != -1)
                path = path.Substring(0, splitIndex);

            var monsterCfg = GameController.Instance.Files.MonsterVarieties.TranslateFromMetadata(path);

            if (monsterCfg == null)
            {
                return entity.Metadata;
            }
            return monsterCfg.MonsterName;
        }

        public static bool IsStrongboxMinion(this Entity entity)
        {
            return entity.HasBuff("summoned_monster_epk_buff");
        }

        public static bool IsHarbingerMinion(this Entity entity)
        {
            return entity.HasBuff("harbinger_minion_new");
        }
    }
}
