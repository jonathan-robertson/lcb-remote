using System;
using System.Collections.Generic;
using System.Linq;

namespace LcbRemote
{
    internal class ConsoleCmdLcbRemote : ConsoleCmdAbstract
    {
        private static readonly string[] Commands = new string[] {
            "lcbremote",
            "lcbr"
        };
        private readonly string help;

        public ConsoleCmdLcbRemote()
        {
            var dict = new Dictionary<string, string>() {
                { "debug", "toggle debug logging mode" },
                { "check", "check the current activation state of the lcb you are within range of" },
                { "activate", "activate lcb area frame for the lcb you are within range of (only the lcb owner will see it)" },
                { "deactivate", "deactivate lcb area frame for the lcb you are within range of" },
            };

            var i = 1; var j = 1;
            help = $"Usage:\n  {string.Join("\n  ", dict.Keys.Select(command => $"{i++}. {GetCommands()[0]} {command}").ToList())}\nDescription Overview\n{string.Join("\n", dict.Values.Select(description => $"{j++}. {description}").ToList())}";
        }

        public override string[] getCommands()
        {
            return Commands;
        }

        public override string getDescription()
        {
            return "Configure or adjust settings for the LCB Remote mod.";
        }

        public override string GetHelp()
        {
            return help;
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count == 0)
                {
                    SdtdConsole.Instance.Output($"At least 1 parameter is required; use 'help {Commands[0]}' to learn more.");
                    return;
                }

                switch (_params[0])
                {
                    case "debug":
                        HandleDebug();
                        return;
                    case "check":
                        HandleCheck(_senderInfo);
                        return;
                    case "activate":
                        HandleActivate(_senderInfo);
                        return;
                    case "deactivate":
                        HandleDeactivate(_senderInfo);
                        return;
                }

                SdtdConsole.Instance.Output($"Invald parameter provided; use 'help {Commands[0]}' to learn more.");
            }
            catch (Exception e)
            {
                SdtdConsole.Instance.Output($"Exception encountered: \"{e.Message}\"\n{e.StackTrace}");
            }
        }

        private void HandleDebug()
        {
            ModApi.DebugMode = !ModApi.DebugMode;
            SdtdConsole.Instance.Output($"Debug Mode has successfully been {(ModApi.DebugMode ? "enabled" : "disabled")}.");
        }

        private void HandleCheck(CommandSenderInfo _senderInfo)
        {
            if (!TryGetClosestLandClaimData(_senderInfo, out var landClaimOwner, out var landClaimBlockPos))
            {
                return;
            }
            if (!LandClaimManager.IsLandClaimActive(landClaimBlockPos, out var landClaimActive))
            {
                SdtdConsole.Instance.Output($"No land claim could be found at the expected position of {landClaimBlockPos}.");
                return;
            }
            SdtdConsole.Instance.Output($"The Land Claim Block at position {landClaimBlockPos} is owned by {landClaimOwner.PlayerName} and is currently {(landClaimActive ? "ACTIVATED" : "DEACTIVATED")}.");
        }

        private void HandleActivate(CommandSenderInfo _senderInfo)
        {
            if (!TryGetClosestLandClaimData(_senderInfo, out var landClaimOwner, out var landClaimBlockPos))
            {
                return;
            }
            if (!LandClaimManager.ActivateLandClaim(landClaimBlockPos, out var previouslyActive))
            {
                SdtdConsole.Instance.Output($"No land claim could be found at the expected position of {landClaimBlockPos}.");
                return;
            }
            if (previouslyActive)
            {
                SdtdConsole.Instance.Output($"The Land Claim Block at position {landClaimBlockPos} owned by {landClaimOwner.PlayerName} and was already active (no action taken).");
                return;
            }
            SdtdConsole.Instance.Output($"The Land Claim Block at position {landClaimBlockPos} owned by {landClaimOwner.PlayerName} has been activated just now. Please remember that only the owner ({landClaimOwner.PlayerName}) will see the green land claim frame.");
        }

        private void HandleDeactivate(CommandSenderInfo _senderInfo)
        {
            if (!TryGetClosestLandClaimData(_senderInfo, out var landClaimOwner, out var landClaimBlockPos))
            {
                return;
            }
            if (!LandClaimManager.DeactivateLandClaim(landClaimBlockPos, out var previouslyDeactivated))
            {
                SdtdConsole.Instance.Output($"No land claim could be found at the expected position of {landClaimBlockPos}.");
                return;
            }
            if (previouslyDeactivated)
            {
                SdtdConsole.Instance.Output($"The Land Claim Block at position {landClaimBlockPos} owned by {landClaimOwner.PlayerName} and was already deactivated (no action taken).");
                return;
            }
            SdtdConsole.Instance.Output($"The Land Claim Block at position {landClaimBlockPos} owned by {landClaimOwner.PlayerName} has been deactivated just now.");
        }

        private static bool TryGetClosestLandClaimData(CommandSenderInfo _senderInfo, out PersistentPlayerData landClaimOwner, out Vector3i landClaimBlockPos)
        {
            landClaimOwner = null;
            landClaimBlockPos = Vector3i.zero;
            var entityId = SafelyGetEntityIdFor(_senderInfo.RemoteClientInfo);
            if (entityId == -1)
            {
                SdtdConsole.Instance.Output("Cannot execute from telnet/rcon, please execute as a client.");
                return false;
            }
            if (!GameManager.Instance.World.Players.dict.TryGetValue(entityId, out var player))
            {
                SdtdConsole.Instance.Output($"Could find online player with entityId of {entityId}.");
                return false;
            }
            var playerBlockPos = player.GetBlockPosition();
            if (!LandClaimManager.TryGetClosestLandClaimPosContaining(playerBlockPos, out landClaimOwner, out landClaimBlockPos))
            {
                SdtdConsole.Instance.Output($"No land claim contains block position {playerBlockPos}.");
                return false;
            }
            return true;
        }

        private static int SafelyGetEntityIdFor(ClientInfo clientInfo)
        {
            return clientInfo != null
                ? clientInfo.entityId
                : GameManager.Instance.World.GetPrimaryPlayerId();
        }
    }
}
