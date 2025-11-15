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
            _hexGridManager.Initialize();
            _physicsWorld.CreateGround();
            _physicsWorld.SetupCollisionGroups(
                _hexGridManager.TargetBlocks,
                _hexGridManager.MaterialBlocks,
                _hexGridManager.AnchorBlocks
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

        protected override void Update(GameTime gameTime)
        {
            // Handle input
            _inputHandler.Update(_camera, gameTime, out bool shouldExit);
            if (shouldExit)
                Exit();
            
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
            
            // Draw anchor blocks (light grey)
            foreach (var block in _hexGridManager.AnchorBlocks)
            {
                _hexRenderer.DrawHexagonFilled(_spriteBatch, block.Coordinate, new Color(180, 180, 180, 255));
            }
            
            // Draw material blocks (light grey)
            foreach (var block in _hexGridManager.MaterialBlocks)
            {
                _hexRenderer.DrawHexagonFilledAtWorld(_spriteBatch, block.Body.Position, new Color(200, 200, 200, 255));
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
            
            _spriteBatch.End();
            
            // Draw physics debug view (if available)
            _debugView?.RenderDebugData(_camera.Projection, _camera.View);
            
            base.Draw(gameTime);
        }
    }
}

