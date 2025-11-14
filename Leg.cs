using System;
using SkiaSharp;

namespace Chainbots
{
    /// <summary>
    /// Represents a single-segment leg that can attach/detach from environment blocks.
    /// </summary>
    public class Leg
    {
        public string Name { get; set; }
        public PhysicsBlock Block { get; }
        public PhysicsBlock? AttachedBlock { get; private set; }
        public Joint? AttachmentJoint { get; private set; }
        public bool IsAttached => AttachedBlock != null;

        public Leg(string name, float startX, float startY, float hexSize, SKColor color)
        {
            Name = name;
            
            // Create single leg block
            Block = new PhysicsBlock(startX, startY, hexSize, color);
        }

        /// <summary>
        /// Attaches the leg to an environment block.
        /// </summary>
        public bool AttachTo(PhysicsBlock block, int blockConnectionDir = 4, float maxDistance = 5f)
        {
            if (IsAttached) return false;

            AttachedBlock = block;
            // Attach leg's bottom (direction 1 = SE) to block's connection point
            AttachmentJoint = new Joint(Block, 1, block, blockConnectionDir, maxDistance);
            
            // Make leg static while attached (it becomes the pivot point)
            Block.IsStatic = true;
            
            return true;
        }

        /// <summary>
        /// Detaches the leg from the environment block.
        /// </summary>
        public void Detach()
        {
            if (!IsAttached) return;

            AttachedBlock = null;
            AttachmentJoint = null;
            
            // Make leg dynamic again
            Block.IsStatic = false;
        }

        /// <summary>
        /// Updates the leg's physics.
        /// </summary>
        public void Update(float deltaTime)
        {
            Block.UpdatePhysics(deltaTime);
        }

        /// <summary>
        /// Solves constraints for this leg.
        /// </summary>
        public void SolveConstraints()
        {
            AttachmentJoint?.SolveConstraint();
        }

        /// <summary>
        /// Renders the leg.
        /// </summary>
        public void Render(SKCanvas canvas)
        {
            // Render attachment joint first
            AttachmentJoint?.Render(canvas);
            
            // Render leg block
            Block.Render(canvas);
            
            // Draw attachment indicator if attached
            if (IsAttached && AttachedBlock != null)
            {
                var (footX, footY) = Block.GetConnectionPoint(1);
                using var paint = new SKPaint
                {
                    Color = SKColors.Green,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                };
                canvas.DrawCircle(footX, footY, 6f, paint);
            }
        }

        /// <summary>
        /// Checks if a point is on this leg.
        /// </summary>
        public PhysicsBlock? GetBlockAt(float x, float y)
        {
            if (Block.Contains(x, y)) return Block;
            return null;
        }
    }
}

