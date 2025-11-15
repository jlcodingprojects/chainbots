using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Chainbots.Models;

namespace Chainbots.UI
{
    /// <summary>
    /// Interface for toolbar operations.
    /// </summary>
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
    }
}

