using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace Chainbots
{
    /// <summary>
    /// Manages a collection of physics blocks connected by joints.
    /// </summary>
    public class ChainStructure
    {
        public List<PhysicsBlock> Blocks { get; }
        public List<Joint> Joints { get; }
        public string Name { get; set; }

        public ChainStructure(string name = "Structure")
        {
            Blocks = new List<PhysicsBlock>();
            Joints = new List<Joint>();
            Name = name;
        }

        /// <summary>
        /// Adds a block to the structure.
        /// </summary>
        public void AddBlock(PhysicsBlock block)
        {
            if (!Blocks.Contains(block))
            {
                Blocks.Add(block);
            }
        }

        /// <summary>
        /// Adds a joint connecting two blocks.
        /// </summary>
        public Joint AddJoint(PhysicsBlock blockA, int directionA, PhysicsBlock blockB, int directionB, float maxDistance = 5f)
        {
            // Ensure both blocks are in the structure
            AddBlock(blockA);
            AddBlock(blockB);

            var joint = new Joint(blockA, directionA, blockB, directionB, maxDistance);
            Joints.Add(joint);
            return joint;
        }

        /// <summary>
        /// Removes a block and all associated joints.
        /// </summary>
        public void RemoveBlock(PhysicsBlock block)
        {
            Blocks.Remove(block);
            Joints.RemoveAll(j => j.BlockA == block || j.BlockB == block);
        }

        /// <summary>
        /// Removes a specific joint.
        /// </summary>
        public void RemoveJoint(Joint joint)
        {
            Joints.Remove(joint);
        }

        /// <summary>
        /// Updates physics for all blocks and solves constraints.
        /// </summary>
        public void Update(float deltaTime, int constraintIterations = 5)
        {
            // Update block physics
            foreach (var block in Blocks)
            {
                block.UpdatePhysics(deltaTime);
            }

            // Solve constraints multiple times for stability
            for (int i = 0; i < constraintIterations; i++)
            {
                foreach (var joint in Joints)
                {
                    joint.SolveConstraint();
                }
            }
        }

        /// <summary>
        /// Renders all blocks and joints.
        /// </summary>
        public void Render(SKCanvas canvas)
        {
            // Render joints first (underneath blocks)
            foreach (var joint in Joints)
            {
                joint.Render(canvas);
            }

            // Render blocks
            foreach (var block in Blocks)
            {
                block.Render(canvas);
            }
        }

        /// <summary>
        /// Finds the block at the given position, or null if none.
        /// </summary>
        public PhysicsBlock? FindBlockAt(float x, float y)
        {
            // Search from top (end of list) to bottom
            for (int i = Blocks.Count - 1; i >= 0; i--)
            {
                if (Blocks[i].Contains(x, y))
                {
                    return Blocks[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Brings a block to the front (renders last).
        /// </summary>
        public void BringToFront(PhysicsBlock block)
        {
            if (Blocks.Remove(block))
            {
                Blocks.Add(block);
            }
        }

        /// <summary>
        /// Updates hover state for all blocks based on mouse position.
        /// </summary>
        public bool UpdateHoverState(float mouseX, float mouseY)
        {
            bool anyHovered = false;
            foreach (var block in Blocks)
            {
                block.IsHovered = block.Contains(mouseX, mouseY);
                if (block.IsHovered)
                {
                    anyHovered = true;
                }
            }
            return anyHovered;
        }

        /// <summary>
        /// Snaps a block's position to the nearest grid point.
        /// </summary>
        public void SnapBlockToGrid(PhysicsBlock block, float hexSize)
        {
            // Convert pixel position to hex coordinate
            float sqrt3 = 1.73205080757f;
            float q = (sqrt3 / 3f * block.X - 1f / 3f * block.Y) / hexSize;
            float r = (2f / 3f * block.Y) / hexSize;

            // Round to nearest integer hex coordinate
            int hexQ = (int)Math.Round(q);
            int hexR = (int)Math.Round(r);

            // Convert back to pixel position
            float snapX = hexSize * (sqrt3 * hexQ + sqrt3 / 2f * hexR);
            float snapY = hexSize * (3f / 2f * hexR);

            block.X = snapX;
            block.Y = snapY;
            block.VelocityX = 0;
            block.VelocityY = 0;
            block.AngularVelocity = 0;
        }

        /// <summary>
        /// Creates a simple 2-leg chainbot structure for testing.
        /// </summary>
        public static ChainStructure CreateTwoLegChainbot(float centerX, float centerY, float hexSize)
        {
            var structure = new ChainStructure("Two-Leg Chainbot");

            // Create body (center, smaller)
            var body = new PhysicsBlock(centerX, centerY, hexSize * 0.6f, SKColor.Parse("#4A90E2"));
            structure.AddBlock(body);

            // Create leg A (below body)
            var legA = new PhysicsBlock(
                centerX, 
                centerY + hexSize * 2f, 
                hexSize, 
                SKColor.Parse("#E74C3C")
            );
            structure.AddBlock(legA);

            // Create leg B (below leg A)
            var legB = new PhysicsBlock(
                centerX, 
                centerY + hexSize * 4f, 
                hexSize, 
                SKColor.Parse("#E74C3C")
            );
            structure.AddBlock(legB);

            // Connect body to leg A
            structure.AddJoint(body, 1, legA, 4, hexSize * 1.5f);  // SE of body to NW of legA

            // Connect leg A to leg B
            structure.AddJoint(legA, 1, legB, 4, hexSize * 2f);  // SE of legA to NW of legB

            return structure;
        }

        /// <summary>
        /// Creates a chainbot with 2 parallel legs (like in the reference image).
        /// </summary>
        public static ChainStructure CreateParallelLegChainbot(float centerX, float centerY, float hexSize)
        {
            var structure = new ChainStructure("Parallel-Leg Chainbot");

            // Create body (center, smaller)
            var body = new PhysicsBlock(centerX, centerY, hexSize * 0.7f, SKColor.Parse("#4A90E2"));
            structure.AddBlock(body);

            // Left leg chain
            var leftLeg1 = new PhysicsBlock(
                centerX - hexSize * 1.5f, 
                centerY + hexSize * 1.5f, 
                hexSize, 
                SKColor.Parse("#E74C3C")
            );
            structure.AddBlock(leftLeg1);

            var leftLeg2 = new PhysicsBlock(
                centerX - hexSize * 1.5f, 
                centerY + hexSize * 3.5f, 
                hexSize, 
                SKColor.Parse("#E74C3C")
            );
            structure.AddBlock(leftLeg2);

            // Right leg chain
            var rightLeg1 = new PhysicsBlock(
                centerX + hexSize * 1.5f, 
                centerY + hexSize * 1.5f, 
                hexSize, 
                SKColor.Parse("#E74C3C")
            );
            structure.AddBlock(rightLeg1);

            var rightLeg2 = new PhysicsBlock(
                centerX + hexSize * 1.5f, 
                centerY + hexSize * 3.5f, 
                hexSize, 
                SKColor.Parse("#E74C3C")
            );
            structure.AddBlock(rightLeg2);

            // Connect body to both legs
            structure.AddJoint(body, 2, leftLeg1, 4, hexSize * 1.2f);   // SW of body to NW of left leg
            structure.AddJoint(body, 0, rightLeg1, 3, hexSize * 1.2f);  // E of body to W of right leg

            // Connect leg segments
            structure.AddJoint(leftLeg1, 1, leftLeg2, 4, hexSize * 2f);   // SE to NW
            structure.AddJoint(rightLeg1, 1, rightLeg2, 4, hexSize * 2f); // SE to NW

            return structure;
        }
    }
}

