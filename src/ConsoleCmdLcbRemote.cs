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

        public override string[] GetCommands()
        {
            return Commands;
        }

        public override string GetDescription()
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
                if (_params.Count > 0
                    && HandleParam0(_params, _senderInfo))
                {
                    return;
                }
                SdtdConsole.Instance.Output($"Invald parameter provided; use 'help {Commands[0]}' to learn more.");
            }
            catch (Exception e)
            {
                SdtdConsole.Instance.Output($"Exception encountered: \"{e.Message}\"\n{e.StackTrace}");
            }
        }

        private bool HandleParam0(List<string> _params, CommandSenderInfo _senderInfo)
        {
            int entityId;
            Vector3i playerBlockPos, landClaimBlockPos;
            EntityPlayer player;
            PersistentPlayerData landClaimOwner;

            switch (_params[0])
            {
                case "debug":
                    ModApi.DebugMode = !ModApi.DebugMode;
                    SdtdConsole.Instance.Output($"Debug Mode has successfully been {(ModApi.DebugMode ? "enabled" : "disabled")}.");
                    return true;
                case "check":
                    if (SafelyGetEntityIdFor(_senderInfo.RemoteClientInfo) == -1)
                    {
                        SdtdConsole.Instance.Output("Cannot execute from telnet/rcon, please execute as a client.");
                        return true;
                    }
                    entityId = SafelyGetEntityIdFor(_senderInfo.RemoteClientInfo);
                    if (!GameManager.Instance.World.Players.dict.TryGetValue(entityId, out player))
                    {
                        SdtdConsole.Instance.Output($"Could find online player with entityId of {entityId}.");
                        return true;
                    }
                    playerBlockPos = player.GetBlockPosition();
                    if (!LandClaimManager.TryGetLandClaimPosContaining(playerBlockPos, out landClaimBlockPos, out landClaimOwner))
                    {
                        SdtdConsole.Instance.Output($"No land claim contains block position {playerBlockPos}.");
                        return true;
                    }
                    if (!LandClaimManager.IsLandClaimActive(landClaimBlockPos, out var landClaimActive))
                    {
                        SdtdConsole.Instance.Output($"No land claim could be found at the expected position of {landClaimBlockPos}.");
                        return true;
                    }
                    SdtdConsole.Instance.Output($"The Land Claim Block at position {landClaimBlockPos} is owned by {landClaimOwner.PlayerName} and is currently {(landClaimActive ? "ACTIVATED" : "DEACTIVATED")}.");
                    return true;
                case "activate":
                    if (SafelyGetEntityIdFor(_senderInfo.RemoteClientInfo) == -1)
                    {
                        SdtdConsole.Instance.Output("Cannot execute from telnet/rcon, please execute as a client.");
                        return true;
                    }
                    entityId = SafelyGetEntityIdFor(_senderInfo.RemoteClientInfo);
                    if (!GameManager.Instance.World.Players.dict.TryGetValue(entityId, out player))
                    {
                        SdtdConsole.Instance.Output($"Could find online player with entityId of {entityId}.");
                        return true;
                    }
                    playerBlockPos = player.GetBlockPosition();
                    if (!LandClaimManager.TryGetLandClaimPosContaining(playerBlockPos, out landClaimBlockPos, out landClaimOwner))
                    {
                        SdtdConsole.Instance.Output($"No land claim contains block position {playerBlockPos}.");
                        return true;
                    }
                    if (!LandClaimManager.ActivateLandClaim(landClaimBlockPos, out var previouslyActive))
                    {
                        SdtdConsole.Instance.Output($"No land claim could be found at the expected position of {landClaimBlockPos}.");
                        return true;
                    }
                    if (previouslyActive)
                    {
                        SdtdConsole.Instance.Output($"The Land Claim Block at position {landClaimBlockPos} owned by {landClaimOwner.PlayerName} and was already active (no action taken).");
                        return true;
                    }
                    SdtdConsole.Instance.Output($"The Land Claim Block at position {landClaimBlockPos} owned by {landClaimOwner.PlayerName} has been activated just now. Please remember that only the owner ({landClaimOwner.PlayerName}) will see the green land claim frame.");
                    return true;
                case "deactivate":
                    if (SafelyGetEntityIdFor(_senderInfo.RemoteClientInfo) == -1)
                    {
                        SdtdConsole.Instance.Output("Cannot execute from telnet/rcon, please execute as a client.");
                        return true;
                    }
                    entityId = SafelyGetEntityIdFor(_senderInfo.RemoteClientInfo);
                    if (!GameManager.Instance.World.Players.dict.TryGetValue(entityId, out player))
                    {
                        SdtdConsole.Instance.Output($"Could find online player with entityId of {entityId}.");
                        return true;
                    }
                    playerBlockPos = player.GetBlockPosition();
                    if (!LandClaimManager.TryGetLandClaimPosContaining(playerBlockPos, out landClaimBlockPos, out landClaimOwner))
                    {
                        SdtdConsole.Instance.Output($"No land claim contains block position {playerBlockPos}.");
                        return true;
                    }
                    if (!LandClaimManager.DeactivateLandClaim(landClaimBlockPos, out var previouslyDeactivated))
                    {
                        SdtdConsole.Instance.Output($"No land claim could be found at the expected position of {landClaimBlockPos}.");
                        return true;
                    }
                    if (previouslyDeactivated)
                    {
                        SdtdConsole.Instance.Output($"The Land Claim Block at position {landClaimBlockPos} owned by {landClaimOwner.PlayerName} and was already deactivated (no action taken).");
                        return true;
                    }
                    SdtdConsole.Instance.Output($"The Land Claim Block at position {landClaimBlockPos} owned by {landClaimOwner.PlayerName} has been deactivated just now. Please remember that only the owner ({landClaimOwner.PlayerName}) will see the green land claim frame.");
                    return true;
            }
            return false;
        }

        private static bool TryGetPlayerBlockPosition(ClientInfo clientInfo, out Vector3i blockPos)
        {
            if (!TryGetEntityPlayerFor(clientInfo, out var player))
            {
                blockPos = Vector3i.zero;
                return false;
            }
            blockPos = player.GetBlockPosition();
            return true;
        }

        private static bool TryGetEntityPlayerFor(ClientInfo clientInfo, out EntityPlayer player)
        {
            return GameManager.Instance.World.Players.dict.TryGetValue(SafelyGetEntityIdFor(clientInfo), out player);
        }

        private static int SafelyGetEntityIdFor(ClientInfo clientInfo)
        {
            return clientInfo != null
                ? clientInfo.entityId
                : GameManager.Instance.persistentLocalPlayer.EntityId;
        }
    }
}
