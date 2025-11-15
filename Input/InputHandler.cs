using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Chainbots.Rendering;

namespace Chainbots.Input
{
    /// <summary>
    /// Handles keyboard and mouse input.
    /// </summary>
    public class InputHandler : IInputHandler
    {
        private KeyboardState _previousKeyState;
        private MouseState _previousMouseState;

        public InputHandler()
        {
            _previousKeyState = Keyboard.GetState();
            _previousMouseState = Mouse.GetState();
        }

        public void Update(ICamera camera, GameTime gameTime, out bool shouldExit)
        {
            var keyState = Keyboard.GetState();
            var mouseState = Mouse.GetState();

            shouldExit = GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                         keyState.IsKeyDown(Keys.Escape);

            // Camera controls
            const float cameraSpeed = 5f;
            float deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 cameraDelta = Vector2.Zero;
            if (keyState.IsKeyDown(Keys.Left))
                cameraDelta.X -= cameraSpeed * deltaSeconds;
            if (keyState.IsKeyDown(Keys.Right))
                cameraDelta.X += cameraSpeed * deltaSeconds;
            if (keyState.IsKeyDown(Keys.Up))
                cameraDelta.Y -= cameraSpeed * deltaSeconds;
            if (keyState.IsKeyDown(Keys.Down))
                cameraDelta.Y += cameraSpeed * deltaSeconds;

            if (cameraDelta != Vector2.Zero)
                camera.Pan(cameraDelta);

            // Zoom controls
            if (keyState.IsKeyDown(Keys.OemPlus) || keyState.IsKeyDown(Keys.Add))
                camera.AdjustZoom(0.99f);
            if (keyState.IsKeyDown(Keys.OemMinus) || keyState.IsKeyDown(Keys.Subtract))
                camera.AdjustZoom(1.01f);

            // Reset camera
            if (keyState.IsKeyDown(Keys.R) && !_previousKeyState.IsKeyDown(Keys.R))
            {
                camera.Reset();
            }

            _previousKeyState = keyState;
            _previousMouseState = mouseState;
        }
    }
}

