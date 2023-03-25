namespace LcbRemote
{
    internal class ModApi : IModApi
    {
        private static readonly ModLog<ModApi> _log = new ModLog<ModApi>();

        public static bool DebugMode { get; set; } = true; // TODO: disable before release

        public void InitMod(Mod _modInstance)
        {
            _log.Info("Not yet implemented.");
        }
    }
}
