using System;
using System.Collections.Generic;
using System.Linq;
using PoeHUD.Controllers;
using PoeHUD.EntitiesCache.CachedEntities;
using PoeHUD.Framework.Helpers;
using PoeHUD.Hud.UI;
using PoeHUD.Poe.Components;
using SharpDX;

namespace PoeHUD.Hud.Icons
{
    public class MinimapPlugin : Plugin<MapIconsSettings>
    {
        private readonly Func<IEnumerable<MapIcon>> getIcons;

        public MinimapPlugin(GameController gameController, Graphics graphics, Func<IEnumerable<MapIcon>> gatherMapIcons,
            MapIconsSettings settings)
            : base(gameController, graphics, settings)
        {
            getIcons = gatherMapIcons;
        }

        public override void Render()
        {
            try
            {
                if (!Settings.Enable || !GameController.InGame || !Settings.IconsOnMinimap)
                    return;

                var smallMinimap = GameController.Game.IngameState.IngameUi.Map.SmallMinimap;

                if (!smallMinimap.IsVisible)
                    return;

                if (GameController.Game.IngameState.IngameUi.AtlasPanel.IsVisible)
                    return;

                if (GameController.Game.IngameState.IngameUi.TreePanel.IsVisible)
                    return;

                var playerPos = PlayerInfo.GridPos;
                var posZ = PlayerInfo.GetComponent<Render>().Z;

                const float SCALE = 240f;
                var mapRect = smallMinimap.GetClientRect();
                var mapCenter = new Vector2(mapRect.X + mapRect.Width / 2, mapRect.Y + mapRect.Height / 2).Translate(0, 0);
                var diag = Math.Sqrt(mapRect.Width * mapRect.Width + mapRect.Height * mapRect.Height) / 2.0;

                foreach (var icon in getIcons().Where(x => x.IsVisible()))
                {
                    var iconZ = icon.EntityWrapper.GetComponent<Render>().Z;

                    var point = mapCenter
                                + MapIcon.DeltaInWorldToMinimapDelta(icon.WorldPosition - playerPos, diag, SCALE, (iconZ - posZ) / 20);

                    var texture = icon.TextureIcon;
                    var size = icon.Size;
                    var rect = new RectangleF(point.X - size / 2f, point.Y - size / 2f, size, size);
                    bool isContain;
                    mapRect.Contains(ref rect, out isContain);

                    if (isContain)
                        texture.Draw(Graphics, rect);
                }
            }
            catch
            {
                // do nothing
            }
        }
    }
}
