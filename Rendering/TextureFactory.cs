using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Chainbots.Rendering;

public static class TextureFactory
{
    public static Color[] CreateHexagonTexture(int size)
    {
        int textureSize = size * 2;
        Color[] data = new Color[textureSize * textureSize];

        for (int i = 0; i < data.Length; i++)
            data[i] = Color.Transparent;

        Vector2 center = new Vector2(textureSize / 2f, textureSize / 2f);
        float radius = size;

        Vector2[] vertices = new Vector2[6];
        for (int i = 0; i < 6; i++)
        {
            float angle = MathHelper.ToRadians(60 * i);
            vertices[i] = center + new Vector2(
                radius * (float)Math.Cos(angle),
                radius * (float)Math.Sin(angle)
            );
        }

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                Vector2 point = new Vector2(x + 0.5f, y + 0.5f);
                if (IsPointInPolygon(point, vertices))
                {
                    data[y * textureSize + x] = Color.White;
                }
            }
        }

        return data;
    }

    public static Color[] CreateTriangleTexture(int size)
    {
        int textureSize = size * 2;
        Color[] data = new Color[textureSize * textureSize];

        // Clear background
        for (int i = 0; i < data.Length; i++)
            data[i] = Color.Transparent;

        Vector2 center = new Vector2(textureSize / 2f, textureSize / 2f);
        float radius = size;

        Vector2[] vertices = new Vector2[3];

        float[] angles = { -90f, 150f, 30f };

        for (int i = 0; i < 3; i++)
        {
            float rad = MathHelper.ToRadians(angles[i]);
            vertices[i] = center + new Vector2(
                radius * (float)Math.Cos(rad),
                radius * (float)Math.Sin(rad)
            );
        }

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
                if (IsPointInPolygon(p, vertices))
                    data[y * textureSize + x] = Color.White;
            }
        }

        return data;
    }


    private static bool IsPointInPolygon(Vector2 point, Vector2[] vertices)
    {
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

    public static Color[] CreateSquareTexture(int size)
    {
        int textureSize = size;
        Color[] data = new Color[textureSize * textureSize];

        for (int i = 0; i < data.Length; i++)
            data[i] = Color.White;

        return data;
    }
}

