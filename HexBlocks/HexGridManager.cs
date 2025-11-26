using Chainbots.Interfaces;
using Chainbots.Models;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Chainbots.HexBlocks;

public struct BlockConfiguration
{
    public HexCoordinate Coordinate { get; }
    public bool IsAnchoredToGround { get; }

    public BlockConfiguration(int q, int r, bool anchoredToGround)
    {
        Coordinate = new HexCoordinate(q, r);
        IsAnchoredToGround = anchoredToGround;
    }
}

public class HexGridManager : IHexGridManager
{
    public List<HexBlock> TargetBlocks { get; }
    public List<HexBlock> MaterialBlocks { get; }

    private readonly IPhysicsWorld _physicsWorld;
    private readonly float _hexSize;
    private Body? _groundBody;
    private Vector2 _worldOffset = Vector2.Zero;

    private readonly Dictionary<int, HexBlock> _blocksById = new();
    private readonly Dictionary<HexCoordinate, HexBlock> _blocksByCoordinate = new();
    private readonly Dictionary<int, WeldJoint> _groundAnchorJoints = new();
    private readonly Dictionary<(int first, int second), WeldJoint> _blockLinkJoints = new();

    private static readonly BlockConfiguration[] InitialBlockLayout =
    [
        // ground array
        new BlockConfiguration(-16, 16, anchoredToGround: true),
        new BlockConfiguration(-14, 15, anchoredToGround: true),
        new BlockConfiguration(-12, 14, anchoredToGround: true),
        new BlockConfiguration(-10, 13, anchoredToGround: true),
        new BlockConfiguration(-8, 12, anchoredToGround: true),
        new BlockConfiguration(-6, 11, anchoredToGround: true),
        new BlockConfiguration(-4, 10, anchoredToGround: true),
        new BlockConfiguration(-2, 9, anchoredToGround: true),
        new BlockConfiguration(0, 8, anchoredToGround: true),
        new BlockConfiguration(2, 7, anchoredToGround: true),
        new BlockConfiguration(4, 6, anchoredToGround: true),

        // features
        // block on top
        new BlockConfiguration(-16, 15, anchoredToGround: false),

        // block in between
        new BlockConfiguration(-13, 14, anchoredToGround: false),

        // top and between
        new BlockConfiguration(-10, 12, anchoredToGround: false),
        new BlockConfiguration(-9, 12, anchoredToGround: false),

        new BlockConfiguration(-6, 10, anchoredToGround: false),
        new BlockConfiguration(-5, 10, anchoredToGround: false),
        new BlockConfiguration(-4, 9, anchoredToGround: false),

        new BlockConfiguration(-2, 8, anchoredToGround: false),
        new BlockConfiguration(-2, 7, anchoredToGround: false),

        new BlockConfiguration(0, 7, anchoredToGround: false),
        new BlockConfiguration(0, 6, anchoredToGround: false),

        new BlockConfiguration(2, 6, anchoredToGround: false),
        new BlockConfiguration(2,5, anchoredToGround: false),
        new BlockConfiguration(1,7, anchoredToGround: false),
        new BlockConfiguration(1,6, anchoredToGround: false),
        new BlockConfiguration(3,6, anchoredToGround: false),
        new BlockConfiguration(4, 5, anchoredToGround: false),

        //floating
        new BlockConfiguration(1,5, anchoredToGround: false),
        new BlockConfiguration(2,4, anchoredToGround: false),
        new BlockConfiguration(3,3, anchoredToGround: false),
        new BlockConfiguration(4,3, anchoredToGround: false),
        new BlockConfiguration(4,2, anchoredToGround: false),
        new BlockConfiguration(5,2, anchoredToGround: false),
        new BlockConfiguration(6,1, anchoredToGround: false),

        //reaching up
        new BlockConfiguration(6,0, anchoredToGround: false),
        new BlockConfiguration(6,-1, anchoredToGround: false),
        new BlockConfiguration(7, 0, anchoredToGround: false),
        new BlockConfiguration(7,-1, anchoredToGround: false),
        new BlockConfiguration(5,-1, anchoredToGround: false),
        new BlockConfiguration(6,-2, anchoredToGround: false),
        new BlockConfiguration(4,-1, anchoredToGround: false),
        new BlockConfiguration(3, 0, anchoredToGround: false),
        new BlockConfiguration(3,-1, anchoredToGround: false),

    ];

    public HexGridManager(IPhysicsWorld physicsWorld, float hexSize)
    {
        _physicsWorld = physicsWorld;
        _hexSize = hexSize;

        TargetBlocks = new List<HexBlock>();
        MaterialBlocks = new List<HexBlock>();
    }

    public void Initialize()
    {
        Clear();
        CreateGroundBody();
        CreateMaterialBlocksFromConfiguration();
        CreateLinksFromConfiguration();
    }

    public void AddBlock(int q, int r, bool anchoredToGround)
    {
        BlockConfiguration config = new BlockConfiguration(q, r, anchoredToGround);
        var materialBlock = new HexBlock(
                _physicsWorld.World,
                config.Coordinate,
                _hexSize,
                isStatic: false
            );

        materialBlock.SetPrecisePosition(GetWorldPosition(config.Coordinate));

        MaterialBlocks.Add(materialBlock);
        _blocksById[materialBlock.Id] = materialBlock;
        _blocksByCoordinate[config.Coordinate] = materialBlock;
    }

    public void RemoveBlockById(int id)
    {
        if (_blocksById.TryGetValue(id, out var block))
        {
            MaterialBlocks.Remove(block);
            _blocksById.Remove(id);
        }
    }

    public void Clear()
    {
        TargetBlocks.Clear();
        MaterialBlocks.Clear();
        _blocksById.Clear();
        _blocksByCoordinate.Clear();
        _groundAnchorJoints.Clear();
        _blockLinkJoints.Clear();
        _groundBody = null;
        _worldOffset = Vector2.Zero;
        HexBlock.ResetIds();
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
            var materialBlock = new HexBlock(
                _physicsWorld.World,
                config.Coordinate,
                _hexSize,
                isStatic: false
            );

            materialBlock.SetPrecisePosition(GetWorldPosition(config.Coordinate));

            MaterialBlocks.Add(materialBlock);
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

            if (config.IsAnchoredToGround)
            {
                var joint = HexBlock.CreateGroundAnchorJoint(_physicsWorld.World, block, _groundBody);
                if (joint != null)
                {
                    _groundAnchorJoints[block.Id] = joint;
                }
            }
        }
    }

    public HexBlock? GetBlockById(int id)
    {
        return _blocksById.TryGetValue(id, out var block) ? block : null;
    }

    private HexBlock? GetBlockByCoordinate(HexCoordinate coordinate)
    {
        return _blocksByCoordinate.TryGetValue(coordinate, out var block) ? block : null;
    }

    private Vector2 GetWorldPosition(HexCoordinate coordinate)
    {
        return coordinate.ToPixel(_hexSize) + _worldOffset;
    }

    private static (int first, int second) NormalizePair(int a, int b)
    {
        return a <= b ? (a, b) : (b, a);
    }
}

