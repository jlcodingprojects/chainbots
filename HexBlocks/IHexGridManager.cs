using System.Collections.Generic;

namespace Chainbots.HexBlocks
{
    /// <summary>
    /// Interface for hex grid management.
    /// </summary>
    public interface IHexGridManager
    {
        List<HexBlock> TargetBlocks { get; }
        List<HexBlock> MaterialBlocks { get; }
        List<HexBlock> AnchorBlocks { get; }
        void Initialize();
        void Clear();
    }
}

