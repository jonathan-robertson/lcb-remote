using System;
using System.Collections.Generic;

namespace LcbRemote
{
    internal class LandClaimManager
    {
        private static readonly ModLog<LandClaimManager> _log = new ModLog<LandClaimManager>();

        /// <summary>
        /// Return whether the land claim at the given coordinates is active (showing bounds for its owner).
        /// </summary>
        /// <param name="lcbBlockPos">Vector3i block position of the land claim to check.</param>
        /// <returns>Whether the land claim at the given coordinates is active (showing bounds for its owner).</returns>
        public static bool IsLandClaimActive(Vector3i lcbBlockPos, out bool currentlyActive)
        {
            var world = GameManager.Instance.World;
            var chunkId = world.ChunkCache.ClusterIdx;
            if (world.GetTileEntity(chunkId, lcbBlockPos) is TileEntityLandClaim tileEntityLandClaim)
            {
                currentlyActive = tileEntityLandClaim.ShowBounds;
                return true;
            }
            currentlyActive = false;
            return false;
        }

        /// <summary>
        /// Activate the land claim at the given position.
        /// </summary>
        /// <param name="lcbBlockPos">Vector3i block position of the land claim to activate.</param>
        /// <param name="previouslyActive">Whether this land claim was already active.</param>
        /// <returns>Whether the land claim could be found at the given position.</returns>
        public static bool ActivateLandClaim(Vector3i lcbBlockPos, out bool previouslyActive)
        {
            var world = GameManager.Instance.World;
            var chunkId = world.ChunkCache.ClusterIdx;
            if (world.GetTileEntity(chunkId, lcbBlockPos) is TileEntityLandClaim tileEntityLandClaim)
            {
                previouslyActive = tileEntityLandClaim.ShowBounds;
                if (!previouslyActive)
                {
                    tileEntityLandClaim.ShowBounds = true;
                    TriggerTileChangesForOwner(lcbBlockPos, tileEntityLandClaim);
                }
                return true;
            }
            previouslyActive = false;
            return false;
        }

        /// <summary>
        /// Deactivate the land claim at the given position.
        /// </summary>
        /// <param name="lcbBlockPos">Vector3i block position of the land claim to deactivate.</param>
        /// <param name="previouslyInactive">Whether this land claim was already inactive.</param>
        /// <returns>Whether the land claim could be found at the given position.</returns>
        public static bool DeactivateLandClaim(Vector3i lcbBlockPos, out bool previouslyInactive)
        {
            var world = GameManager.Instance.World;
            var chunkId = world.ChunkCache.ClusterIdx;
            if (world.GetTileEntity(chunkId, lcbBlockPos) is TileEntityLandClaim tileEntityLandClaim)
            {
                previouslyInactive = !tileEntityLandClaim.ShowBounds;
                if (!previouslyInactive)
                {
                    tileEntityLandClaim.ShowBounds = false;
                    TriggerTileChangesForOwner(lcbBlockPos, tileEntityLandClaim);
                }
                return true;
            }
            previouslyInactive = false;
            return false;
        }

        /// <summary>
        /// Get the position and owner of the land claim contiaining the given block coordinates (if such a land claim exists).
        /// </summary>
        /// <param name="blockPos">Vector3i block position contained within the returned land claim (if one exists).</param>
        /// <param name="landClaimOwner">The owner of the land claim containing the given block position (if one exists).</param>
        /// <param name="landClaimBlockPos">Vector3i block position of the land claim containing the given block position (if one exists).</param>
        /// <returns>Whether a land claim containing the given block position was located.</returns>
        public static bool TryGetLandClaimPosContaining(Vector3i blockPos, out PersistentPlayerData landClaimOwner, out Vector3i landClaimBlockPos)
        {
            foreach (var kvp in GameManager.Instance.persistentPlayers.m_lpBlockMap)
            {
                if (IsWithinLandClaimAtBlockPos(blockPos, kvp.Key))
                {
                    landClaimBlockPos = kvp.Key;
                    landClaimOwner = kvp.Value;
                    return true;
                }
            }
            landClaimBlockPos = Vector3i.zero;
            landClaimOwner = null;
            return false;
        }

        /// <summary>
        /// Get the position and owner of the closest land claim also containing the given block coordinates (if such a land claim exists).
        /// </summary>
        /// <param name="blockPos">Vector3i block position contained within the returned land claim (if one exists).</param>
        /// <param name="landClaimOwner">The owner of the land claim containing the given block position (if one exists).</param>
        /// <param name="landClaimBlockPos">Vector3i block position of the land claim containing the given block position (if one exists).</param>
        /// <returns></returns>
        public static bool TryGetClosestLandClaimPosContaining(Vector3i blockPos, out PersistentPlayerData landClaimOwner, out Vector3i landClaimBlockPos)
        {
            if (!TryGetLandClaimPositionsContaining(blockPos, out landClaimOwner, out var landClaimBlockPositionsContaining))
            {
                landClaimOwner = null;
                landClaimBlockPos = Vector3i.zero;
                return false;
            }

            landClaimBlockPos = GetClosestLandClaimPosition(blockPos, landClaimBlockPositionsContaining);
            return true;
        }

        public static bool TryGetLandClaimPositionsContaining(Vector3i blockPos, out PersistentPlayerData landClaimOwner, out List<Vector3i> landClaimBlockPositions)
        {
            landClaimOwner = null;
            landClaimBlockPositions = new List<Vector3i>();
            foreach (var kvp in GameManager.Instance.persistentPlayers.m_lpBlockMap)
            {
                if (IsWithinLandClaimAtBlockPos(blockPos, kvp.Key))
                {
                    landClaimBlockPositions.Add(kvp.Key);
                    landClaimOwner = kvp.Value;
                }
            }
            return landClaimBlockPositions.Count > 0;
        }

        public static Vector3i GetClosestLandClaimPosition(Vector3i blockPos, List<Vector3i> landClaimPositions)
        {
            if (landClaimPositions.Count == 0)
            {
                return Vector3i.zero;
            }
            var closestLandClaimPos = landClaimPositions[0];
            var shortestDistance = Distance(blockPos, closestLandClaimPos);
            if (landClaimPositions.Count > 1)
            {
                for (var i = 1; i < landClaimPositions.Count; i++)
                {
                    var thisDistance = Distance(blockPos, landClaimPositions[i]);
                    if (thisDistance < shortestDistance)
                    {
                        shortestDistance = thisDistance;
                        closestLandClaimPos = landClaimPositions[i];
                    }
                }
            }
            return closestLandClaimPos;
        }

        private static bool IsWithinLandClaimAtBlockPos(Vector3i blockPos, Vector3i landClaimPos)
        {
            return landClaimPos.x - ModApi.LandClaimRadius - 1 <= blockPos.x
                    && blockPos.x <= landClaimPos.x + ModApi.LandClaimRadius + 1
                && landClaimPos.z - ModApi.LandClaimRadius - 1 <= blockPos.z
                    && blockPos.z <= landClaimPos.z + ModApi.LandClaimRadius + 1;
        }

        /// <summary>
        /// To trigger a client-side update of LCB Bounds, the client either needs to reload the LCB block (done via chunk reload) by leaving and returning or logging out... or trigger an OnBlockAdded call since this method checks for an existing TileEntityLandClaim object and properly applies its ShowBounds variable.
        /// 
        /// It's unfortunate that the OnBlockChanged flow always *ignores* the TileEntityLandClaim.ShowBounds value and sets bounds to false... but this method spoofs a fake block type change on the LCB, re-sends the TileEntityLandClaim update (with the updated ShowBounds value) and then sets the block type back to being an LCB. This triggers a call to OnBlockAdded without also purging TileEntityLandClaim from the block's position.
        /// </summary>
        /// <param name="lcbBlockPos">Vector3i block position of the Land Claim block to modify.</param>
        /// <param name="tileEntityLandClaim">TileEntityLandClaim at the given lcbBlockPos coordinates.</param>
        private static void TriggerTileChangesForOwner(Vector3i lcbBlockPos, TileEntityLandClaim tileEntityLandClaim)
        {
            _log.Trace("TriggerTileChangesForOwner");
            var ownerId = tileEntityLandClaim.GetOwner();
            if (!GameManager.Instance.persistentPlayers.Players.TryGetValue(ownerId, out var ownerPlayerData))
            {
                _log.Trace("no ppdata found");
                return;
            }

            var clientInfo = ConnectionManager.Instance.Clients.ForEntityId(ownerPlayerData.EntityId);
            if (clientInfo == null)
            {
                _log.Trace("no client found");
                return;
            }

            var foundPlayer = GameManager.Instance.World.Players.dict.TryGetValue(ownerPlayerData.EntityId, out var ownerPlayer);
            if (!foundPlayer)
            {
                _log.Trace($"no online player found with entity id of {ownerPlayerData.EntityId}");
                return;
            }
            var playerWithinVisibleRange = AreWithinVisibleRange(lcbBlockPos, ownerPlayer.GetBlockPosition());
            if (!playerWithinVisibleRange)
            {
                _log.Trace($"player {ownerPlayer.GetDebugName()} is at {ownerPlayer.position} and not within visible range of lcb at {lcbBlockPos}");
                return;
            }

            _log.Trace("player found and within visible range; sending package to player to spoof block type and trigger OnBlockAdded flow to update client-side LCB Bounds state.");

            var blockValue = tileEntityLandClaim.blockValue;
            var originalType = blockValue.type;
            blockValue.type = 1;
            var spoofedTypePackage = NetPackageManager.GetPackage<NetPackageSetBlock>().Setup(ownerPlayerData, new List<BlockChangeInfo>() {
                new BlockChangeInfo(lcbBlockPos, blockValue, false),
            }, -1);
            blockValue.type = originalType;
            var originalTypePackage = NetPackageManager.GetPackage<NetPackageSetBlock>().Setup(ownerPlayerData, new List<BlockChangeInfo>() {
                new BlockChangeInfo(lcbBlockPos, blockValue, false),
            }, -1);
            _ = NetPackageManager.GetPackage<NetPackageTileEntity>().Setup(tileEntityLandClaim, TileEntity.StreamModeWrite.ToClient, byte.MaxValue);

            clientInfo.SendPackage(spoofedTypePackage);
            //clientInfo.SendPackage(tileEntityPackage); // add tileEntity again since it was likely wiped out by the type change above
            clientInfo.SendPackage(originalTypePackage);
        }

        private static bool AreWithinVisibleRange(Vector3i pos1, Vector3i pos2)
        {
            var viewDistance = 128;
            return pos1.x - viewDistance - 1 <= pos2.x
                    && pos2.x <= pos1.x + viewDistance + 1
                && pos1.z - viewDistance - 1 <= pos2.z
                    && pos2.z <= pos1.z + viewDistance + 1;
        }

        // lifted from Vector3.Distance method and reapplied here
        private static float Distance(Vector3i a, Vector3i b)
        {
            float num = a.x - b.x;
            float num2 = a.y - b.y;
            float num3 = a.z - b.z;
            return (float)Math.Sqrt((num * num) + (num2 * num2) + (num3 * num3));
        }
    }
}
