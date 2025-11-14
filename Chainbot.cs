using System;
using System.Collections.Generic;
using SkiaSharp;

namespace Chainbots
{
    /// <summary>
    /// Represents a chainbot with a body and two single-segment legs.
    /// 3 sections total: Body + Left Leg + Right Leg
    /// 3 motors: Left Hip + Right Hip + Knee (connecting the two legs)
    /// </summary>
    public class Chainbot
    {
        public Guid Id { get; }
        public string Name { get; set; }
        public PhysicsBlock Body { get; }
        public Leg LeftLeg { get; }
        public Leg RightLeg { get; }
        public Joint LeftHip { get; }   // Motor 1: Body to Left Leg
        public Joint RightHip { get; }  // Motor 2: Body to Right Leg
        public Joint Knee { get; }      // Motor 3: Left Leg to Right Leg (allows angular motion)
        
        // Track which leg is selected for attach/detach
        public Leg? SelectedLeg { get; set; }

        public Chainbot(string name, float centerX, float centerY, float hexSize)
        {
            Id = Guid.NewGuid();
            Name = name;
            
            // Create body (center section)
            Body = new PhysicsBlock(centerX, centerY, hexSize * 0.9f, SKColor.Parse("#4A90E2"));
            
            // Create single-segment legs (positioned below and to sides of body)
            float legOffsetX = hexSize * 1.5f;
            float legOffsetY = hexSize * 2.0f;
            LeftLeg = new Leg("Left", centerX - legOffsetX, centerY + legOffsetY, hexSize, SKColor.Parse("#E74C3C"));
            RightLeg = new Leg("Right", centerX + legOffsetX, centerY + legOffsetY, hexSize, SKColor.Parse("#E74C3C"));
            
            // Create hip joints connecting body to each leg (Motors 1 & 2)
            LeftHip = new Joint(Body, 2, LeftLeg.Block, 4, hexSize * 2.0f);   // SW of body to NW of left leg
            RightHip = new Joint(Body, 0, RightLeg.Block, 3, hexSize * 2.0f); // E of body to W of right leg
            
            // Create knee joint connecting the two legs together (Motor 3)
            // This allows the legs to angle relative to each other
            Knee = new Joint(LeftLeg.Block, 0, RightLeg.Block, 3, hexSize * 3.0f); // Right side of left leg to left side of right leg
        }

        /// <summary>
        /// Attempts to attach a leg to an environment block.
        /// </summary>
        public bool AttachLeg(Leg leg, PhysicsBlock environmentBlock)
        {
            // Can't attach if already attached
            if (leg.IsAttached) return false;
            
            // Find closest connection point on the environment block
            int bestDirection = FindClosestConnectionPoint(leg.Block, environmentBlock);
            
            return leg.AttachTo(environmentBlock, bestDirection, 50f);
        }

        /// <summary>
        /// Detaches a leg from its environment block.
        /// </summary>
        public void DetachLeg(Leg leg)
        {
            leg.Detach();
        }

        /// <summary>
        /// Finds the closest connection point on the target block to the leg.
        /// </summary>
        private int FindClosestConnectionPoint(PhysicsBlock leg, PhysicsBlock target)
        {
            var (footX, footY) = leg.GetConnectionPoint(1); // Bottom of leg
            
            int bestDir = 4; // Default to top (NW)
            float bestDist = float.MaxValue;
            
            for (int dir = 0; dir < 6; dir++)
            {
                var (px, py) = target.GetConnectionPoint(dir);
                float dx = px - footX;
                float dy = py - footY;
                float dist = dx * dx + dy * dy;
                
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestDir = dir;
                }
            }
            
            return bestDir;
        }

        /// <summary>
        /// Updates the chainbot's physics.
        /// </summary>
        public void Update(float deltaTime, int constraintIterations = 5)
        {
            // Update physics for body and legs
            Body.UpdatePhysics(deltaTime);
            LeftLeg.Update(deltaTime);
            RightLeg.Update(deltaTime);
            
            // Solve constraints multiple times for stability
            for (int i = 0; i < constraintIterations; i++)
            {
                LeftHip.SolveConstraint();   // Motor 1
                RightHip.SolveConstraint();  // Motor 2
                Knee.SolveConstraint();      // Motor 3
                LeftLeg.SolveConstraints();
                RightLeg.SolveConstraints();
            }
        }

        /// <summary>
        /// Renders the chainbot.
        /// </summary>
        public void Render(SKCanvas canvas)
        {
            // Render all joints (motors)
            LeftHip.Render(canvas);   // Motor 1
            RightHip.Render(canvas);  // Motor 2
            Knee.Render(canvas);      // Motor 3
            
            // Render legs
            LeftLeg.Render(canvas);
            RightLeg.Render(canvas);
            
            // Render body
            Body.Render(canvas);
            
            // Draw selection indicator if a leg is selected
            if (SelectedLeg != null)
            {
                using var paint = new SKPaint
                {
                    Color = SKColors.Yellow,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 3f,
                    IsAntialias = true
                };
                
                float radius = SelectedLeg.Block.Size + 5f;
                canvas.DrawCircle(SelectedLeg.Block.X, SelectedLeg.Block.Y, radius, paint);
            }
        }

        /// <summary>
        /// Finds which block (if any) is at the given position.
        /// </summary>
        public PhysicsBlock? GetBlockAt(float x, float y)
        {
            if (Body.Contains(x, y)) return Body;
            
            var leftBlock = LeftLeg.GetBlockAt(x, y);
            if (leftBlock != null) return leftBlock;
            
            var rightBlock = RightLeg.GetBlockAt(x, y);
            if (rightBlock != null) return rightBlock;
            
            return null;
        }

        /// <summary>
        /// Finds which leg (if any) is at the given position.
        /// </summary>
        public Leg? GetLegAt(float x, float y)
        {
            if (LeftLeg.Block.Contains(x, y))
                return LeftLeg;
            
            if (RightLeg.Block.Contains(x, y))
                return RightLeg;
            
            return null;
        }

        /// <summary>
        /// Updates hover states for all blocks.
        /// </summary>
        public bool UpdateHoverState(float mouseX, float mouseY)
        {
            bool anyHovered = false;
            
            Body.IsHovered = Body.Contains(mouseX, mouseY);
            if (Body.IsHovered) anyHovered = true;
            
            LeftLeg.Block.IsHovered = LeftLeg.Block.Contains(mouseX, mouseY);
            if (LeftLeg.Block.IsHovered) anyHovered = true;
            
            RightLeg.Block.IsHovered = RightLeg.Block.Contains(mouseX, mouseY);
            if (RightLeg.Block.IsHovered) anyHovered = true;
            
            return anyHovered;
        }
    }
}

