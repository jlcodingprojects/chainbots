using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Chainbots.HexBlocks;
using System.Collections.Generic;
using Chainbots.Interfaces;

namespace Chainbots.Input;

public class InputHandler : IInputHandler
{
    private KeyboardState _previousKeyState;
    private KeyboardState _currentKeyState;
    private MouseState _previousMouseState;
    private MouseState _currentMouseState;

    public InputHandler()
    {
        _previousKeyState = Keyboard.GetState();
        _currentKeyState = _previousKeyState;
        _previousMouseState = Mouse.GetState();
        _currentMouseState = _previousMouseState;
    }

    public void BeginFrame()
    {
        _currentKeyState = Keyboard.GetState();
        _currentMouseState = Mouse.GetState();
    }

    public void EndFrame()
    {
        _previousKeyState = _currentKeyState;
        _previousMouseState = _currentMouseState;
    }

    public void Update(ICamera camera, GameTime gameTime, out bool shouldExit)
    {
        shouldExit = GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                     _currentKeyState.IsKeyDown(Keys.Escape);

        // Camera controls
        const float cameraSpeed = 5f;
        float deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

        Vector2 cameraDelta = Vector2.Zero;
        if (_currentKeyState.IsKeyDown(Keys.Left))
            cameraDelta.X -= cameraSpeed * deltaSeconds;
        if (_currentKeyState.IsKeyDown(Keys.Right))
            cameraDelta.X += cameraSpeed * deltaSeconds;
        if (_currentKeyState.IsKeyDown(Keys.Up))
            cameraDelta.Y -= cameraSpeed * deltaSeconds;
        if (_currentKeyState.IsKeyDown(Keys.Down))
            cameraDelta.Y += cameraSpeed * deltaSeconds;

        if (cameraDelta != Vector2.Zero)
            camera.Pan(cameraDelta);

        // Zoom controls
        if (_currentKeyState.IsKeyDown(Keys.OemPlus) || _currentKeyState.IsKeyDown(Keys.Add))
            camera.AdjustZoom(0.99f);
        if (_currentKeyState.IsKeyDown(Keys.OemMinus) || _currentKeyState.IsKeyDown(Keys.Subtract))
            camera.AdjustZoom(1.01f);

        // Reset camera
        if (_currentKeyState.IsKeyDown(Keys.R) && !_previousKeyState.IsKeyDown(Keys.R))
        {
            camera.Reset();
        }
    }

    public void UpdateDragAndDrop(ICamera camera, List<HexBlock> blocks, out HexBlock? clickedBlock, out Vector2 worldMousePosition)
    {
        Vector2 mouseScreenPos = new Vector2(_currentMouseState.X, _currentMouseState.Y);
        worldMousePosition = camera.ScreenToWorld(mouseScreenPos);
        
        clickedBlock = null;

        // Check if left mouse button was just pressed
        if (GetMouseEvent(MouseButton.Left, InputState.Pressed))
        {
            // Find the block under the mouse
            float minDistance = float.MaxValue;
            const float hexRadius = 0.5f; // Match HexSize constant
            
            foreach (var block in blocks)
            {
                if (block.Body == null) continue;
                
                float distance = Vector2.Distance(worldMousePosition, block.Body.Position);
                
                // Check if within hex radius and it's the closest one
                if (distance < hexRadius && distance < minDistance)
                {
                    minDistance = distance;
                    clickedBlock = block;
                }
            }
        }
    }

    public bool GetMouseEvent(MouseButton button, InputState state)
        => button switch
        {
            MouseButton.Left => state switch
            {
                InputState.Pressed => _currentMouseState.LeftButton == ButtonState.Pressed &&
                                      _previousMouseState.LeftButton == ButtonState.Released,
                InputState.Released => _currentMouseState.LeftButton == ButtonState.Released &&
                                       _previousMouseState.LeftButton == ButtonState.Pressed,
                _ => false,
            },
            MouseButton.Right => state switch
            {
                InputState.Pressed => _currentMouseState.RightButton == ButtonState.Pressed &&
                                      _previousMouseState.RightButton == ButtonState.Released,
                InputState.Released => _currentMouseState.RightButton == ButtonState.Released &&
                                       _previousMouseState.RightButton == ButtonState.Pressed,
                _ => false,
            },
            _ => false,
        };
}

