using Microsoft.Xna.Framework;
using Chainbots.Rendering;
using Chainbots.HexBlocks;
using System.Collections.Generic;

namespace Chainbots.Input
{
    /// <summary>
    /// Interface for input handling.
    /// </summary>
    public interface IInputHandler
    {
        void BeginFrame();
        void EndFrame();
        void Update(ICamera camera, GameTime gameTime, out bool shouldExit);
        void UpdateDragAndDrop(ICamera camera, List<HexBlock> blocks, out HexBlock? clickedBlock, out Vector2 worldMousePosition);
        bool IsMouseButtonPressed();
        bool IsMouseButtonReleased();
    }
}

