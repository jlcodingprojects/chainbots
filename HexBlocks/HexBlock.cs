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
        public HexCoordinate Coordinate { get; }
        public Body? Body { get; private set; }
        public HexBlockType BlockType { get; }
        public float Size { get; }
        
        // For visual-only blocks (like Target), store position without physics
        private Vector2 visualPosition;
        private float visualRotation;

        public HexBlock(World world, HexCoordinate coordinate, float size, HexBlockType blockType, bool isStatic = false)
        {
            Coordinate = coordinate;
            Size = size;
            BlockType = blockType;
            
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
            
            // Create physics body for Material and Anchor blocks
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
            
            // Flat-top hexagon
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.ToRadians(60 * i - 30);
                float x = radius * (float)Math.Cos(angle);
                float y = radius * (float)Math.Sin(angle);
                vertices.Add(new Vector2(x, y));
            }
            
            return vertices;
        }

        /// <summary>
        /// Creates an anchor joint between two hex blocks.
        /// </summary>
        public static WeldJoint? CreateAnchorJoint(World world, HexBlock blockA, HexBlock blockB)
        {
            if (blockA.Body == null || blockB.Body == null || world == null)
                return null;
            
            // Create a weld joint to rigidly connect the blocks
            var joint = JointFactory.CreateWeldJoint(
                world,
                blockA.Body, 
                blockB.Body, 
                blockA.Body.Position, 
                blockB.Body.Position
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

        /// <summary>
        /// Draws the hex block as a sprite.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, Texture2D texture, Color color)
        {
            // Use visual position for Target blocks, physics body position for others
            Vector2 position = (BlockType == HexBlockType.Target) ? visualPosition : Body.Position;
            float rotation = (BlockType == HexBlockType.Target) ? visualRotation : Body.Rotation;
            
            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
            float scale = (Size * 2f) / texture.Width;
            
            spriteBatch.Draw(
                texture,
                position,
                null,
                color,
                rotation,
                origin,
                scale,
                SpriteEffects.None,
                0f
            );
            
            // Draw outline for target blocks
            if (BlockType == HexBlockType.Target)
            {
                // Draw a thin border (would need a separate texture or shader for proper outline)
                spriteBatch.Draw(
                    texture,
                    position,
                    null,
                    new Color(200, 200, 200, 150),
                    rotation,
                    origin,
                    scale * 1.05f,
                    SpriteEffects.None,
                    0f
                );
            }
        }

        /// <summary>
        /// Gets the world position of a connection point on the hexagon.
        /// </summary>
        public Vector2 GetConnectionPoint(int direction)
        {
            Vector2 position = (BlockType == HexBlockType.Target) ? visualPosition : Body.Position;
            float rotation = (BlockType == HexBlockType.Target) ? visualRotation : Body.Rotation;
            
            float angle = MathHelper.ToRadians(60 * direction + rotation * (180f / MathF.PI));
            float x = position.X + Size * (float)Math.Cos(angle);
            float y = position.Y + Size * (float)Math.Sin(angle);
            return new Vector2(x, y);
        }
    }
}

