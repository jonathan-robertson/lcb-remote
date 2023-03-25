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
        /// Activate the land claim at the given coordinates.
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
                }
                return true;
            }
            previouslyActive = false;
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
    }
}
