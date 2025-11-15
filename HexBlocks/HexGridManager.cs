using Chainbots.Models;
using Chainbots.Physics;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Chainbots.HexBlocks;

/// <summary>
/// Represents the initial configuration of a block.
/// </summary>
public struct BlockConfiguration
{
    public int Id;
    public Vector2 WorldPosition;
    public List<int> ConnectedBlockIds;
    public bool IsAnchoredToGround;

    public BlockConfiguration(int id, float x, float y, bool anchoredToGround, params int[] connectedIds)
    {
        Id = id;
        WorldPosition = new Vector2(x, y);
        IsAnchoredToGround = anchoredToGround;
        ConnectedBlockIds = new List<int>(connectedIds);
    }
}

public class HexGridManager : IHexGridManager
{
    public List<HexBlock> TargetBlocks { get; }
    public List<HexBlock> MaterialBlocks { get; }

    private readonly IPhysicsWorld _physicsWorld;
    private readonly float _hexSize;
    private Body? _groundBody; // Static body representing the ground for anchoring

    private Dictionary<int, WeldJoint> _blockJoints = new Dictionary<int, WeldJoint>();

    private static readonly BlockConfiguration[] InitialBlockPositions =
    [
        new BlockConfiguration(1, 0, 0, anchoredToGround: true),

        new BlockConfiguration(2, 2, 0, anchoredToGround: true),

        new BlockConfiguration(3, 4, 0, anchoredToGround: true),

        new BlockConfiguration(4, 1, -1, anchoredToGround: false, 1, 2),

        new BlockConfiguration(5, 3, -1, anchoredToGround: false, 2, 3),

        new BlockConfiguration(6, 0, -2, anchoredToGround: false, 4)
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

    public void Clear()
    {
        TargetBlocks.Clear();
        MaterialBlocks.Clear();
        _blockJoints.Clear();
        _groundBody = null;
    }

    private void CreateGroundBody()
    {
        _groundBody = _physicsWorld.GroundBody;
    }

    private void CreateTargetGrid()
    {
        // Create target grid (skeleton view) - larger structure
        for (int q = -4; q <= 4; q++)
        {
            for (int r = -4; r <= 4; r++)
            {
                if (Math.Abs(q) + Math.Abs(r) <= 4)
                {
                    var targetBlock = new HexBlock(
                        _physicsWorld.World,
                        new HexCoordinate(q, r),
                        _hexSize,
                        HexBlockType.Target,
                        isStatic: true
                    );
                    TargetBlocks.Add(targetBlock);
                }
            }
        }
    }

    private void CreateMaterialBlocksFromConfiguration()
    {
        // Create material blocks from the initial configuration array
        foreach (var config in InitialBlockPositions)
        {
            // Convert world position to hex coordinate (approximate)
            var hexCoord = HexCoordinate.FromPixel(config.WorldPosition, _hexSize);

            // Create the material block at the exact world position
            var materialBlock = new HexBlock(
                _physicsWorld.World,
                hexCoord,
                _hexSize,
                HexBlockType.Material,
                isStatic: false
            );

            if (materialBlock.Body != null)
            {
                var x = 0 + (config.WorldPosition.X * 0.75f);
                // Calculate a clean block placement Y coordinate
                var y = 7.05f + (config.WorldPosition.Y * 0.375f);
                              //- ((config.WorldPosition.X + 1) % 2) * 0.375f)
                              
                materialBlock.Body.Position = new Vector2(x, y);
            }

            MaterialBlocks.Add(materialBlock);
        }
    }

    private void CreateLinksFromConfiguration()
    {
        if (_groundBody == null) return;

        foreach (var config in InitialBlockPositions)
        {
            var block = GetBlockById(config.Id);
            if (block == null) continue;

            // Anchor to ground if specified
            if (config.IsAnchoredToGround)
            {
                var joint = HexBlock.CreateGroundAnchorJoint(_physicsWorld.World, block, _groundBody);
                if (joint != null)
                {
                    _blockJoints[config.Id] = joint;
                }
            }

            // Create links to connected blocks
            foreach (var connectedId in config.ConnectedBlockIds)
            {
                CreateLinkBetweenBlocks(config.Id, connectedId);
            }
        }
    }

    /// <summary>
    /// Reusable method to create a link (weld joint) between two blocks by their IDs.
    /// The joint connects the blocks through their centers.
    /// </summary>
    /// <param name="blockIdA">ID of the first block</param>
    /// <param name="blockIdB">ID of the second block</param>
    /// <returns>True if the link was created successfully, false otherwise</returns>
    public bool CreateLinkBetweenBlocks(int blockIdA, int blockIdB)
    {
        var blockA = GetBlockById(blockIdA);
        var blockB = GetBlockById(blockIdB);

        if (blockA == null || blockB == null)
        {
            Console.WriteLine($"Cannot create link: Block #{blockIdA} or #{blockIdB} not found");
            return false;
        }

        if (blockA.Body == null || blockB.Body == null)
        {
            Console.WriteLine($"Cannot create link: Block #{blockIdA} or #{blockIdB} has no physics body");
            return false;
        }

        // Create a weld joint connecting the two blocks through their centers
        var joint = HexBlock.CreateAnchorJoint(_physicsWorld.World, blockA, blockB);

        if (joint != null)
        {
            Console.WriteLine($"Created link between Block #{blockIdA} and Block #{blockIdB}");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets a block by its ID.
    /// </summary>
    /// <param name="id">The block ID</param>
    /// <returns>The HexBlock if found, null otherwise</returns>
    public HexBlock? GetBlockById(int id)
    {
        int index = id - 1;

        if (index >= 0 && index < MaterialBlocks.Count)
        {
            var block = MaterialBlocks[index];
            // Verify the ID matches (safety check)
            if (block.Id == id)
            {
                return block;
            }
        }

        // Fallback: search through all blocks
        return MaterialBlocks.FirstOrDefault(b => b.Id == id);
    }
}

