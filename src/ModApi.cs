using System;

namespace LcbRemote
{
    internal class ModApi : IModApi
    {
        private static readonly ModLog<ModApi> _log = new ModLog<ModApi>();

        public static bool DebugMode { get; set; } = true; // TODO: disable before release
        public static bool IsServer { get; private set; }
        public static int LandClaimSize { get; private set; }
        public static int LandClaimRadius { get; private set; }

        public void InitMod(Mod _modInstance)
        {
            ModEvents.GameStartDone.RegisterHandler(OnGameStartDone);
        }

        private void OnGameStartDone()
        {
            try
            {
                IsServer = SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;
                if (IsServer)
                {
                    _log.Trace("OnGameStartDone");
                    LandClaimSize = GameStats.GetInt(EnumGameStats.LandClaimSize); // 41 is the default, for example
                    LandClaimRadius = LandClaimSize % 2 == 1 ? (LandClaimSize - 1) / 2 : LandClaimSize / 2;
                    _log.Debug($"LandClaimSize: {LandClaimSize}, LandClaimRadius: {LandClaimRadius}");
                }
            }
            catch (Exception e)
            {
                _log.Error("Failed OnGameStartDone", e);
            }
        }
    }
}
