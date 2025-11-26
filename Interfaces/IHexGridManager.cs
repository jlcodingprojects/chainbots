using Chainbots.HexBlocks;
using System.Collections.Generic;

namespace Chainbots.Interfaces;

public interface IHexGridManager
{
    List<HexBlock> MaterialBlocks { get; }
    void Initialize();
    void Clear();
    HexBlock? GetBlockById(int id);
    void RemoveBlockById(int id);
    void AddBlock(int q, int r, bool anchoredToGround);
}

