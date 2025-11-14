using System;
using SkiaSharp;

namespace Chainbots
{
    /// <summary>
    /// Represents a hexagonal block with physics properties.
    /// </summary>
    public class PhysicsBlock
    {
        public Guid Id { get; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Rotation { get; set; }  // In degrees
        public float VelocityX { get; set; }
        public float VelocityY { get; set; }
        public float AngularVelocity { get; set; }  // Degrees per second
        public float Mass { get; set; }
        public float Size { get; set; }  // Hex radius
        public SKColor Color { get; set; }
        public bool IsStatic { get; set; }  // Fixed in place (e.g., ground)
        public bool IsDragging { get; set; }
        public bool IsHovered { get; set; }

        public PhysicsBlock(float x, float y, float size, SKColor color, bool isStatic = false)
        {
            Id = Guid.NewGuid();
            X = x;
            Y = y;
            Rotation = 0f;
            VelocityX = 0f;
            VelocityY = 0f;
            AngularVelocity = 0f;
            Mass = isStatic ? float.PositiveInfinity : 1f;
            Size = size;
            Color = color;
            IsStatic = isStatic;
            IsDragging = false;
            IsHovered = false;
        }

        /// <summary>
        /// Checks if a point is inside this hexagonal block.
        /// </summary>
        public bool Contains(float pointX, float pointY)
        {
            // Simple distance check (circle approximation)
            float dx = pointX - X;
            float dy = pointY - Y;
            float distSq = dx * dx + dy * dy;
            return distSq <= Size * Size;
        }

        /// <summary>
        /// Gets the position of a connection point on the hexagon edge.
        /// </summary>
        /// <param name="direction">Direction 0-5 (E, SE, SW, W, NW, NE)</param>
        public (float x, float y) GetConnectionPoint(int direction)
        {
            // Connection points are at the edge midpoints
            float angle = (float)(Math.PI / 180 * (60 * direction + Rotation));
            float x = X + Size * (float)Math.Cos(angle);
            float y = Y + Size * (float)Math.Sin(angle);
            return (x, y);
        }

        /// <summary>
        /// Applies a force to the block at a specific point (for constraint solving).
        /// </summary>
        public void ApplyForce(float forceX, float forceY, float pointX, float pointY)
        {
            if (IsStatic || IsDragging) return;

            // Linear acceleration
            VelocityX += forceX / Mass;
            VelocityY += forceY / Mass;

            // Torque (for rotation)
            float dx = pointX - X;
            float dy = pointY - Y;
            float torque = dx * forceY - dy * forceX;
            AngularVelocity += torque / (Mass * Size * Size);
        }

        /// <summary>
        /// Updates the block's position based on velocity.
        /// </summary>
        public void UpdatePhysics(float deltaTime)
        {
            if (IsStatic || IsDragging) return;

            // Update position
            X += VelocityX * deltaTime;
            Y += VelocityY * deltaTime;
            Rotation += AngularVelocity * deltaTime;

            // Apply damping
            float damping = 0.95f;
            VelocityX *= damping;
            VelocityY *= damping;
            AngularVelocity *= damping;
        }

        /// <summary>
        /// Renders the block.
        /// </summary>
        public void Render(SKCanvas canvas)
        {
            canvas.Save();
            canvas.Translate(X, Y);
            canvas.RotateDegrees(Rotation);

            using var path = new SKPath();
            
            // Create flat-top hexagon
            for (int i = 0; i < 6; i++)
            {
                float angle = (float)(Math.PI / 180 * (60 * i - 30));
                float px = Size * (float)Math.Cos(angle);
                float py = Size * (float)Math.Sin(angle);

                if (i == 0)
                    path.MoveTo(px, py);
                else
                    path.LineTo(px, py);
            }
            path.Close();

            // Fill
            SKColor fillColor = Color;
            if (IsHovered && !IsDragging)
            {
                fillColor = new SKColor(
                    (byte)Math.Min(255, Color.Red + 40),
                    (byte)Math.Min(255, Color.Green + 40),
                    (byte)Math.Min(255, Color.Blue + 40),
                    Color.Alpha
                );
            }

            using var fillPaint = new SKPaint
            {
                Color = fillColor,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };
            canvas.DrawPath(path, fillPaint);

            // Stroke
            using var strokePaint = new SKPaint
            {
                Color = IsDragging ? SKColors.Yellow : SKColors.Black,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = IsDragging ? 3f : 2f,
                IsAntialias = true
            };
            canvas.DrawPath(path, strokePaint);

            // Draw connection points
            if (IsHovered || IsDragging)
            {
                using var dotPaint = new SKPaint
                {
                    Color = SKColors.Red,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                };

                for (int i = 0; i < 6; i++)
                {
                    float angle = (float)(Math.PI / 180 * (60 * i));
                    float px = Size * (float)Math.Cos(angle);
                    float py = Size * (float)Math.Sin(angle);
                    canvas.DrawCircle(px, py, 3f, dotPaint);
                }
            }

            canvas.Restore();
        }
    }
}

