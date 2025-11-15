using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Genbox.VelcroPhysics.MonoGame.DebugView;
using Chainbots.Models;
using Chainbots.HexBlocks;
using Chainbots.Physics;
using Chainbots.Rendering;
using Chainbots.UI;
using Chainbots.Input;

namespace Chainbots.Core
{
    /// <summary>
    /// Main game class for the Chainbots simulation using MonoGame and VelcroPhysics.
    /// </summary>
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private readonly IPhysicsWorld _physicsWorld;
        private readonly IHexGridManager _hexGridManager;
        private readonly ICamera _camera;
        private readonly IInputHandler _inputHandler;
        
        private SpriteBatch? _spriteBatch;
        private DebugView? _debugView;
        private Texture2D? _hexTexture;
        private IToolbar? _toolbar;
        private IHexRenderer? _hexRenderer;
        private SpriteFont? _hudFont;
        
        private bool _isSimulationRunning = false;
        private HexBlock? _hoveredBlock = null;
        
        // Constants
        private const float HexSize = 0.5f; // meters in physics world
        private const float PixelsPerMeter = 60f;

        public Game1(
            IPhysicsWorld physicsWorld,
            IHexGridManager hexGridManager,
            ICamera camera,
            IInputHandler inputHandler)
        {
            _graphics = new GraphicsDeviceManager(this);
            _physicsWorld = physicsWorld;
            _hexGridManager = hexGridManager;
            _camera = camera;
            _inputHandler = inputHandler;
            
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            // Higher resolution for better quality
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.PreferMultiSampling = true; // Enable MSAA for anti-aliasing
        }

        protected override void Initialize()
        {
            // Update camera viewport
            _camera.UpdateViewport(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            
            // Initialize debug view (optional - requires font content)
            try
            {
                _debugView = new DebugView(_physicsWorld.World);
                _debugView.LoadContent(GraphicsDevice, Content);
                _debugView.DefaultShapeColor = Color.White;
            }
            catch (Exception)
            {
                // DebugView requires content files, which we haven't set up
                // The simulation will work fine without it
                _debugView = null;
            }
            
            // Initialize hex grid and physics
            InitializeSimulation();
            
            base.Initialize();
        }

        private void InitializeSimulation()
        {
            _physicsWorld.CreateGround();
            _hexGridManager.Initialize();
            _physicsWorld.SetupCollisionGroups(
                _hexGridManager.TargetBlocks,
                _hexGridManager.MaterialBlocks
            );
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // Create hexagon texture at higher resolution for better quality
            _hexTexture = TextureFactory.CreateHexagonTexture(GraphicsDevice, 128);
            
            // Load font for UI
            try
            {
                _hudFont = Content.Load<SpriteFont>("Fonts/Hud");
            }
            catch (Exception)
            {
                // Font loading failed, toolbar will work without text
                _hudFont = null;
            }
            
            // Initialize toolbar
            _toolbar = new Toolbar(_hexTexture, GraphicsDevice.Viewport.Width, 60, _hudFont);
            _toolbar.OnStart += OnSimulationStart;
            _toolbar.OnStop += OnSimulationStop;
            _toolbar.OnReset += OnSimulationReset;
            
            // Initialize hex renderer
            _hexRenderer = new HexRenderer(_hexTexture, _camera, HexSize, PixelsPerMeter);
        }
        
        private void OnSimulationStart()
        {
            _isSimulationRunning = true;
        }
        
        private void OnSimulationStop()
        {
            _isSimulationRunning = false;
        }
        
        private void OnSimulationReset()
        {
            _isSimulationRunning = false;
            
            // Clear physics world
            _physicsWorld.Clear();
            
            // Reinitialize
            InitializeSimulation();
        }

        private void UpdateHoverDetection()
        {
            var mouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();
            Vector2 mouseScreenPos = new Vector2(mouseState.X, mouseState.Y);
            Vector2 mouseWorldPos = _camera.ScreenToWorld(mouseScreenPos);
            
            // Check if mouse is over any material block
            _hoveredBlock = null;
            float minDistance = float.MaxValue;
            
            foreach (var block in _hexGridManager.MaterialBlocks)
            {
                if (block.Body == null) continue;
                
                float distance = Vector2.Distance(mouseWorldPos, block.Body.Position);
                
                // Check if within hex radius
                if (distance < HexSize && distance < minDistance)
                {
                    minDistance = distance;
                    _hoveredBlock = block;
                }
            }
        }

        protected override void Update(GameTime gameTime)
        {
            // Handle input
            _inputHandler.Update(_camera, gameTime, out bool shouldExit);
            if (shouldExit)
                Exit();
            
            // Update hover detection
            UpdateHoverDetection();
            
            // Update toolbar
            _toolbar?.Update();
            
            // Fixed timestep physics update (only if simulation is running)
            if (_isSimulationRunning)
            {
                _physicsWorld.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            }
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Dark grey background
            GraphicsDevice.Clear(new Color(30, 30, 30));

            if (_spriteBatch == null || _hexTexture == null || _hexRenderer == null) return;
            
            // Draw world elements (ground, target blocks, physics blocks)
            _spriteBatch.Begin(
                sortMode: SpriteSortMode.Deferred,
                blendState: BlendState.AlphaBlend,
                samplerState: SamplerState.AnisotropicClamp, // Better quality for scaling
                depthStencilState: null,
                rasterizerState: null
            );
            
            // Draw ground
            if (_physicsWorld.GroundBody != null)
            {
                _hexRenderer.DrawGround(_spriteBatch, _physicsWorld.GroundBody.Position);
            }
            
            // Draw target blocks as dotted outlines
            foreach (var block in _hexGridManager.TargetBlocks)
            {
                _hexRenderer.DrawHexagonOutline(_spriteBatch, block.Coordinate, new Color(100, 100, 100, 150), true);
            }
            
            _spriteBatch.End();
            
            // Draw physics blocks as sprites
            _spriteBatch.Begin(
                sortMode: SpriteSortMode.Deferred,
                blendState: BlendState.AlphaBlend,
                samplerState: SamplerState.AnisotropicClamp, // Better quality
                depthStencilState: null,
                rasterizerState: null
            );
            
            // Draw material blocks
            foreach (var block in _hexGridManager.MaterialBlocks)
            {
                if (block.Body == null) continue; // Skip blocks without physics bodies
                
                // Use different color for anchored blocks
                Color blockColor = block.IsAnchoredToGround 
                    ? new Color(150, 150, 200, 255)  // Bluish-grey for anchored blocks
                    : new Color(200, 200, 200, 255); // Light grey for regular material blocks
                    
                _hexRenderer.DrawHexagonFilledAtWorld(_spriteBatch, block.Body.Position, blockColor);
            }
            
            _spriteBatch.End();
            
            // Draw UI elements on top
            _spriteBatch.Begin(
                sortMode: SpriteSortMode.Deferred,
                blendState: BlendState.AlphaBlend,
                samplerState: SamplerState.LinearClamp,
                depthStencilState: null,
                rasterizerState: null
            );
            
            // Draw toolbar
            _toolbar?.Draw(_spriteBatch);
            
            // Draw hovered block info
            DrawHoveredBlockInfo(_spriteBatch);
            
            _spriteBatch.End();
            
            // Draw physics debug view (if available)
            _debugView?.RenderDebugData(_camera.Projection, _camera.View);
            
            base.Draw(gameTime);
        }

        private void DrawHoveredBlockInfo(SpriteBatch spriteBatch)
        {
            if (_hoveredBlock == null || _hudFont == null) return;
            
            // Get block information
            Vector2 position = _hoveredBlock.Body?.Position ?? Vector2.Zero;
            float rotation = _hoveredBlock.Body?.Rotation ?? 0f;
            int id = _hoveredBlock.Id;
            
            // Convert rotation to degrees
            float rotationDegrees = MathHelper.ToDegrees(rotation);
            
            // Format the text
            string infoText = $"Block #{id}\n" +
                             $"X: {position.X:F2} m\n" +
                             $"Y: {position.Y:F2} m\n" +
                             $"Rotation: {rotationDegrees:F1}°";
            
            // Position info box near the mouse (top-right of screen for visibility)
            Vector2 infoPosition = new Vector2(
                _graphics.PreferredBackBufferWidth - 250,
                100
            );
            
            // Draw background box
            var mouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();
            Vector2 textSize = _hudFont.MeasureString(infoText);
            Rectangle backgroundRect = new Rectangle(
                (int)infoPosition.X - 10,
                (int)infoPosition.Y - 10,
                (int)textSize.X + 20,
                (int)textSize.Y + 20
            );
            
            // Create a simple background texture if we don't have one
            if (_hexTexture != null)
            {
                // Draw a semi-transparent dark background
                DrawRectangle(spriteBatch, backgroundRect, new Color(0, 0, 0, 200));
            }
            
            // Draw the text
            spriteBatch.DrawString(_hudFont, infoText, infoPosition, Color.White);
        }

        private void DrawRectangle(SpriteBatch spriteBatch, Rectangle rect, Color color)
        {
            // Create a 1x1 white texture for drawing rectangles
            Texture2D pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
            
            spriteBatch.Draw(pixel, rect, color);
        }
    }
}

