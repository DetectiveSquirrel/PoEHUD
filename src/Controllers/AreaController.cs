using System;
using System.Collections.Generic;
using PoeHUD.Models;
using PoeHUD.Plugins;

namespace PoeHUD.Controllers
{
    public class AreaController
    {
        private const double DELETE_FROM_CACHE_OLD_AREA_MIN = 15; //TODO: To core config
        private readonly Dictionary<uint, AreaInstance> _areasCache = new Dictionary<uint, AreaInstance>();
        private readonly GameController _gController;

        public AreaController(GameController gameController)
        {
            _gController = gameController;
        }

        public AreaInstance CurrentArea { get; private set; }
        [Obsolete("Use OnAreaChange event in plugin to avoid memory leek")]
        public event Action<AreaController> OnAreaChange;
        public event Action<AreaController> AreaChange;

        public void RefreshState()
        {
            if (!_gController.InGame)
                return;

            _gController.Cache.UpdateCache();
            var inGameData = _gController.Game.IngameState.Data;
            var areaHash = inGameData.CurrentAreaHash;

            if (CurrentArea?.Hash != areaHash)
            {
                if (_areasCache.TryGetValue(areaHash, out var currentAreaData))
                {
                    BasePlugin.LogMessage("Area change. Found in cache.", 5);
                    CurrentArea = currentAreaData;
                    currentAreaData.UsedFromCache();
                }
                else
                {
                    var areaTemplate = inGameData.CurrentArea;
                    CurrentArea = new AreaInstance(areaTemplate, areaHash, inGameData.CurrentAreaLevel);

                    _areasCache.Add(areaHash, CurrentArea);
                    BasePlugin.LogMessage("Area change. Created new...", 5);
                }

                AreaChange?.Invoke(this);
                OnAreaChange?.Invoke(this);
            }
        }
    }
}
