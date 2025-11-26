using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Chainbots.Models;

namespace Chainbots.Rendering
{
    /// <summary>
    /// Interface for hexagon rendering operations.
    /// </summary>
    public interface IHexRenderer
    {
        void DrawHexagonOutline(SpriteBatch spriteBatch, HexCoordinate coord, Color color, bool dotted);
        void DrawHexagonOutlineAtWorld(SpriteBatch spriteBatch, Vector2 worldPos, Color color, bool dotted);
        void DrawHexagonFilled(SpriteBatch spriteBatch, HexCoordinate coord, Color color);
        void DrawHexagonFilledAtWorld(SpriteBatch spriteBatch, Vector2 worldPos, Color color);
        void DrawGround(SpriteBatch spriteBatch, Vector2 groundPosition);
    }
}

