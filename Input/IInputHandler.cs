using Microsoft.Xna.Framework;
using Chainbots.Rendering;

namespace Chainbots.Input
{
    /// <summary>
    /// Interface for input handling.
    /// </summary>
    public interface IInputHandler
    {
        void Update(ICamera camera, GameTime gameTime, out bool shouldExit);
    }
}

