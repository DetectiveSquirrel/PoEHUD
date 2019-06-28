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
    public class LargeMapPlugin : Plugin<MapIconsSettings>
    {
        private readonly Func<IEnumerable<MapIcon>> getIcons;

        public LargeMapPlugin(GameController gameController, Graphics graphics, Func<IEnumerable<MapIcon>> gatherMapIcons,
            MapIconsSettings settings)
            : base(gameController, graphics, settings)
        {
            getIcons = gatherMapIcons;
        }

        public override void Render()
        {
            try
            {
                if (!Settings.Enable || !GameController.InGame || !Settings.IconsOnLargeMap
                    || !GameController.Game.IngameState.IngameUi.Map.LargeMap.IsVisible)
                    return;

                if (GameController.Game.IngameState.IngameUi.AtlasPanel.IsVisible)
                    return;

                if (GameController.Game.IngameState.IngameUi.TreePanel.IsVisible)
                    return;

                var camera = GameController.Game.IngameState.Camera;
                var mapWindow = GameController.Game.IngameState.IngameUi.Map;
                var mapRect = mapWindow.GetClientRect();

                var playerPos = PlayerInfo.GridPos;
                var posZ = PlayerInfo.GetComponent<Render>().Z;

                var screenCenter = new Vector2(mapRect.Width / 2, mapRect.Height / 2).Translate(0, -20) + new Vector2(mapRect.X, mapRect.Y)
                                                                                                        + new Vector2(mapWindow.LargeMapShiftX,
                                                                                                            mapWindow.LargeMapShiftY);

                var diag = (float) Math.Sqrt(camera.Width * camera.Width + camera.Height * camera.Height);
                var k = camera.Width < 1024f ? 1120f : 1024f;
                var scale = k / camera.Height * camera.Width * 3f / 4f / mapWindow.LargeMapZoom;

                foreach (var icon in getIcons().Where(x => x.IsVisible()))
                {
                    var iconZ = icon.EntityWrapper.GetComponent<Render>().Z;

                    var point = screenCenter
                                + MapIcon.DeltaInWorldToMinimapDelta(icon.WorldPosition - playerPos, diag, scale,
                                    (iconZ - posZ) / (9f / mapWindow.LargeMapZoom));

                    var texture = icon.TextureIcon;
                    var size = icon.Size * 2; //icon.SizeOfLargeIcon.GetValueOrDefault(icon.Size * 2);
                    texture.Draw(Graphics, new RectangleF(point.X - size / 2f, point.Y - size / 2f, size, size));
                }
            }
            catch
            {
                // do nothing
            }
        }
    }
}
