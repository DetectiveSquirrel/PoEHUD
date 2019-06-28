using PoeHUD.Controllers;
using PoeHUD.Models;
using PoeHUD.Poe;
using PoeHUD.Poe.Components;
using SharpDX;

namespace PoeHUD.EntitiesCache.CachedEntities
{
    public static class PlayerInfo
    {
        public static Vector2 GridPos { get; private set; }
        public static Vector3 WorldPos { get; private set; }
        public static RectangleF GameWindowRectangle { get; private set; }
        public static Vector2 GameWindowRectCenter { get; private set; }
        public static Vector2 ScreenCenterPlayerStop { get; private set; }
        public static Entity PlayerEntity { get; private set; }
        public static EntityWrapper Player { get; private set; }
        public static Actor ActorComponent { get; private set; }

        private static long PlayerCachedAddr;

        internal static void UpdateInfo()
        {
            GameWindowRectangle = GameController.Instance.Window.GetWindowRectangle();
            GameWindowRectCenter = GameWindowRectangle.Center;
            ScreenCenterPlayerStop = new Vector2(GameWindowRectCenter.X, GameWindowRectCenter.Y + 40);
            PlayerEntity = GameController.Instance.Game.IngameState.Data.LocalPlayer;
            Player = new EntityWrapper(GameController.Instance, PlayerEntity);
            GridPos = Player.PositionedComp.GridPos;
            WorldPos = Player.Pos;

            if (PlayerCachedAddr == 0 || PlayerCachedAddr != Player.Address)
            {
                PlayerCachedAddr = Player.Address;
                ActorComponent = Player.GetComponent<Actor>();
            }
        }

        public static T GetComponent<T>() where T : Component, new()
        {
            return Player.GetComponent<T>();
        }
    }
}
