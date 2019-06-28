﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using PoeHUD.Controllers;
using PoeHUD.EntitiesCache;
using PoeHUD.EntitiesCache.CachedEntities;
using PoeHUD.Framework;
using PoeHUD.Framework.Helpers;
using PoeHUD.Hud.Preload;
using PoeHUD.Hud.Settings;
using PoeHUD.Hud.UI;
using PoeHUD.Models;
using PoeHUD.Poe.Components;
using SharpDX;
using SharpDX.Direct3D9;

namespace PoeHUD.Hud.XpRate
{
    public class XpRatePlugin : SizedPlugin<XpRateSettings>
    {
        private readonly SettingsHub settingsHub;

        private readonly Dictionary<int, float> ArenaEffectiveLevels = new Dictionary<int, float>
        {
            {71, 70.94f},
            {72, 71.82f},
            {73, 72.64f},
            {74, 73.4f},
            {75, 74.1f},
            {76, 74.74f},
            {77, 75.32f},
            {78, 75.84f},
            {79, 76.3f},
            {80, 76.7f},
            {81, 77.04f},
            {82, 77.32f},
            {83, 77.54f},
            {84, 77.7f}
        };

        private bool autoHide;
        private readonly int DelveSulphiteCapacityID;
        private bool holdKey;
        private double levelXpPenalty, partyXpPenalty;
        private Player LocalPlayer;
        private DateTime startTime, lastTime;
        private long startXp, getXp, xpLeftQ;
        private string xpRate, timeLeft;

        public XpRatePlugin(GameController gameController, Graphics graphics, XpRateSettings settings, SettingsHub settingsHub)
            : base(gameController, graphics, settings)
        {
            this.settingsHub = settingsHub;
            GameController.Area.AreaChange += area => AreaChange();
            DelveSulphiteCapacityID = GameController.Instance.Files.Stats.records["delve_sulphite_capacity"].ID;
        }

        public override void Render()
        {
            if (GameController.Game.IngameState.IngameUi.InventoryPanel.IsVisible) return;

            try
            {
                var UIHover = GameController.Game.IngameState.UIHover;
                var miniMap = GameController.Game.IngameState.IngameUi.Map.SmallMinimap;

                if (Settings.Enable.Value && UIHover.Address != 0x00 && UIHover.Tooltip.Address != 0x00 &&
                    UIHover.Tooltip.IsVisible && UIHover.Tooltip.GetClientRect().Intersects(miniMap.GetClientRect()))
                {
                    autoHide = true;
                    Settings.Enable.Value = false;
                }

                if (autoHide && (UIHover.Address == 0x00 || UIHover.Tooltip.Address == 0x00 ||
                                 !UIHover.Tooltip.IsVisible))
                {
                    autoHide = false;
                    Settings.Enable.Value = true;
                }

                if (!holdKey && WinApi.IsKeyDown(Keys.F10))
                {
                    holdKey = true;
                    Settings.Enable.Value = !Settings.Enable.Value;
                    SettingsHub.Save(settingsHub);
                }
                else if (holdKey && !WinApi.IsKeyDown(Keys.F10))
                    holdKey = false;

                if (!Settings.Enable)
                    return;

                LocalPlayer = PlayerInfo.GetComponent<Player>();
                var nowTime = DateTime.Now;
                var elapsedTime = nowTime - lastTime;

                if (elapsedTime.TotalSeconds > 1)
                {
                    CalculateXp(nowTime);
                    partyXpPenalty = PartyXpPenalty();
                    lastTime = nowTime;
                }

                var showInTown =
                    !Settings.ShowInTown && GameController.Area.CurrentArea.IsTown ||
                    !Settings.ShowInTown && GameController.Area.CurrentArea.IsHideout;

                var position = StartDrawPointFunc();
                var fps = $"fps:({GameController.Game.IngameState.CurFps})";
                var areaName = $"{GameController.Area.CurrentArea.DisplayName}";
                var AreaNameColor = PreloadAlertPlugin.AreaNameColor;

                if (Settings.OnlyAreaName)
                {
                    if (!showInTown)
                    {
                        var areaNameSize = Graphics.MeasureText(areaName, Settings.TextSize);
                        float boxHeight = areaNameSize.Height;
                        var boxWidth = MathHepler.Max(areaNameSize.Width);
                        var bounds = new RectangleF(position.X - 84 - boxWidth, position.Y - 5, boxWidth + 90, boxHeight + 12);
                        var latency = $"({GameController.Game.IngameState.CurLatency})";
                        Graphics.DrawText(areaName, Settings.TextSize, new Vector2(bounds.X + 84, position.Y), AreaNameColor);
                        Graphics.DrawImage("preload-start.png", bounds, Settings.BackgroundColor);
                        Graphics.DrawImage("preload-end.png", bounds, Settings.BackgroundColor);

                        if (Settings.ShowLatency)
                            Graphics.DrawText(latency, Settings.TextSize, new Vector2(bounds.X + 35, position.Y), Settings.LatencyTextColor);

                        Size = bounds.Size;
                        Margin = new Vector2(0, 5);
                    }
                }

                if (!Settings.OnlyAreaName)
                {
                    if (!showInTown)
                    {
                        var xpReceiving = levelXpPenalty * partyXpPenalty;
                        var xpReceivingText = $"{xpRate}  *{xpReceiving:p0}";
                        var xpGetLeft = $"Got: {ConvertHelper.ToShorten(getXp, "0.00")}  Left: {ConvertHelper.ToShorten(xpLeftQ, "0.00")}";
                        var ping = $"ping:({GameController.Game.IngameState.CurLatency})";
                        var areaNameSize = Graphics.DrawText(areaName, Settings.TextSize, position - 1, AreaNameColor, FontDrawFlags.Right);
                        var secondLine = position.Translate(-1, areaNameSize.Height + 2);
                        var xpRateSize = Graphics.DrawText(timeLeft, Settings.TextSize, secondLine, Settings.XphTextColor, FontDrawFlags.Right);
                        var thirdLine = secondLine.Translate(-1, xpRateSize.Height + 2);

                        var xpLeftSize = Graphics.DrawText(xpReceivingText, Settings.TextSize, thirdLine, Settings.TimeLeftColor,
                            FontDrawFlags.Right);

                        var fourLine = thirdLine.Translate(-1, xpLeftSize.Height + 2);

                        var xpGetLeftSize = Graphics.DrawText(xpGetLeft, Settings.TextSize, fourLine,
                            Settings.XphTextColor, FontDrawFlags.Right);

                        var delveInfoSize = Size2.Zero;

                        if (PlayerInfo.GetComponent<Stats>().StatDictionary.TryGetValue(DelveSulphiteCapacityID, out var sulphiteCapacity))
                        {
                            if (sulphiteCapacity > 0)
                            {
                                var sulphite = $"Sulphite: {GameController.Game.IngameState.ServerData.CurrentSulphiteAmount}/{sulphiteCapacity}";
                                var azurite = $"Azurite: {GameController.Game.IngameState.ServerData.CurrentAzuriteAmount}";
                                var fifthLine = fourLine.Translate(-1, xpGetLeftSize.Height + 2);

                                delveInfoSize = Graphics.DrawText($"{sulphite} {azurite}", Settings.TextSize, fifthLine,
                                    Settings.DelveInfoTextcolor, FontDrawFlags.Right);

                                delveInfoSize.Width += 40;
                            }
                        }

                        var timer = AreaInstance.GetTimeString(nowTime - GameController.Area.CurrentArea.TimeEntered);
                        var timerSize = Graphics.MeasureText(timer, Settings.TextSize);

                        var boxWidth = MathHepler.Max(xpRateSize.Width + 40, xpLeftSize.Width + 40, areaNameSize.Width + 20, timerSize.Width,
                            delveInfoSize.Width);

                        float boxHeight = xpRateSize.Height + xpLeftSize.Height + areaNameSize.Height + delveInfoSize.Height;
                        var bounds = new RectangleF(position.X - boxWidth - 104, position.Y - 7, boxWidth + 110, boxHeight + 40);

                        var timeFpsSize = Graphics.MeasureText(fps, Settings.TextSize);
                        var dif = bounds.Width - (12 + timeFpsSize.Width + xpRateSize.Width);

                        if (dif < 0)
                        {
                            bounds.X += dif;
                            bounds.Width -= dif;
                        }

                        Graphics.DrawText(timer, Settings.TextSize, new Vector2(bounds.X + 70, position.Y), Settings.TimerTextColor);

                        if (Settings.ShowFps)
                            Graphics.DrawText(fps, Settings.TextSize, new Vector2(bounds.X + 70, secondLine.Y), Settings.FpsTextColor);

                        if (Settings.ShowLatency)
                            Graphics.DrawText(ping, Settings.TextSize, new Vector2(bounds.X + 70, thirdLine.Y), Settings.LatencyTextColor);

                        Graphics.DrawImage("preload-start.png", bounds, Settings.BackgroundColor);
                        Graphics.DrawImage("preload-end.png", bounds, Settings.BackgroundColor);
                        Size = bounds.Size;
                        Margin = new Vector2(0, 5);
                    }
                }
            }
            catch
            {
                // do nothing
            }
        }

        private void CalculateXp(DateTime nowTime)
        {
            var level = LocalPlayer.Level;

            if (level >= 100)
            {
                // player can't level up, just show fillers
                xpRate = "0.00 xp/h";
                timeLeft = "--h--m--s";
                return;
            }

            long currentXp = LocalPlayer.XP;
            getXp = currentXp - startXp;
            var rate = (currentXp - startXp) / (nowTime - startTime).TotalHours;
            xpRate = $"{ConvertHelper.ToShorten(rate, "0.00")} xp/h";

            if (level >= 0 && level + 1 < Constants.PlayerXpLevels.Length && rate > 1)
            {
                var xpLeft = Constants.PlayerXpLevels[level + 1] - currentXp;
                xpLeftQ = xpLeft;
                var time = TimeSpan.FromHours(xpLeft / rate);
                timeLeft = $"{time.Hours:0}h {time.Minutes:00}m {time.Seconds:00}s to level up";
            }
        }

        private double LevelXpPenalty()
        {
            var arenaLevel = GameController.Area.CurrentArea.RealLevel;
            var characterLevel = LocalPlayer.Level;

            var effectiveArenaLevel = arenaLevel < 71 ? arenaLevel : ArenaEffectiveLevels[arenaLevel];
            var safeZone = Math.Floor(Convert.ToDouble(characterLevel) / 16) + 3;
            var effectiveDifference = Math.Max(Math.Abs(characterLevel - effectiveArenaLevel) - safeZone, 0);
            double xpMultiplier;

            xpMultiplier = Math.Pow((characterLevel + 5) / (characterLevel + 5 + Math.Pow(effectiveDifference, 2.5)), 1.5);

            if (characterLevel >= 95) //For player levels equal to or higher than 95:
                xpMultiplier *= 1d / (1 + 0.1 * (characterLevel - 94));

            xpMultiplier = Math.Max(xpMultiplier, 0.01);

            return xpMultiplier;
        }

        private double PartyXpPenalty()
        {
            var levels = new List<int>();
    
            foreach (var entity in EntitiesAreaCache.Current.Players.AllEntities)
            {
                if (!entity.IsVisible)
                    continue;

                if (entity.HasComponent<Player>())
                    levels.Add(entity.GetComponent<Player>().Level);
            }

            var characterLevel = LocalPlayer.Level;
            var partyXpPenalty = Math.Pow(characterLevel + 10, 2.71) / levels.Sum(level => Math.Pow(level + 10, 2.71));
            return partyXpPenalty * levels.Count;
        }

        private IEnumerator StartXp()
        {
            xpRate = "0.00 xp/h";
            timeLeft = "-h -m -s  to level up";
            getXp = 0;
            xpLeftQ = 0;

            //yield return new WaitFunction(() =>{return !GameController.InGameReal;});
            yield return new WaitFunction(() => GameController.Game.IsGameLoading);

            //yield return new WaitTime(300);
            startTime = lastTime = DateTime.Now;
            LocalPlayer = PlayerInfo.GetComponent<Player>();
            startXp = LocalPlayer.XP;
            levelXpPenalty = LevelXpPenalty();
        }

        private void AreaChange()
        {
            new Coroutine(StartXp(), nameof(XpRatePlugin), "AreaChange Start Xp").Run();
        }
    }
}
