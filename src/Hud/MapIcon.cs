using PoeHUD.Models;
using PoeHUD.Poe.Components;
using SharpDX;
using System;
using PoeHUD.EntitiesCache.CachedEntities;
using PoeHUD.Models.Enums;

namespace PoeHUD.Hud
{
    public class CreatureMapIcon : MapIcon<CachedMonsterEntity>
    {
        public CreatureMapIcon(CachedMonsterEntity cachedEntity, string hudTexture, Func<bool> show, float iconSize)
            : base(cachedEntity, new HudTexture(hudTexture), show, iconSize)
        { }

        public override bool IsVisible()
        {
            if (!base.IsVisible() || !CachedEntity.IsAlive)
                return false;

            if (CachedEntity.IsLegion)
            {
                var rarity = CachedEntity.GetComponent<ObjectMagicProperties>().Rarity;
                if (rarity < MonsterRarity.Rare && (CachedEntity.Entity.IsFrozenInTime || !CachedEntity.Entity.IsActive))
                    return false;

                if (rarity == MonsterRarity.Rare && !CachedEntity.Entity.IsFrozenInTime && !CachedEntity.Entity.IsActive)
                    return false;
                if (Math.Round(CachedEntity.GetComponent<Life>().HPPercentage, 2) == 0.01)
                    return false;
            }

            return true;
        }
    }

    public class ChestMapIcon : MapIcon<CachedChestEntity>
    {
        public ChestMapIcon(CachedChestEntity cachedEntity, HudTexture hudTexture, Func<bool> show, float iconSize)
            : base(cachedEntity, hudTexture, show, iconSize)
        { }

        public override bool IsEntityStillValid()
        {
            return CachedEntity.IsVisible && !CachedEntity.IsOpened;//TODO: not looks correct for me, I'll look later
        }
    }

    public class MapIcon<T> where  T : CachedEntity
    {
        private readonly Func<bool> show;

        public MapIcon(T cachedEntity, HudTexture hudTexture, Func<bool> show, float iconSize = 10f)
        {
            CachedEntity = cachedEntity;
            TextureIcon = hudTexture;
            this.show = show;
            Size = iconSize;
        }

        public float? SizeOfLargeIcon { get; set; }
        public T CachedEntity { get; }
        public HudTexture TextureIcon { get; private set; }
        public float Size { get; private set; }
        public Vector2 WorldPosition => CachedEntity.GetComponent<Positioned>().GridPos;

        public static Vector2 DeltaInWorldToMinimapDelta(Vector2 delta, double diag, float scale, float deltaZ = 0)
        {
            const float CAMERA_ANGLE = 38 * MathUtil.Pi / 180;
            // Values according to 40 degree rotation of cartesian coordiantes, still doesn't seem right but closer
            var cos = (float)(diag * Math.Cos(CAMERA_ANGLE) / scale);
            var sin = (float)(diag * Math.Sin(CAMERA_ANGLE) / scale); // possible to use cos so angle = nearly 45 degrees
            // 2D rotation formulas not correct, but it's what appears to work?
            return new Vector2((delta.X - delta.Y) * cos, deltaZ - (delta.X + delta.Y) * sin);
        }

        public virtual bool IsEntityStillValid()
        {
            return CachedEntity.IsVisible;//TODO: not looks correct for me, I'll look later
        }

        public virtual bool IsVisible()
        {
            return show();
        }
    }
}