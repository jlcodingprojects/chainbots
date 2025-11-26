using Chainbots.Interfaces;
using Chainbots.Models;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Chainbots.ChainBots;

// need to further expand on this coordinate system...
public struct ChainBotCellConfiguration
{
    public HexCoordinate Coordinate { get; }

    public ChainBotCellConfiguration(int q, int r)
    {
        Coordinate = new HexCoordinate(q, r);
    }
}

public class ChainBot
{
    public List<ChainBotCell> Cells { get; }

    private readonly IPhysicsWorld _physicsWorld;
    private readonly float _hexSize;
    private Body? _groundBody;
    private Vector2 _worldOffset = Vector2.Zero;

    private readonly Dictionary<int, ChainBotCell> _blocksById = new();
    private readonly Dictionary<HexCoordinate, ChainBotCell> _blocksByCoordinate = new();
    private readonly Dictionary<(int first, int second), WeldJoint> _blockLinkJoints = new();

    private static readonly ChainBotCellConfiguration[] InitialBlockLayout =
    [
        // ground array
        new ChainBotCellConfiguration(-16, 16),
        new ChainBotCellConfiguration(-14, 15),
        new ChainBotCellConfiguration(-12, 14),
        new ChainBotCellConfiguration(-10, 13),
        new ChainBotCellConfiguration(-8, 12),
        new ChainBotCellConfiguration(-6, 11),
        new ChainBotCellConfiguration(-4, 10),

    ];

    public ChainBot(IPhysicsWorld physicsWorld, float hexSize)
    {
        _physicsWorld = physicsWorld;
        _hexSize = hexSize;

        Cells = new List<ChainBotCell>();
    }

    public void Initialize()
    {
        Clear();
        CreateMaterialBlocksFromConfiguration();
        CreateLinksFromConfiguration();
    }

    public void AddBlock(int q, int r)
    {
        ChainBotCellConfiguration config = new ChainBotCellConfiguration(q, r);
        var materialBlock = new ChainBotCell(
                _physicsWorld.World,
                config.Coordinate,
                _hexSize
            );

        materialBlock.SetPrecisePosition(GetWorldPosition(config.Coordinate));

        Cells.Add(materialBlock);
        _blocksById[materialBlock.Id] = materialBlock;
        _blocksByCoordinate[config.Coordinate] = materialBlock;
    }

    public void RemoveBlockById(int id)
    {
        if (_blocksById.TryGetValue(id, out var block))
        {
            Cells.Remove(block);
            _blocksById.Remove(id);
        }
    }

    public void Clear()
    {
        Cells.Clear();
        Cells.Clear();
        _blocksById.Clear();
        _blocksByCoordinate.Clear();
        _blockLinkJoints.Clear();
        _groundBody = null;
        _worldOffset = Vector2.Zero;
        ChainBotCell.ResetIds();
    }

    private void CreateGroundBody()
    {
        _groundBody = _physicsWorld.GroundBody;
        if (_groundBody != null)
        {
            float baseY = _groundBody.Position.Y - (_hexSize + 0.02f);
            //_worldOffset = new Vector2(0f, baseY);
        }
        else
        {
            //_worldOffset = Vector2.Zero;
        }
    }

    private void CreateMaterialBlocksFromConfiguration()
    {
        foreach (var config in InitialBlockLayout)
        {
            var materialBlock = new ChainBotCell(
                _physicsWorld.World,
                config.Coordinate,
                _hexSize
            );

            materialBlock.SetPrecisePosition(GetWorldPosition(config.Coordinate));

            Cells.Add(materialBlock);
            _blocksById[materialBlock.Id] = materialBlock;
            _blocksByCoordinate[config.Coordinate] = materialBlock;
        }
    }

    private void CreateLinksFromConfiguration()
    {
        if (_groundBody == null) return;

        foreach (var config in InitialBlockLayout)
        {
            var block = GetBlockByCoordinate(config.Coordinate);
            if (block == null) continue;
        }
    }

    public ChainBotCell? GetBlockById(int id)
    {
        return _blocksById.TryGetValue(id, out var block) ? block : null;
    }

    private ChainBotCell? GetBlockByCoordinate(HexCoordinate coordinate)
    {
        return _blocksByCoordinate.TryGetValue(coordinate, out var block) ? block : null;
    }

    private Vector2 GetWorldPosition(HexCoordinate coordinate)
    {
        return coordinate.ToPixel(_hexSize) + _worldOffset;
    }
}

