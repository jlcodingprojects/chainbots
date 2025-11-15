using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Chainbots.Models;

namespace Chainbots.UI
{
    /// <summary>
    /// Represents a toolbar with buttons for controlling the simulation.
    /// </summary>
    public class Toolbar : IToolbar
    {
        public int Height { get; private set; }
        public SimulationState SimulationState { get; private set; } = SimulationState.Stopped;

        private List<Button> _buttons;
        private Texture2D _texture;
        private SpriteFont? _font;
        private int _viewportWidth;
        private MouseState _previousMouseState;

        private Button _startButton = null!;
        private Button _stopButton = null!;
        private Button _resetButton = null!;

        public event Action? OnStart;
        public event Action? OnStop;
        public event Action? OnReset;

        public Toolbar(Texture2D texture, int viewportWidth, int height = 60, SpriteFont? font = null)
        {
            _texture = texture;
            _viewportWidth = viewportWidth;
            Height = height;
            _font = font;
            _buttons = new List<Button>();
            _previousMouseState = Mouse.GetState();

            InitializeButtons();
        }

        private void InitializeButtons()
        {
            int buttonWidth = 100;
            int buttonHeight = 40;
            int buttonSpacing = 10;
            int startX = 10;
            int startY = (Height - buttonHeight) / 2;

            // Start button
            _startButton = new Button(
                new Rectangle(startX, startY, buttonWidth, buttonHeight),
                "Start",
                () =>
                {
                    SimulationState = SimulationState.Running;
                    OnStart?.Invoke();
                    UpdateButtonStates();
                }
            );
            _buttons.Add(_startButton);

            // Stop button
            _stopButton = new Button(
                new Rectangle(startX + buttonWidth + buttonSpacing, startY, buttonWidth, buttonHeight),
                "Stop",
                () =>
                {
                    SimulationState = SimulationState.Stopped;
                    OnStop?.Invoke();
                    UpdateButtonStates();
                }
            );
            _buttons.Add(_stopButton);

            // Reset button
            _resetButton = new Button(
                new Rectangle(startX + (buttonWidth + buttonSpacing) * 2, startY, buttonWidth, buttonHeight),
                "Reset",
                () =>
                {
                    SimulationState = SimulationState.Stopped;
                    OnReset?.Invoke();
                    UpdateButtonStates();
                }
            );
            _buttons.Add(_resetButton);

            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            // Update button enabled states based on simulation state
            _startButton.IsEnabled = SimulationState != SimulationState.Running;
            _stopButton.IsEnabled = SimulationState == SimulationState.Running;
            _resetButton.IsEnabled = true; // Always enabled
        }

        /// <summary>
        /// Updates the toolbar and handles button interactions.
        /// </summary>
        public void Update()
        {
            MouseState mouseState = Mouse.GetState();

            foreach (var button in _buttons)
            {
                button.Update(mouseState, _previousMouseState);
            }

            _previousMouseState = mouseState;
        }

        /// <summary>
        /// Updates the viewport width (call when window is resized).
        /// </summary>
        public void UpdateViewportWidth(int width)
        {
            _viewportWidth = width;
        }

        /// <summary>
        /// Draws the toolbar and all its buttons.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw toolbar background (full width)
            spriteBatch.Draw(
                _texture,
                new Rectangle(0, 0, _viewportWidth, Height),
                new Color(50, 50, 50, 255)
            );

            // Draw toolbar border (bottom edge)
            int borderThickness = 2;
            spriteBatch.Draw(
                _texture,
                new Rectangle(0, Height - borderThickness, _viewportWidth, borderThickness),
                new Color(80, 80, 80, 255)
            );

            // Draw all buttons
            foreach (var button in _buttons)
            {
                button.Draw(spriteBatch, _texture, _font);
            }
        }

        /// <summary>
        /// Checks if a point is within the toolbar bounds.
        /// </summary>
        public bool ContainsPoint(Point point)
        {
            return new Rectangle(0, 0, _viewportWidth, Height).Contains(point);
        }
    }
}

