using Chainbots.HexBlocks;
using System.Collections.Generic;

namespace Chainbots.Interfaces;

public interface IHexGridManager
{
    List<HexBlock> MaterialBlocks { get; }
    void Initialize();
    void Clear();

    /// <summary>
    /// Gets a block by its ID.
    /// </summary>
    /// <param name="id">The block ID</param>
    /// <returns>The HexBlock if found, null otherwise</returns>
    HexBlock? GetBlockById(int id);
}

