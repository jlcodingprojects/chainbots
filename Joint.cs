using System;
using SkiaSharp;

namespace Chainbots
{
    /// <summary>
    /// Represents a rotational joint connecting two physics blocks.
    /// </summary>
    public class Joint
    {
        public Guid Id { get; }
        public PhysicsBlock BlockA { get; }
        public PhysicsBlock BlockB { get; }
        public int ConnectionDirectionA { get; }  // 0-5: which edge of block A
        public int ConnectionDirectionB { get; }  // 0-5: which edge of block B
        public float MaxDistance { get; set; }  // Maximum allowed distance between connection points
        public float Stiffness { get; set; }  // How strongly the constraint is enforced (0-1)
        public bool IsEnabled { get; set; }

        public Joint(PhysicsBlock blockA, int directionA, PhysicsBlock blockB, int directionB, float maxDistance = 5f)
        {
            Id = Guid.NewGuid();
            BlockA = blockA;
            BlockB = blockB;
            ConnectionDirectionA = directionA;
            ConnectionDirectionB = directionB;
            MaxDistance = maxDistance;
            Stiffness = 0.8f;
            IsEnabled = true;
        }

        /// <summary>
        /// Applies constraint forces to keep the blocks connected.
        /// </summary>
        public void SolveConstraint()
        {
            if (!IsEnabled) return;

            var (ax, ay) = BlockA.GetConnectionPoint(ConnectionDirectionA);
            var (bx, by) = BlockB.GetConnectionPoint(ConnectionDirectionB);

            // Calculate distance between connection points
            float dx = bx - ax;
            float dy = by - ay;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);

            // If distance exceeds max, apply corrective forces
            if (distance > MaxDistance && distance > 0.001f)
            {
                float error = distance - MaxDistance;
                float correctionStrength = error * Stiffness;

                // Normalized direction vector
                float nx = dx / distance;
                float ny = dy / distance;

                // Calculate correction forces
                float forceX = nx * correctionStrength;
                float forceY = ny * correctionStrength;

                // Apply forces to both blocks (opposite directions)
                if (!BlockA.IsStatic && !BlockA.IsDragging)
                {
                    BlockA.ApplyForce(forceX, forceY, ax, ay);
                }
                if (!BlockB.IsStatic && !BlockB.IsDragging)
                {
                    BlockB.ApplyForce(-forceX, -forceY, bx, by);
                }
            }
        }

        /// <summary>
        /// Renders the joint as a line connecting the two blocks.
        /// </summary>
        public void Render(SKCanvas canvas)
        {
            if (!IsEnabled) return;

            var (ax, ay) = BlockA.GetConnectionPoint(ConnectionDirectionA);
            var (bx, by) = BlockB.GetConnectionPoint(ConnectionDirectionB);

            using var paint = new SKPaint
            {
                Color = SKColors.DarkGray,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2f,
                IsAntialias = true
            };

            canvas.DrawLine(ax, ay, bx, by, paint);

            // Draw joint pivot points
            using var pivotPaint = new SKPaint
            {
                Color = SKColors.Blue,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            canvas.DrawCircle(ax, ay, 4f, pivotPaint);
            canvas.DrawCircle(bx, by, 4f, pivotPaint);
        }

        /// <summary>
        /// Gets the current distance between the connection points.
        /// </summary>
        public float GetCurrentDistance()
        {
            var (ax, ay) = BlockA.GetConnectionPoint(ConnectionDirectionA);
            var (bx, by) = BlockB.GetConnectionPoint(ConnectionDirectionB);
            float dx = bx - ax;
            float dy = by - ay;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
    }
}

