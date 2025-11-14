using System.Collections.Generic;
using SkiaSharp;

namespace Chainbots
{
    /// <summary>
    /// Manages freehand drawing state - paths drawn by the user.
    /// Separated from rendering logic for clean architecture.
    /// </summary>
    public class FreeDraw
    {
        private readonly List<SKPath> paths;
        private SKPath? currentPath;

        public FreeDraw()
        {
            paths = new List<SKPath>();
            currentPath = null;
        }

        /// <summary>
        /// Starts a new drawing path at the specified position.
        /// </summary>
        public void StartPath(float x, float y)
        {
            currentPath = new SKPath();
            currentPath.MoveTo(x, y);
        }

        /// <summary>
        /// Continues the current path to a new position.
        /// </summary>
        public void ContinuePath(float x, float y)
        {
            if (currentPath != null)
            {
                currentPath.LineTo(x, y);
            }
        }

        /// <summary>
        /// Ends the current path and stores it.
        /// </summary>
        public void EndPath()
        {
            if (currentPath != null)
            {
                paths.Add(currentPath);
                currentPath = null;
            }
        }

        /// <summary>
        /// Clears all drawn paths.
        /// </summary>
        public void Clear()
        {
            foreach (var path in paths)
            {
                path.Dispose();
            }
            paths.Clear();

            if (currentPath != null)
            {
                currentPath.Dispose();
                currentPath = null;
            }
        }

        /// <summary>
        /// Renders all drawn paths.
        /// </summary>
        public void Render(SKCanvas canvas)
        {
            using var paint = new SKPaint
            {
                Color = new SKColor(0, 120, 255, 200), // Blue, semi-transparent
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 3f,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round
            };

            // Draw all completed paths
            foreach (var path in paths)
            {
                canvas.DrawPath(path, paint);
            }

            // Draw current path being drawn
            if (currentPath != null)
            {
                canvas.DrawPath(currentPath, paint);
            }
        }

        /// <summary>
        /// Gets the total number of completed paths.
        /// </summary>
        public int PathCount => paths.Count;

        /// <summary>
        /// Checks if currently drawing a path.
        /// </summary>
        public bool IsDrawing => currentPath != null;
    }
}

