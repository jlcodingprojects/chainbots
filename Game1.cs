using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.MonoGame.DebugView;
using Genbox.VelcroPhysics.Collision.Shapes;

namespace Chainbots
{
    /// <summary>
    /// Main game class for the Chainbots simulation using MonoGame and VelcroPhysics.
    /// </summary>
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch? _spriteBatch;
        private DebugView? _debugView;
        
        // Physics world
        private World? _world;
        private const float FixedTimeStep = 1f / 60f;
        private float _accumulator = 0f;
        
        // Camera
        private Matrix _view;
        private Matrix _projection;
        private float _cameraZoom = 1.0f;
        private Vector2 _cameraPosition;
        
        // Hex blocks
        private List<HexBlock> _targetBlocks;
        private List<HexBlock> _materialBlocks;
        private List<HexBlock> _anchorBlocks;
        
        // Constants
        private const float HexSize = 0.5f; // meters in physics world
        private const float PixelsPerMeter = 60f;
        
        // Input state
        private KeyboardState _previousKeyState;
        private MouseState _previousMouseState;
        
        // Textures
        private Texture2D? _hexTexture;
        
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            
            _targetBlocks = new List<HexBlock>();
            _materialBlocks = new List<HexBlock>();
            _anchorBlocks = new List<HexBlock>();
            
            _cameraPosition = Vector2.Zero;
        }

        protected override void Initialize()
        {
            base.Initialize();
            
            // Initialize physics world with gravity
            _world = new World(new Vector2(0f, 9.8f)); // 9.8 m/s^2 gravity
            
            // Set up camera matrices
            UpdateCamera();
            
            // Initialize debug view (optional - requires font content)
            try
            {
                _debugView = new DebugView(_world);
                _debugView.LoadContent(GraphicsDevice, Content);
                _debugView.DefaultShapeColor = Color.White;
            }
            catch (Exception)
            {
                // DebugView requires content files, which we haven't set up
                // The simulation will work fine without it
                _debugView = null;
            }
            
            // Initialize hex blocks
            InitializeHexGrid();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // Create hexagon texture
            _hexTexture = CreateHexagonTexture(GraphicsDevice, (int)(HexSize * PixelsPerMeter));
        }

        private void InitializeHexGrid()
        {
            if (_world == null) return;
            
            // Create target grid (skeleton view) - larger structure
            for (int q = -4; q <= 4; q++)
            {
                for (int r = -4; r <= 4; r++)
                {
                    if (Math.Abs(q) + Math.Abs(r) <= 4)
                    {
                        var targetBlock = new HexBlock(
                            _world,
                            new HexCoordinate(q, r),
                            HexSize,
                            HexBlockType.Target,
                            isStatic: true
                        );
                        _targetBlocks.Add(targetBlock);
                    }
                }
            }
            
            // Create anchor blocks (locked to ground) - positioned off to the side
            // These are part of the material group
            for (int i = 0; i < 3; i++)
            {
                var anchorBlock = new HexBlock(
                    _world,
                    new HexCoordinate(-8, i - 1),
                    HexSize,
                    HexBlockType.Anchor,
                    isStatic: true
                );
                _anchorBlocks.Add(anchorBlock);
            }
            
            // Create material blocks - smaller starting structure with physics enabled
            // These will have gravity and can move
            for (int q = -2; q <= 2; q++)
            {
                for (int r = -2; r <= 2; r++)
                {
                    if (Math.Abs(q) + Math.Abs(r) <= 2)
                    {
                        var materialBlock = new HexBlock(
                            _world,
                            new HexCoordinate(q, r),
                            HexSize,
                            HexBlockType.Material,
                            isStatic: false
                        );
                        _materialBlocks.Add(materialBlock);
                    }
                }
            }
            
            // Connect material blocks with anchor joints
            ConnectHexBlocks(_materialBlocks);
            
            // Connect some material blocks to anchor blocks
            if (_anchorBlocks.Count > 0 && _materialBlocks.Count > 0)
            {
                // Find material blocks near the anchors and connect them
                foreach (var anchorBlock in _anchorBlocks)
                {
                    // Find nearby material blocks
                    foreach (var materialBlock in _materialBlocks)
                    {
                        float distance = Vector2.Distance(
                            anchorBlock.Coordinate.ToPixel(HexSize),
                            materialBlock.Coordinate.ToPixel(HexSize)
                        );
                        
                        // Connect if within reasonable range
                        if (distance < HexSize * 5f)
                        {
                            HexBlock.CreateAnchorJoint(_world, anchorBlock, materialBlock);
                        }
                    }
                }
            }
            
            // Set collision groups
            SetupCollisionGroups();
        }

        private void ConnectHexBlocks(List<HexBlock> blocks)
        {
            // Connect neighboring hex blocks with anchor joints
            foreach (var blockA in blocks)
            {
                foreach (var blockB in blocks)
                {
                    if (blockA == blockB) continue;
                    
                    // Check if they are neighbors
                    for (int dir = 0; dir < 6; dir++)
                    {
                        var neighbor = blockA.Coordinate.GetNeighbor(dir);
                        if (neighbor.Equals(blockB.Coordinate))
                        {
                            // Create anchor joint between neighbors
                            HexBlock.CreateAnchorJoint(_world, blockA, blockB);
                            break;
                        }
                    }
                }
            }
        }

        private void SetupCollisionGroups()
        {
            // Set up collision categories and masks
            // Target blocks: Category 0x0001 (only collide with nothing - visual only)
            // Material blocks: Category 0x0002 (collide with material and anchors)
            // Anchor blocks: Category 0x0004 (collide with material)
            
            foreach (var block in _targetBlocks)
            {
                block.SetCollisionCategory(0x0001, 0x0000);
            }
            
            foreach (var block in _materialBlocks)
            {
                block.SetCollisionCategory(0x0002, 0x0002 | 0x0004);
            }
            
            foreach (var block in _anchorBlocks)
            {
                block.SetCollisionCategory(0x0004, 0x0002 | 0x0004);
            }
        }

        private void UpdateCamera()
        {
            float viewportWidth = _graphics.PreferredBackBufferWidth / PixelsPerMeter;
            float viewportHeight = _graphics.PreferredBackBufferHeight / PixelsPerMeter;
            
            _projection = Matrix.CreateOrthographicOffCenter(
                -viewportWidth / 2f * _cameraZoom + _cameraPosition.X,
                viewportWidth / 2f * _cameraZoom + _cameraPosition.X,
                viewportHeight / 2f * _cameraZoom + _cameraPosition.Y,
                -viewportHeight / 2f * _cameraZoom + _cameraPosition.Y,
                -1f,
                1f
            );
            
            _view = Matrix.Identity;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || 
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var keyState = Keyboard.GetState();
            var mouseState = Mouse.GetState();
            
            // Camera controls
            const float cameraSpeed = 5f;
            if (keyState.IsKeyDown(Keys.Left))
                _cameraPosition.X -= cameraSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (keyState.IsKeyDown(Keys.Right))
                _cameraPosition.X += cameraSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (keyState.IsKeyDown(Keys.Up))
                _cameraPosition.Y -= cameraSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (keyState.IsKeyDown(Keys.Down))
                _cameraPosition.Y += cameraSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            // Zoom controls
            if (keyState.IsKeyDown(Keys.OemPlus) || keyState.IsKeyDown(Keys.Add))
                _cameraZoom *= 0.99f;
            if (keyState.IsKeyDown(Keys.OemMinus) || keyState.IsKeyDown(Keys.Subtract))
                _cameraZoom *= 1.01f;
            
            // Reset camera
            if (keyState.IsKeyDown(Keys.R) && !_previousKeyState.IsKeyDown(Keys.R))
            {
                _cameraPosition = Vector2.Zero;
                _cameraZoom = 1.0f;
            }
            
            UpdateCamera();
            
            // Fixed timestep physics update
            _accumulator += (float)gameTime.ElapsedGameTime.TotalSeconds;
            while (_accumulator >= FixedTimeStep)
            {
                _world?.Step(FixedTimeStep);
                _accumulator -= FixedTimeStep;
            }
            
            _previousKeyState = keyState;
            _previousMouseState = mouseState;
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            if (_spriteBatch == null || _hexTexture == null || _debugView == null) return;
            
            // Draw hex blocks as sprites
            _spriteBatch.Begin(
                sortMode: SpriteSortMode.Deferred,
                blendState: BlendState.AlphaBlend,
                samplerState: SamplerState.LinearClamp,
                depthStencilState: null,
                rasterizerState: null,
                effect: null,
                transformMatrix: _view * _projection * 
                    Matrix.CreateScale(PixelsPerMeter, PixelsPerMeter, 1f) *
                    Matrix.CreateTranslation(_graphics.PreferredBackBufferWidth / 2f, _graphics.PreferredBackBufferHeight / 2f, 0f)
            );
            
            // Draw target blocks (skeleton)
            foreach (var block in _targetBlocks)
            {
                block.Draw(_spriteBatch, _hexTexture, new Color(150, 150, 150, 80));
            }
            
            // Draw anchor blocks
            foreach (var block in _anchorBlocks)
            {
                block.Draw(_spriteBatch, _hexTexture, new Color(100, 100, 100, 255));
            }
            
            // Draw material blocks (solid)
            foreach (var block in _materialBlocks)
            {
                block.Draw(_spriteBatch, _hexTexture, new Color(45, 45, 45, 255));
            }
            
            _spriteBatch.End();
            
            // Draw physics debug view (if available)
            _debugView?.RenderDebugData(_projection, _view);
            
            base.Draw(gameTime);
        }

        private Texture2D CreateHexagonTexture(GraphicsDevice device, int size)
        {
            int textureSize = size * 2;
            Texture2D texture = new Texture2D(device, textureSize, textureSize);
            Color[] data = new Color[textureSize * textureSize];
            
            // Initialize all pixels to transparent
            for (int i = 0; i < data.Length; i++)
                data[i] = Color.Transparent;
            
            // Draw hexagon
            Vector2 center = new Vector2(textureSize / 2f, textureSize / 2f);
            float radius = size * 0.9f;
            
            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    if (IsInsideHexagon(pos, center, radius))
                    {
                        data[y * textureSize + x] = Color.White;
                    }
                }
            }
            
            texture.SetData(data);
            return texture;
        }

        private bool IsInsideHexagon(Vector2 point, Vector2 center, float radius)
        {
            Vector2 diff = point - center;
            
            // Use simplified circle check for now (can be made more accurate)
            float distance = diff.Length();
            return distance <= radius;
        }
    }
}
