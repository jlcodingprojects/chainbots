using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.Dynamics.Joints;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Collision.Filtering;
using Chainbots.Models;

namespace Chainbots.HexBlocks
{
    /// <summary>
    /// Represents a hexagonal block in the physics simulation.
    /// Uses VelcroPhysics for physics simulation and sprites for rendering.
    /// </summary>
    public class HexBlock
    {
        private static int _nextId = 1;
        
        public int Id { get; }
        public HexCoordinate Coordinate { get; }
        public Body? Body { get; private set; }
        public HexBlockType BlockType { get; }
        public float Size { get; }
        public bool IsAnchoredToGround { get; set; }
        
        // For visual-only blocks (like Target), store position without physics
        private Vector2 visualPosition;
        private float visualRotation;

        public HexBlock(World world, HexCoordinate coordinate, float size, HexBlockType blockType, bool isStatic = false)
        {
            Id = _nextId++;
            Coordinate = coordinate;
            Size = size;
            BlockType = blockType;
            IsAnchoredToGround = false;
            
            // Convert hex coordinate to world position
            Vector2 position = coordinate.ToPixel(size);
            
            // Target blocks are visual only - no physics
            if (blockType == HexBlockType.Target)
            {
                visualPosition = position;
                visualRotation = 0f;
                Body = null; // No physics body for target blocks
                return;
            }
            
            // Create physics body for Material blocks
            Body = BodyFactory.CreateBody(world, position, 0f, isStatic ? BodyType.Static : BodyType.Dynamic);
            
            if (!isStatic)
            {
                Body.Mass = 1.0f;
                Body.LinearDamping = 0.5f;
                Body.AngularDamping = 0.5f;
            }
            
            // Create hexagon fixture (approximated as regular polygon)
            var vertices = CreateHexagonVertices(size);
            
            // Create polygon shape
            var shape = new PolygonShape(vertices, 1f);
            var fixture = Body.CreateFixture(shape);
            fixture.Friction = 0.5f;
            fixture.Restitution = 0.3f;
        }

        /// <summary>
        /// Creates the vertices for a hexagon shape.
        /// </summary>
        private Vertices CreateHexagonVertices(float radius)
        {
            var vertices = new Vertices(6);
            
            // Flat-top hexagon: vertices at 0°, 60°, 120°, 180°, 240°, 300°
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.ToRadians(60 * i);
                float x = radius * (float)Math.Cos(angle);
                float y = radius * (float)Math.Sin(angle);
                vertices.Add(new Vector2(x, y));
            }
            
            return vertices;
        }

        /// <summary>
        /// Creates an anchor joint between two hex blocks.
        /// The blocks are connected through their centers.
        /// </summary>
        public static WeldJoint? CreateAnchorJoint(World world, HexBlock blockA, HexBlock blockB)
        {
            if (blockA.Body == null || blockB.Body == null || world == null)
                return null;
            
            // Connect blocks at their centers using local coordinates
            // Vector2.Zero represents the center of each body in local space
            var joint = JointFactory.CreateDistanceJoint(
                world,
                blockA.Body, 
                blockB.Body, 
                Vector2.Zero,  // Local anchor on blockA (at its center)
                Vector2.Zero   // Local anchor on blockB (at its center)
            );
            
            return null;
        }

        /// <summary>
        /// Creates an anchor joint between a hex block and a static ground body.
        /// The block is anchored at its center to a point on the ground directly below it.
        /// </summary>
        public static WeldJoint? CreateGroundAnchorJoint(World world, HexBlock block, Body groundBody)
        {
            if (block.Body == null || groundBody == null || world == null)
                return null;
            
            block.IsAnchoredToGround = true;
            
            // Calculate local anchor points
            // We want to anchor the block at its current world position
            // Ground body is typically at (0, 0), so local anchor on ground = block's world position
            // Block's local anchor = (0, 0) which is its center
            Vector2 localAnchorOnGround = block.Body.Position;
            Vector2 localAnchorOnBlock = groundBody.Position; // Center of the block
            
            // Create a weld joint to rigidly connect the block to ground
            var joint = JointFactory.CreateWeldJoint(
                world,
                groundBody,
                block.Body, 
                localAnchorOnGround,  // Local anchor point on ground body
                localAnchorOnBlock    // Local anchor point on block (its center)
            );
            
            return joint;
        }

        /// <summary>
        /// Sets the collision category and mask for this block.
        /// </summary>
        public void SetCollisionCategory(short category, short collidesWith)
        {
            if (Body == null) return;
            
            foreach (var fixture in Body.FixtureList)
            {
                fixture.CollisionCategories = (Category)category;
                fixture.CollidesWith = (Category)collidesWith;
            }
        }
    }
}

