using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Chainbots.Models;

namespace Chainbots.Rendering
{
    /// <summary>
    /// Handles all hexagon rendering operations.
    /// </summary>
    public class HexRenderer : IHexRenderer
    {
        private readonly Texture2D _hexTexture;
        private readonly ICamera _camera;
        private readonly float _hexSize;
        private readonly float _pixelsPerMeter;

        public HexRenderer(Texture2D hexTexture, ICamera camera, float hexSize, float pixelsPerMeter)
        {
            _hexTexture = hexTexture;
            _camera = camera;
            _hexSize = hexSize;
            _pixelsPerMeter = pixelsPerMeter;
        }

        public void DrawHexagonOutline(SpriteBatch spriteBatch, HexCoordinate coord, Color color, bool dotted)
        {
            Vector2 worldPos = coord.ToPixel(_hexSize);
            Vector2 screenPos = _camera.WorldToScreen(worldPos);

            float screenRadius = _hexSize * _pixelsPerMeter;

            // Draw hexagon outline - match physics and texture orientation
            Vector2[] vertices = new Vector2[6];
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.ToRadians(60 * i + 30);
                vertices[i] = screenPos + new Vector2(
                    screenRadius * (float)Math.Cos(angle),
                    screenRadius * (float)Math.Sin(angle)
                );
            }

            // Draw lines between vertices
            for (int i = 0; i < 6; i++)
            {
                Vector2 start = vertices[i];
                Vector2 end = vertices[(i + 1) % 6];

                if (dotted)
                {
                    DrawDottedLine(spriteBatch, start, end, color, 5f);
                }
                else
                {
                    DrawLine(spriteBatch, start, end, color, 2f);
                }
            }
        }

        public void DrawHexagonFilled(SpriteBatch spriteBatch, HexCoordinate coord, Color color)
        {
            Vector2 worldPos = coord.ToPixel(_hexSize);
            DrawHexagonFilledAtWorld(spriteBatch, worldPos, color);
        }

        public void DrawHexagonFilledAtWorld(SpriteBatch spriteBatch, Vector2 worldPos, Color color)
        {
            Vector2 screenPos = _camera.WorldToScreen(worldPos);
            float screenRadius = _hexSize * _pixelsPerMeter;

            // Draw filled hexagon using the texture
            Vector2 origin = new Vector2(_hexTexture.Width / 2f, _hexTexture.Height / 2f);
            float scale = (screenRadius * 2f) / _hexTexture.Width;

            spriteBatch.Draw(
                _hexTexture,
                screenPos,
                null,
                color,
                0f,
                origin,
                scale,
                SpriteEffects.None,
                0f
            );
        }

        private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);
            float length = edge.Length();

            spriteBatch.Draw(
                _hexTexture,
                start,
                null,
                color,
                angle,
                new Vector2(0, _hexTexture.Height / 2f),
                new Vector2(length, thickness),
                SpriteEffects.None,
                0f
            );
        }

        private void DrawDottedLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float dotSpacing)
        {
            Vector2 direction = end - start;
            float length = direction.Length();
            direction.Normalize();

            float distance = 0f;
            while (distance < length)
            {
                Vector2 dotPos = start + direction * distance;
                spriteBatch.Draw(
                    _hexTexture,
                    dotPos,
                    null,
                    color,
                    0f,
                    new Vector2(_hexTexture.Width / 2f, _hexTexture.Height / 2f),
                    new Vector2(2f / _hexTexture.Width, 2f / _hexTexture.Height),
                    SpriteEffects.None,
                    0f
                );
                distance += dotSpacing;
            }
        }

        public void DrawGround(SpriteBatch spriteBatch, Vector2 groundPosition)
        {
            Vector2 screenPos = _camera.WorldToScreen(groundPosition);

            // Draw wide ground rectangle
            float groundWidth = 200f * _pixelsPerMeter; // 200 meters wide in screen pixels
            float groundHeight = 1f * _pixelsPerMeter; // 1 meter thick

            // Draw ground as a solid rectangle
            Rectangle groundRect = new Rectangle(
                (int)(screenPos.X - groundWidth / 2),
                (int)(screenPos.Y - groundHeight / 2),
                (int)groundWidth,
                (int)groundHeight
            );

            spriteBatch.Draw(
                _hexTexture,
                groundRect,
                new Color(60, 60, 60, 255) // Slightly lighter than background
            );
        }
    }
}

