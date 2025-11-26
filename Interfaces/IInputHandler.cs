using Microsoft.Xna.Framework;
using Chainbots.HexBlocks;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace Chainbots.Interfaces;

public interface IInputHandler
{
    void BeginFrame();
    void EndFrame();
    void Update(ICamera camera, GameTime gameTime, out bool shouldExit);
    void UpdateDragAndDrop(ICamera camera, List<HexBlock> blocks, out HexBlock? clickedBlock, out Vector2 worldMousePosition);
    bool GetMouseEvent(MouseButton button, InputState state);
}

public enum MouseButton
{
    Left,
    Right,
    Middle,
}

public enum InputState
{
    Pressed,
    Released,
    Held,
}