using Chainbots.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Chainbots.UI;

public interface IToolbar
{
    int Height { get; }
    SimulationState SimulationState { get; }
    event System.Action? OnStart;
    event System.Action? OnStop;
    event System.Action? OnReset;
    void Update();
    void UpdateViewportWidth(int width);
    void Draw(SpriteBatch spriteBatch);
    bool ContainsPoint(Point point);
    void Initialize(int viewportWidth, int height = 60, SpriteFont? font = null);
}

