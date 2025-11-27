using Chainbots.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Chainbots.Interfaces;

public interface IHexRenderer
{
    void DrawHexagonOutline(SpriteBatch spriteBatch, HexCoordinate coord, Color color);
    void DrawHexagonOutlineAtWorld(SpriteBatch spriteBatch, Vector2 worldPos, Color color);
    void DrawHexagonFilled(SpriteBatch spriteBatch, HexCoordinate coord, Color color);
    void DrawHexagonFilledAtWorld(SpriteBatch spriteBatch, Vector2 worldPos, Color color);

    void DrawTriangleFilledAtWorld(SpriteBatch spriteBatch, Vector2 worldPos, Color color);

    void DrawGround(SpriteBatch spriteBatch, Vector2 groundPosition);
    void Initialize(float hexSize, float pixelsPerMeter);
}

