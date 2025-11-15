using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Chainbots.Models;
using Chainbots.Physics;

namespace Chainbots.HexBlocks
{
    /// <summary>
    /// Manages hex block collections and initialization.
    /// </summary>
    public class HexGridManager : IHexGridManager
    {
        public List<HexBlock> TargetBlocks { get; }
        public List<HexBlock> MaterialBlocks { get; }
        public List<HexBlock> AnchorBlocks { get; }

        private readonly IPhysicsWorld _physicsWorld;
        private readonly float _hexSize;

        public HexGridManager(IPhysicsWorld physicsWorld, float hexSize)
        {
            _physicsWorld = physicsWorld;
            _hexSize = hexSize;

            TargetBlocks = new List<HexBlock>();
            MaterialBlocks = new List<HexBlock>();
            AnchorBlocks = new List<HexBlock>();
        }

        public void Initialize()
        {
            Clear();
            CreateTargetGrid();
            CreateAnchorBlocks();
            CreateMaterialBlocks();
            ConnectHexBlocks(MaterialBlocks);
            ConnectAnchorsToMaterial();
        }

        public void Clear()
        {
            TargetBlocks.Clear();
            MaterialBlocks.Clear();
            AnchorBlocks.Clear();
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

        private void CreateAnchorBlocks()
        {
            // Create anchor blocks (locked to ground) - positioned off to the side
            // These are part of the material group
            for (int i = 0; i < 3; i++)
            {
                var anchorBlock = new HexBlock(
                    _physicsWorld.World,
                    new HexCoordinate(-8, i - 1),
                    _hexSize,
                    HexBlockType.Anchor,
                    isStatic: true
                );
                AnchorBlocks.Add(anchorBlock);
            }
        }

        private void CreateMaterialBlocks()
        {
            // Create material blocks - smaller starting structure with physics enabled
            // These will have gravity and can move
            for (int q = -2; q <= 2; q++)
            {
                for (int r = -2; r <= 2; r++)
                {
                    if (Math.Abs(q) + Math.Abs(r) <= 2)
                    {
                        var materialBlock = new HexBlock(
                            _physicsWorld.World,
                            new HexCoordinate(q, r),
                            _hexSize,
                            HexBlockType.Material,
                            isStatic: false
                        );
                        MaterialBlocks.Add(materialBlock);
                    }
                }
            }
        }

        private void ConnectHexBlocks(List<HexBlock> blocks)
        {
            // Connect neighboring hex blocks with anchor joints
            foreach (var blockA in blocks)
            {
                foreach (var blockB in blocks)
                {
                    if (blockA == blockB) continue;

                    // Check if they are neighbors
                    for (int dir = 0; dir < 6; dir++)
                    {
                        var neighbor = blockA.Coordinate.GetNeighbor(dir);
                        if (neighbor.Equals(blockB.Coordinate))
                        {
                            // Create anchor joint between neighbors
                            HexBlock.CreateAnchorJoint(_physicsWorld.World, blockA, blockB);
                            break;
                        }
                    }
                }
            }
        }

        private void ConnectAnchorsToMaterial()
        {
            // Connect some material blocks to anchor blocks
            if (AnchorBlocks.Count > 0 && MaterialBlocks.Count > 0)
            {
                // Find material blocks near the anchors and connect them
                foreach (var anchorBlock in AnchorBlocks)
                {
                    // Find nearby material blocks
                    foreach (var materialBlock in MaterialBlocks)
                    {
                        float distance = Vector2.Distance(
                            anchorBlock.Coordinate.ToPixel(_hexSize),
                            materialBlock.Coordinate.ToPixel(_hexSize)
                        );

                        // Connect if within reasonable range
                        if (distance < _hexSize * 5f)
                        {
                            HexBlock.CreateAnchorJoint(_physicsWorld.World, anchorBlock, materialBlock);
                        }
                    }
                }
            }
        }
    }
}

