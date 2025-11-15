using System.Collections.Generic;

namespace Chainbots.HexBlocks
{
    /// <summary>
    /// Manages hex block collections and initialization.
    /// </summary>
    public interface IHexGridManager
    {
        List<HexBlock> TargetBlocks { get; }
        List<HexBlock> MaterialBlocks { get; }
        void Initialize();
        void Clear();
        
        /// <summary>
        /// Creates a link (weld joint) between two blocks by their IDs.
        /// </summary>
        /// <param name="blockIdA">ID of the first block</param>
        /// <param name="blockIdB">ID of the second block</param>
        /// <returns>True if the link was created successfully, false otherwise</returns>
        bool CreateLinkBetweenBlocks(int blockIdA, int blockIdB);
        
        /// <summary>
        /// Gets a block by its ID.
        /// </summary>
        /// <param name="id">The block ID</param>
        /// <returns>The HexBlock if found, null otherwise</returns>
        HexBlock? GetBlockById(int id);
    }
}

