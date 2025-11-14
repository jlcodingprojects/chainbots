using System;
using SkiaSharp;

namespace Chainbots
{
    /// <summary>
    /// A demo hex block that can be positioned and rotated freely using pixel coordinates.
    /// Used for visualization/planning. Completely independent from the hex mesh coordinate system.
    /// </summary>
    public class DemoHexBlock
    {
        public Guid Id { get; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Rotation { get; set; } // Rotation in degrees
        public float Size { get; set; }
        public bool IsHovered { get; set; }

        public DemoHexBlock(float x, float y, float size)
        {
            Id = Guid.NewGuid();
            X = x;
            Y = y;
            Size = size;
            Rotation = 0f;
            IsHovered = false;
        }

        /// <summary>
        /// Checks if a point is inside this hex block.
        /// </summary>
        public bool Contains(float x, float y)
        {
            // Simple distance check for hit testing (slightly larger for easier interaction)
            float dx = x - X;
            float dy = y - Y;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);
            return distance <= Size * 1.2f; // 20% larger hit area
        }

        /// <summary>
        /// Renders the demo hex block as a red, semi-transparent hexagon.
        /// </summary>
        public void Render(SKCanvas canvas)
        {
            canvas.Save();
            
            // Translate to position
            canvas.Translate(X, Y);
            
            // Apply rotation
            canvas.RotateDegrees(Rotation);

            // Create hexagon path
            using var path = new SKPath();
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

            // Fill with red, semi-transparent (brighter when hovered)
            byte alpha = IsHovered ? (byte)180 : (byte)128;
            using var fillPaint = new SKPaint
            {
                Color = new SKColor(255, 0, 0, alpha),
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };
            canvas.DrawPath(path, fillPaint);

            // Border (thicker when hovered)
            float strokeWidth = IsHovered ? 3f : 2f;
            using var strokePaint = new SKPaint
            {
                Color = new SKColor(200, 0, 0, 255), // Darker red, fully opaque
                Style = SKPaintStyle.Stroke,
                StrokeWidth = strokeWidth,
                IsAntialias = true
            };
            canvas.DrawPath(path, strokePaint);

            canvas.Restore();
        }
    }
}

