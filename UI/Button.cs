using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Chainbots.UI
{
    /// <summary>
    /// Represents a clickable button in the UI.
    /// </summary>
    public class Button
    {
        public Rectangle Bounds { get; set; }
        public string Text { get; set; }
        public Action? OnClick { get; set; }
        public bool IsEnabled { get; set; } = true;

        private bool _wasPressed = false;
        private Color _normalColor = new Color(60, 60, 60, 255);
        private Color _hoverColor = new Color(80, 80, 80, 255);
        private Color _pressedColor = new Color(40, 40, 40, 255);
        private Color _disabledColor = new Color(40, 40, 40, 255);
        private Color _textColor = new Color(220, 220, 220, 255);
        private Color _disabledTextColor = new Color(100, 100, 100, 255);

        public Button(Rectangle bounds, string text, Action? onClick = null)
        {
            Bounds = bounds;
            Text = text;
            OnClick = onClick;
        }

        /// <summary>
        /// Updates the button state and handles click detection.
        /// </summary>
        public void Update(MouseState mouseState, MouseState previousMouseState)
        {
            if (!IsEnabled)
            {
                _wasPressed = false;
                return;
            }

            Point mousePosition = new Point(mouseState.X, mouseState.Y);
            bool isHovered = Bounds.Contains(mousePosition);
            bool isPressed = mouseState.LeftButton == ButtonState.Pressed;
            bool wasPressed = previousMouseState.LeftButton == ButtonState.Pressed;

            // Detect click (mouse released while hovering)
            if (isHovered && !isPressed && wasPressed && _wasPressed)
            {
                OnClick?.Invoke();
            }

            _wasPressed = isHovered && isPressed;
        }

        /// <summary>
        /// Draws the button.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, Texture2D texture, SpriteFont? font = null)
        {
            // Determine button color based on state
            MouseState mouseState = Mouse.GetState();
            Point mousePosition = new Point(mouseState.X, mouseState.Y);
            bool isHovered = Bounds.Contains(mousePosition);
            bool isPressed = mouseState.LeftButton == ButtonState.Pressed && isHovered;

            Color backgroundColor;
            Color textColor;

            if (!IsEnabled)
            {
                backgroundColor = _disabledColor;
                textColor = _disabledTextColor;
            }
            else if (isPressed)
            {
                backgroundColor = _pressedColor;
                textColor = _textColor;
            }
            else if (isHovered)
            {
                backgroundColor = _hoverColor;
                textColor = _textColor;
            }
            else
            {
                backgroundColor = _normalColor;
                textColor = _textColor;
            }

            // Draw button background
            spriteBatch.Draw(texture, Bounds, backgroundColor);

            // Draw button border
            int borderThickness = 2;
            Color borderColor = new Color(100, 100, 100, 255);
            
            // Top border
            spriteBatch.Draw(texture, new Rectangle(Bounds.X, Bounds.Y, Bounds.Width, borderThickness), borderColor);
            // Bottom border
            spriteBatch.Draw(texture, new Rectangle(Bounds.X, Bounds.Y + Bounds.Height - borderThickness, Bounds.Width, borderThickness), borderColor);
            // Left border
            spriteBatch.Draw(texture, new Rectangle(Bounds.X, Bounds.Y, borderThickness, Bounds.Height), borderColor);
            // Right border
            spriteBatch.Draw(texture, new Rectangle(Bounds.X + Bounds.Width - borderThickness, Bounds.Y, borderThickness, Bounds.Height), borderColor);

            // Draw button text (if font is provided)
            if (font != null && !string.IsNullOrEmpty(Text))
            {
                Vector2 textSize = font.MeasureString(Text);
                Vector2 textPosition = new Vector2(
                    Bounds.X + (Bounds.Width - textSize.X) / 2,
                    Bounds.Y + (Bounds.Height - textSize.Y) / 2
                );
                spriteBatch.DrawString(font, Text, textPosition, textColor);
            }
        }
    }
}

