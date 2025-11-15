using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Chainbots.Rendering
{
    /// <summary>
    /// Creates textures for the application.
    /// </summary>
    public static class TextureFactory
    {
        public static Texture2D CreateHexagonTexture(GraphicsDevice device, int size)
        {
            int textureSize = size * 2;
            Texture2D texture = new Texture2D(device, textureSize, textureSize);
            Color[] data = new Color[textureSize * textureSize];

            // Initialize all pixels to transparent
            for (int i = 0; i < data.Length; i++)
                data[i] = Color.Transparent;

            // Draw proper hexagon using point-in-polygon test
            Vector2 center = new Vector2(textureSize / 2f, textureSize / 2f);
            float radius = size * 0.85f;

            // Calculate hexagon vertices (flat-top orientation)
            // Match the physics vertices for proper alignment
            Vector2[] vertices = new Vector2[6];
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.ToRadians(60 * i + 30);
                vertices[i] = center + new Vector2(
                    radius * (float)Math.Cos(angle),
                    radius * (float)Math.Sin(angle)
                );
            }

            // Fill hexagon
            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    Vector2 point = new Vector2(x + 0.5f, y + 0.5f);
                    if (IsPointInHexagon(point, vertices))
                    {
                        data[y * textureSize + x] = Color.White;
                    }
                }
            }

            texture.SetData(data);
            return texture;
        }

        private static bool IsPointInHexagon(Vector2 point, Vector2[] vertices)
        {
            // Ray casting algorithm for point-in-polygon test
            bool inside = false;
            int j = vertices.Length - 1;

            for (int i = 0; i < vertices.Length; i++)
            {
                if (((vertices[i].Y > point.Y) != (vertices[j].Y > point.Y)) &&
                    (point.X < (vertices[j].X - vertices[i].X) * (point.Y - vertices[i].Y) /
                    (vertices[j].Y - vertices[i].Y) + vertices[i].X))
                {
                    inside = !inside;
                }
                j = i;
            }

            return inside;
        }
    }
}

