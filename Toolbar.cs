using System;
using System.Collections.Generic;
using SkiaSharp;

namespace Chainbots
{
    /// <summary>
    /// Manages toolbar rendering and button interactions.
    /// Separated from main window for clean architecture.
    /// </summary>
    public class Toolbar
    {
        private readonly List<ToolbarButton> buttons;
        private ToolbarButton? hoveredButton;

        public Toolbar()
        {
            buttons = new List<ToolbarButton>();
        }

        /// <summary>
        /// Adds a button to the toolbar.
        /// </summary>
        public void AddButton(string label, Action onClick)
        {
            buttons.Add(new ToolbarButton
            {
                Label = label,
                OnClick = onClick,
                Bounds = SKRect.Empty
            });
        }

        /// <summary>
        /// Renders the toolbar at the top of the canvas.
        /// </summary>
        public void Render(SKCanvas canvas, int canvasWidth)
        {
            const float toolbarHeight = 50f;
            const float buttonWidth = 100f;
            const float buttonHeight = 30f;
            const float buttonSpacing = 10f;
            const float startX = 10f;
            const float startY = 10f;

            // Draw toolbar background
            using var bgPaint = new SKPaint
            {
                Color = SKColor.Parse("#F0F0F0"),
                Style = SKPaintStyle.Fill
            };
            canvas.DrawRect(0, 0, canvasWidth, toolbarHeight, bgPaint);

            // Draw toolbar border
            using var borderPaint = new SKPaint
            {
                Color = SKColor.Parse("#CCCCCC"),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1f
            };
            canvas.DrawLine(0, toolbarHeight, canvasWidth, toolbarHeight, borderPaint);

            // Draw buttons
            float currentX = startX;
            for (int i = 0; i < buttons.Count; i++)
            {
                var button = buttons[i];
                button.Bounds = new SKRect(currentX, startY, currentX + buttonWidth, startY + buttonHeight);
                
                RenderButton(canvas, button, button == hoveredButton);
                
                currentX += buttonWidth + buttonSpacing;
            }
        }

        private void RenderButton(SKCanvas canvas, ToolbarButton button, bool isHovered)
        {
            // Button background
            var bgColor = isHovered ? SKColor.Parse("#E0E0E0") : SKColors.White;
            using var bgPaint = new SKPaint
            {
                Color = bgColor,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };
            canvas.DrawRoundRect(button.Bounds, 4f, 4f, bgPaint);

            // Button border
            using var borderPaint = new SKPaint
            {
                Color = SKColor.Parse("#999999"),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1.5f,
                IsAntialias = true
            };
            canvas.DrawRoundRect(button.Bounds, 4f, 4f, borderPaint);

            // Button text
            using var textPaint = new SKPaint
            {
                Color = SKColor.Parse("#333333"),
                TextSize = 14f,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Normal),
                TextAlign = SKTextAlign.Center
            };

            float textX = button.Bounds.MidX;
            float textY = button.Bounds.MidY + 5f; // Offset for vertical centering
            canvas.DrawText(button.Label, textX, textY, textPaint);
        }

        /// <summary>
        /// Handles mouse move events for hover effects.
        /// </summary>
        public bool HandleMouseMove(float x, float y)
        {
            ToolbarButton? newHovered = null;
            foreach (var button in buttons)
            {
                if (button.Bounds.Contains(x, y))
                {
                    newHovered = button;
                    break;
                }
            }

            bool changed = newHovered != hoveredButton;
            hoveredButton = newHovered;
            return changed;
        }

        /// <summary>
        /// Handles mouse click events.
        /// Returns true if a button was clicked.
        /// </summary>
        public bool HandleMouseClick(float x, float y)
        {
            foreach (var button in buttons)
            {
                if (button.Bounds.Contains(x, y))
                {
                    button.OnClick?.Invoke();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the height of the toolbar.
        /// </summary>
        public float GetHeight() => 50f;

        private class ToolbarButton
        {
            public string Label { get; set; } = string.Empty;
            public Action? OnClick { get; set; }
            public SKRect Bounds { get; set; }
        }
    }
}

