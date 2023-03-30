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
        /// <param name="landClaimBlockPos">Vector3i block position of the land claim containing the given block position (if one exists).</param>
        /// <param name="landClaimOwner">The owner of the land claim containing the given block position (if one exists).</param>
        /// <returns>Whether a land claim containing the given block position was located.</returns>
        public static bool TryGetLandClaimPosContaining(Vector3i blockPos, out Vector3i landClaimBlockPos, out PersistentPlayerData landClaimOwner)
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

        private static bool IsWithinLandClaimAtBlockPos(Vector3i blockPos, Vector3i landClaimPos)
        {
            return landClaimPos.x - ModApi.LandClaimRadius - 1 <= blockPos.x
                    && blockPos.x <= landClaimPos.x + ModApi.LandClaimRadius + 1
                && landClaimPos.z - ModApi.LandClaimRadius - 1 <= blockPos.z
                    && blockPos.z <= landClaimPos.z + ModApi.LandClaimRadius + 1;
        }

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

            if (foundPlayer && playerWithinVisibleRange)
            {
                _log.Trace("player found and within visible range; sending package to player to remove/re-add LCB");

                var blockValue = tileEntityLandClaim.blockValue;
                var originalType = blockValue.type;
                blockValue.type = 1;
                var newTypePackage = NetPackageManager.GetPackage<NetPackageSetBlock>().Setup(ownerPlayerData, new List<BlockChangeInfo>() {
                    new BlockChangeInfo(lcbBlockPos, blockValue, false),
                }, -1);

                blockValue.type = originalType;
                var originalTypePackage = NetPackageManager.GetPackage<NetPackageSetBlock>().Setup(ownerPlayerData, new List<BlockChangeInfo>() {
                    new BlockChangeInfo(lcbBlockPos, blockValue, false),
                }, -1);

                //_log.Trace($"changeBlockValueLandClaimNewRotation; bChangeBlockValue: {changeBlockValueLandClaimNewRotation.bChangeBlockValue}");

                // TODO: DO NOT SET TO AIR (it wipes the TileEntity when we do)
                _ = NetPackageManager.GetPackage<NetPackageSetBlock>().Setup(ownerPlayerData, new List<BlockChangeInfo>() {
                    new BlockChangeInfo(lcbBlockPos, BlockValue.Air, false),
                }, -1);
                _ = NetPackageManager.GetPackage<NetPackageSetBlock>().Setup(ownerPlayerData, new List<BlockChangeInfo>()
                {
                    //new BlockChangeInfo(lcbBlockPos, BlockValue.Air, false),
                    //changeBlockValueLandClaimNewRotation, // TODO: confirm correct bools (2 of them)
                    // maybe we change damage; blockChangeInfo.bChangeDamage or maybe this won't make a difference...
                }, -1);
                var tileEntityPackage = NetPackageManager.GetPackage<NetPackageTileEntity>().Setup(tileEntityLandClaim, TileEntity.StreamModeWrite.ToClient, byte.MaxValue);

                //clientInfo.SendPackage(removeBlockPackage); // this reliably removes frame on client

                //clientInfo.SendPackage(tileEntityPackage); // does not make any difference

                // TODO: try adding harmony hooks in local mod to track movement in client

                // TODO: swapping block type will be necessary in order to trigger blockAdded..
                //  The easiest way to do this would be to remove/add the block, but removing deletes the underlying TileEntity which we need to keep.
                //  Instead, switching blockType may work.
                //   TODO: confirm if switching to a different block type (temporarily) will purge the existing TileEntityLandClaim
                // To enable showBounds field on the client, the tileEntity must already exist.
                clientInfo.SendPackage(newTypePackage);
                // TODO: should we re-send the TileEntity here?
                clientInfo.SendPackage(tileEntityPackage);
                clientInfo.SendPackage(originalTypePackage);

                //clientInfo.SendPackage(tileEntityPackage); // this allows client to interact with lcb once again

                //tileEntityLandClaim.SetChunkModified(); // TODO: ?

                /* NOTE: the below code *partially* works, but maybe not for the reason I'm hoping.
                 * Currently looking into these possibilities...
                 * NetPackageSetBlock.ProcessPackage(World, GameManager) : void @06003229
                 * GameManager.ChangeBlocks(PlatformUserIdentifierAbs, List<BlockChangeInfo>) : void @06005F06
                 * ChunkCluster.SetBlock(Vector3i, bool, BlockValue, bool, sbyte, bool, bool, bool) : BlockValue @06003E2E
                 * Chunk.SetBlock(WorldBase, int, int, int, int, BlockValue, bool, bool) : BlockValue @06003D45

                // TODO: try damaging a little?
                var changeBlockValueAir = new BlockChangeInfo(lcbBlockPos, BlockValue.Air, false);
                var blockChangeInfoList = new List<BlockChangeInfo>() {
                    changeBlockValueAir,
                    //new BlockChangeInfo(lcbBlockPos, tileEntityLandClaim.blockValue, false), // TODO: confirm correct bools (2 of them)
                };
                clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageSetBlock>().Setup(ownerPlayerData, blockChangeInfoList, -1));
                // interesting... setting to Air (removing) then adding back the same object results in the frame disappearing, but now
                //  prevents the player from interacting with his lcb the normal way.

                var changeBlockValueLandClaimNewRotation = new BlockChangeInfo(lcbBlockPos, tileEntityLandClaim.blockValue, false);
                _log.Trace($"changeBlockValueLandClaimNewRotation; bChangeBlockValue: {changeBlockValueLandClaimNewRotation.bChangeBlockValue}");
                var blockChangeInfoList2 = new List<BlockChangeInfo>() {
                    //new BlockChangeInfo(lcbBlockPos, BlockValue.Air, false),
                    changeBlockValueLandClaimNewRotation, // TODO: confirm correct bools (2 of them)
                };
                clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageSetBlock>().Setup(ownerPlayerData, blockChangeInfoList2, -1));

                // This... or some *part* of this does allow us to regain our ability to interact with the LCB after modifying it
                //tileEntityLandClaim.SetChunkModified();
                //SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageTileEntity>().Setup(tileEntityLandClaim, TileEntity.StreamModeWrite.ToClient, byte.MaxValue), true, -1, -1, -1, -1);

                */
            }
        }

        private static bool AreWithinVisibleRange(Vector3i pos1, Vector3i pos2)
        {
            var viewDistance = 128;
            return pos1.x - viewDistance - 1 <= pos2.x
                    && pos2.x <= pos1.x + viewDistance + 1
                && pos1.z - viewDistance - 1 <= pos2.z
                    && pos2.z <= pos1.z + viewDistance + 1;
        }
    }
}
