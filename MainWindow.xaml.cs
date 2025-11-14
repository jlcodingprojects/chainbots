using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;

namespace Chainbots
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer renderTimer;
        private DateTime lastFrameTime;
        private readonly HexMesh targetMesh;
        private readonly HexMesh materialMesh;
        private readonly Toolbar toolbar;
        private readonly List<DemoHexBlock> demoBlocks;
        private readonly FreeDraw freeDraw;
        private readonly List<ChainStructure> chainStructures;
        private readonly List<Chainbot> chainbots;
        private readonly List<PhysicsBlock> environmentBlocks;
        private readonly float hexSize = 30f;
        private bool isSimulationRunning = false;
        private DemoHexBlock? draggedBlock = null;
        private PhysicsBlock? draggedPhysicsBlock = null;
        private ChainStructure? draggedBlockStructure = null;
        private Chainbot? draggedChainbot = null;
        private float dragOffsetX = 0f;
        private float dragOffsetY = 0f;
        private float canvasScaleX = 1f;
        private float canvasScaleY = 1f;
        private bool isFreeDrawing = false;
        private bool physicsEnabled = true;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize hex meshes
            targetMesh = new HexMesh();
            materialMesh = new HexMesh();
            InitializeTestMeshes();

            // Initialize demo blocks list
            demoBlocks = new List<DemoHexBlock>();

            // Initialize chain structures list
            chainStructures = new List<ChainStructure>();

            // Initialize chainbots list
            chainbots = new List<Chainbot>();

            // Initialize environment blocks list
            environmentBlocks = new List<PhysicsBlock>();

            // Initialize free draw
            freeDraw = new FreeDraw();

            // Initialize toolbar
            toolbar = new Toolbar();
            toolbar.AddButton("Start Sim", OnStartSimulation);
            toolbar.AddButton("Stop Sim", OnStopSimulation);
            toolbar.AddButton("Reset", OnResetSimulation);
            toolbar.AddButton("Add Chainbot", OnAddChainbot);
            toolbar.AddButton("Add Ground", OnAddEnvironmentBlock);
            toolbar.AddButton("Attach Leg (A)", OnAttachLeg);
            toolbar.AddButton("Detach Leg (D)", OnDetachLeg);
            toolbar.AddButton("Clear Draw", OnClearFreeDraw);

            // Wire up mouse events
            skiaCanvas.MouseMove += OnCanvasMouseMove;
            skiaCanvas.MouseLeftButtonDown += OnCanvasMouseDown;
            skiaCanvas.MouseLeftButtonUp += OnCanvasMouseUp;
            skiaCanvas.MouseRightButtonDown += OnCanvasMouseRightDown;
            skiaCanvas.MouseWheel += OnCanvasMouseWheel;
            
            // Wire up keyboard events
            this.KeyDown += OnKeyDown;

            // Set up render loop (60 FPS target)
            renderTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1000.0 / 60.0)
            };
            renderTimer.Tick += RenderFrame;
            renderTimer.Start();

            lastFrameTime = DateTime.Now;
        }

        private void InitializeTestMeshes()
        {
            // Target mesh - what the chainbots should build (larger structure)
            for (int q = -4; q <= 4; q++)
            {
                for (int r = -4; r <= 4; r++)
                {
                    if (Math.Abs(q) + Math.Abs(r) <= 4)
                    {
                        targetMesh.AddHexagon(new HexCoordinate(q, r));
                    }
                }
            }

            // Material mesh - currently placed blocks (smaller starting structure)
            for (int q = -2; q <= 2; q++)
            {
                for (int r = -2; r <= 2; r++)
                {
                    if (Math.Abs(q) + Math.Abs(r) <= 2)
                    {
                        materialMesh.AddHexagon(new HexCoordinate(q, r));
                    }
                }
            }
        }

        private void OnStartSimulation()
        {
            isSimulationRunning = true;
        }

        private void OnStopSimulation()
        {
            isSimulationRunning = false;
        }

        private void OnResetSimulation()
        {
            isSimulationRunning = false;
            materialMesh.Clear();
            chainStructures.Clear();
            chainbots.Clear();
            environmentBlocks.Clear();
            // Reset to initial state
            for (int q = -2; q <= 2; q++)
            {
                for (int r = -2; r <= 2; r++)
                {
                    if (Math.Abs(q) + Math.Abs(r) <= 2)
                    {
                        materialMesh.AddHexagon(new HexCoordinate(q, r));
                    }
                }
            }
        }

        private void OnAddChainbot()
        {
            // Add a chainbot at center of screen
            float centerX = (float)skiaCanvas.ActualWidth / 2f * canvasScaleX;
            float centerY = (float)skiaCanvas.ActualHeight / 2f * canvasScaleY + 50f;
            var bot = new Chainbot($"Bot-{chainbots.Count + 1}", centerX, centerY, hexSize);
            chainbots.Add(bot);
        }

        private void OnAddEnvironmentBlock()
        {
            // Add an environment block (ground) at center of screen
            float centerX = (float)skiaCanvas.ActualWidth / 2f * canvasScaleX;
            float centerY = (float)skiaCanvas.ActualHeight / 2f * canvasScaleY;
            var block = new PhysicsBlock(centerX, centerY, hexSize, SKColor.Parse("#95A5A6"), isStatic: true);
            environmentBlocks.Add(block);
        }

        private void OnAttachLeg()
        {
            // Find the first chainbot with a selected leg
            foreach (var bot in chainbots)
            {
                if (bot.SelectedLeg != null && !bot.SelectedLeg.IsAttached)
                {
                    // Find closest environment block
                    var closestBlock = FindClosestEnvironmentBlock(bot.SelectedLeg.Block);
                    if (closestBlock != null)
                    {
                        bot.AttachLeg(bot.SelectedLeg, closestBlock);
                    }
                    break;
                }
            }
        }

        private void OnDetachLeg()
        {
            // Find the first chainbot with a selected leg
            foreach (var bot in chainbots)
            {
                if (bot.SelectedLeg != null && bot.SelectedLeg.IsAttached)
                {
                    bot.DetachLeg(bot.SelectedLeg);
                    break;
                }
            }
        }

        private PhysicsBlock? FindClosestEnvironmentBlock(PhysicsBlock reference)
        {
            PhysicsBlock? closest = null;
            float closestDist = float.MaxValue;

            foreach (var block in environmentBlocks)
            {
                float dx = block.X - reference.X;
                float dy = block.Y - reference.Y;
                float dist = dx * dx + dy * dy;

                if (dist < closestDist && dist < 10000f) // Only consider blocks within reasonable range
                {
                    closestDist = dist;
                    closest = block;
                }
            }

            return closest;
        }

        private void OnAddDemoBlock()
        {
            // Add a new demo block at center of screen
            // Canvas center in WPF DIPs, converted to SkiaSharp physical pixels
            float centerX = (float)skiaCanvas.ActualWidth / 2f * canvasScaleX;
            float centerY = (float)skiaCanvas.ActualHeight / 2f * canvasScaleY;
            // Use base hex size - no scaling needed as we're in physical pixel space
            demoBlocks.Add(new DemoHexBlock(centerX, centerY, hexSize));
        }

        private void OnClearFreeDraw()
        {
            freeDraw.Clear();
        }

        private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // A key - Attach selected leg
            if (e.Key == System.Windows.Input.Key.A)
            {
                OnAttachLeg();
                skiaCanvas.InvalidateVisual();
            }
            // D key - Detach selected leg
            else if (e.Key == System.Windows.Input.Key.D)
            {
                OnDetachLeg();
                skiaCanvas.InvalidateVisual();
            }
            // Tab key - Cycle selected leg
            else if (e.Key == System.Windows.Input.Key.Tab)
            {
                foreach (var bot in chainbots)
                {
                    if (bot.SelectedLeg == bot.LeftLeg)
                    {
                        bot.SelectedLeg = bot.RightLeg;
                    }
                    else if (bot.SelectedLeg == bot.RightLeg)
                    {
                        bot.SelectedLeg = null;
                    }
                    else
                    {
                        bot.SelectedLeg = bot.LeftLeg;
                    }
                }
                skiaCanvas.InvalidateVisual();
            }
        }

        private void OnCanvasMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var pos = e.GetPosition(skiaCanvas);
            // Convert WPF DIPs to SkiaSharp physical pixels
            float mouseX = (float)pos.X * canvasScaleX;
            float mouseY = (float)pos.Y * canvasScaleY;

            bool needsRedraw = false;

            // Handle toolbar hover (toolbar uses physical pixels)
            if (toolbar.HandleMouseMove(mouseX, mouseY))
            {
                needsRedraw = true;
            }

            // Handle freedraw mode
            if (isFreeDrawing && e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                freeDraw.ContinuePath(mouseX, mouseY);
                needsRedraw = true;
            }
            // Handle physics block dragging
            else if (draggedPhysicsBlock != null)
            {
                draggedPhysicsBlock.X = mouseX - dragOffsetX;
                draggedPhysicsBlock.Y = mouseY - dragOffsetY;
                needsRedraw = true;
            }
            // Handle demo block dragging
            else if (draggedBlock != null)
            {
                draggedBlock.X = mouseX - dragOffsetX;
                draggedBlock.Y = mouseY - dragOffsetY;
                needsRedraw = true;
            }
            else
            {
                // Update hover state for chainbots
                bool anyChainbotHovered = false;
                foreach (var bot in chainbots)
                {
                    if (bot.UpdateHoverState(mouseX, mouseY))
                    {
                        anyChainbotHovered = true;
                        needsRedraw = true;
                    }
                }

                // Update hover state for environment blocks
                bool anyEnvBlockHovered = false;
                foreach (var block in environmentBlocks)
                {
                    bool wasHovered = block.IsHovered;
                    block.IsHovered = block.Contains(mouseX, mouseY);
                    if (block.IsHovered)
                    {
                        anyEnvBlockHovered = true;
                    }
                    if (wasHovered != block.IsHovered)
                    {
                        needsRedraw = true;
                    }
                }

                // Update hover state for physics blocks
                bool anyPhysicsHovered = false;
                foreach (var structure in chainStructures)
                {
                    if (structure.UpdateHoverState(mouseX, mouseY))
                    {
                        anyPhysicsHovered = true;
                        needsRedraw = true;
                    }
                }

                // Update hover state for demo blocks
                bool anyHovered = false;
                foreach (var block in demoBlocks)
                {
                    bool wasHovered = block.IsHovered;
                    block.IsHovered = block.Contains(mouseX, mouseY);
                    if (block.IsHovered)
                    {
                        anyHovered = true;
                    }
                    if (wasHovered != block.IsHovered)
                    {
                        needsRedraw = true;
                    }
                }

                // Update cursor based on mode and hover state
                if (isFreeDrawing)
                {
                    skiaCanvas.Cursor = System.Windows.Input.Cursors.Pen;
                }
                else
                {
                    skiaCanvas.Cursor = (anyHovered || anyPhysicsHovered || anyChainbotHovered || anyEnvBlockHovered) ? System.Windows.Input.Cursors.Hand : System.Windows.Input.Cursors.Arrow;
                }
            }

            if (needsRedraw)
            {
                skiaCanvas.InvalidateVisual();
            }
        }

        private void OnCanvasMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(skiaCanvas);
            // Convert WPF DIPs to SkiaSharp physical pixels
            float mouseX = (float)pos.X * canvasScaleX;
            float mouseY = (float)pos.Y * canvasScaleY;

            // Check toolbar first
            if (toolbar.HandleMouseClick(mouseX, mouseY))
            {
                skiaCanvas.InvalidateVisual();
                return;
            }

            // Check if Ctrl key is pressed for freedraw mode
            if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl) || 
                System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.RightCtrl))
            {
                isFreeDrawing = true;
                freeDraw.StartPath(mouseX, mouseY);
                skiaCanvas.Cursor = System.Windows.Input.Cursors.Pen;
                skiaCanvas.InvalidateVisual();
                return;
            }

            // Check if clicking on a chainbot (iterate backwards)
            for (int i = chainbots.Count - 1; i >= 0; i--)
            {
                var bot = chainbots[i];
                var block = bot.GetBlockAt(mouseX, mouseY);
                if (block != null)
                {
                    // Check if clicking on a leg to select it
                    var leg = bot.GetLegAt(mouseX, mouseY);
                    if (leg != null)
                    {
                        bot.SelectedLeg = leg;
                    }
                    
                    draggedPhysicsBlock = block;
                    draggedChainbot = bot;
                    dragOffsetX = mouseX - block.X;
                    dragOffsetY = mouseY - block.Y;
                    block.IsDragging = true;
                    
                    // Move bot to top of list (render last = on top)
                    chainbots.RemoveAt(i);
                    chainbots.Add(bot);
                    
                    skiaCanvas.InvalidateVisual();
                    return;
                }
            }

            // Check if clicking on an environment block
            for (int i = environmentBlocks.Count - 1; i >= 0; i--)
            {
                var block = environmentBlocks[i];
                if (block.Contains(mouseX, mouseY))
                {
                    draggedPhysicsBlock = block;
                    dragOffsetX = mouseX - block.X;
                    dragOffsetY = mouseY - block.Y;
                    block.IsDragging = true;
                    
                    // Move to top of list (render last = on top)
                    environmentBlocks.RemoveAt(i);
                    environmentBlocks.Add(block);
                    
                    skiaCanvas.InvalidateVisual();
                    return;
                }
            }

            // Check if clicking on a physics block (iterate structures backwards)
            for (int i = chainStructures.Count - 1; i >= 0; i--)
            {
                var structure = chainStructures[i];
                var block = structure.FindBlockAt(mouseX, mouseY);
                if (block != null)
                {
                    draggedPhysicsBlock = block;
                    draggedBlockStructure = structure;
                    dragOffsetX = mouseX - block.X;
                    dragOffsetY = mouseY - block.Y;
                    block.IsDragging = true;
                    
                    // Move structure to top of list (render last = on top)
                    chainStructures.RemoveAt(i);
                    chainStructures.Add(structure);
                    structure.BringToFront(block);
                    
                    skiaCanvas.InvalidateVisual();
                    return;
                }
            }

            // Check if clicking on a demo block (iterate backwards to get topmost)
            for (int i = demoBlocks.Count - 1; i >= 0; i--)
            {
                var block = demoBlocks[i];
                if (block.Contains(mouseX, mouseY))
                {
                    draggedBlock = block;
                    dragOffsetX = mouseX - block.X;
                    dragOffsetY = mouseY - block.Y;
                    
                    // Move to top of list (render last = on top)
                    demoBlocks.RemoveAt(i);
                    demoBlocks.Add(block);
                    
                    skiaCanvas.InvalidateVisual();
                    return;
                }
            }
        }

        private void OnCanvasMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (isFreeDrawing)
            {
                freeDraw.EndPath();
                isFreeDrawing = false;
                skiaCanvas.InvalidateVisual();
            }
            else if (draggedPhysicsBlock != null)
            {
                draggedPhysicsBlock.IsDragging = false;
                
                // Snap to grid if Shift key is held
                if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftShift) || 
                    System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.RightShift))
                {
                    if (draggedBlockStructure != null)
                    {
                        draggedBlockStructure.SnapBlockToGrid(draggedPhysicsBlock, hexSize);
                    }
                    // Snap environment blocks to grid too
                    else if (environmentBlocks.Contains(draggedPhysicsBlock))
                    {
                        // Convert pixel position to hex coordinate
                        float sqrt3 = 1.73205080757f;
                        float q = (sqrt3 / 3f * draggedPhysicsBlock.X - 1f / 3f * draggedPhysicsBlock.Y) / hexSize;
                        float r = (2f / 3f * draggedPhysicsBlock.Y) / hexSize;

                        // Round to nearest integer hex coordinate
                        int hexQ = (int)Math.Round(q);
                        int hexR = (int)Math.Round(r);

                        // Convert back to pixel position
                        float snapX = hexSize * (sqrt3 * hexQ + sqrt3 / 2f * hexR);
                        float snapY = hexSize * (3f / 2f * hexR);

                        draggedPhysicsBlock.X = snapX;
                        draggedPhysicsBlock.Y = snapY;
                        draggedPhysicsBlock.VelocityX = 0;
                        draggedPhysicsBlock.VelocityY = 0;
                        draggedPhysicsBlock.AngularVelocity = 0;
                    }
                }
                
                draggedPhysicsBlock = null;
                draggedBlockStructure = null;
                draggedChainbot = null;
                skiaCanvas.InvalidateVisual();
            }
            else if (draggedBlock != null)
            {
                draggedBlock = null;
                skiaCanvas.InvalidateVisual();
            }
        }

        private void OnCanvasMouseRightDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(skiaCanvas);
            // Convert WPF DIPs to SkiaSharp physical pixels
            float mouseX = (float)pos.X * canvasScaleX;
            float mouseY = (float)pos.Y * canvasScaleY;

            // Check if right-clicking on a demo block to delete it
            for (int i = demoBlocks.Count - 1; i >= 0; i--)
            {
                var block = demoBlocks[i];
                if (block.Contains(mouseX, mouseY))
                {
                    demoBlocks.Remove(block);
                    skiaCanvas.InvalidateVisual();
                    e.Handled = true;
                    return;
                }
            }
        }

        private void OnCanvasMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            // Rotate demo block under cursor
            var pos = e.GetPosition(skiaCanvas);
            // Convert WPF DIPs to SkiaSharp physical pixels
            float mouseX = (float)pos.X * canvasScaleX;
            float mouseY = (float)pos.Y * canvasScaleY;

            // Find block under cursor (topmost)
            for (int i = demoBlocks.Count - 1; i >= 0; i--)
            {
                var block = demoBlocks[i];
                if (block.Contains(mouseX, mouseY))
                {
                    // Rotate 15 degrees per wheel notch
                    block.Rotation += e.Delta > 0 ? 15f : -15f;
                    skiaCanvas.InvalidateVisual();
                    e.Handled = true;
                    return;
                }
            }
        }


        private void RenderFrame(object? sender, EventArgs e)
        {
            // Calculate delta time
            DateTime currentTime = DateTime.Now;
            float deltaTime = (float)(currentTime - lastFrameTime).TotalSeconds;
            lastFrameTime = currentTime;

            // Update physics if simulation is running
            if (isSimulationRunning && physicsEnabled)
            {
                foreach (var structure in chainStructures)
                {
                    structure.Update(deltaTime, constraintIterations: 10);
                }
                
                foreach (var bot in chainbots)
                {
                    bot.Update(deltaTime, constraintIterations: 10);
                }
            }

            // Invalidate the canvas to trigger a redraw
            skiaCanvas.InvalidateVisual();
        }

        private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            var info = e.Info;

            // Calculate DPI scaling factor (SkiaSharp pixels vs WPF DIPs)
            canvasScaleX = info.Width / (float)skiaCanvas.ActualWidth;
            canvasScaleY = info.Height / (float)skiaCanvas.ActualHeight;

            // Clear the canvas with white background
            canvas.Clear(SKColors.White);

            // Render toolbar first (at top, before any transforms)
            toolbar.Render(canvas, info.Width);

            // Save canvas state
            canvas.Save();

            // Translate to center of screen (accounting for toolbar)
            float toolbarHeight = toolbar.GetHeight();
            canvas.Translate(info.Width / 2f, (info.Height + toolbarHeight) / 2f);

            // Render both hex meshes
            RenderHexMesh(canvas, targetMesh, HexRenderMode.Skeleton);
            RenderHexMesh(canvas, materialMesh, HexRenderMode.Solid);

            // Restore canvas state
            canvas.Restore();

            // Render environment blocks (in screen space, after restore)
            foreach (var block in environmentBlocks)
            {
                block.Render(canvas);
            }

            // Render chain structures (in screen space, after restore)
            foreach (var structure in chainStructures)
            {
                structure.Render(canvas);
            }

            // Render chainbots
            foreach (var bot in chainbots)
            {
                bot.Render(canvas);
            }

            // Render demo blocks (in screen space, after restore)
            foreach (var demoBlock in demoBlocks)
            {
                demoBlock.Render(canvas);
            }

            // Render freedraw on top of everything
            freeDraw.Render(canvas);
        }

        private void RenderHexMesh(SKCanvas canvas, HexMesh mesh, HexRenderMode mode)
        {
            SKPaint fillPaint;
            SKPaint strokePaint;

            if (mode == HexRenderMode.Skeleton)
            {
                // Skeleton mode - transparent with dashed outline
                fillPaint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = SKColors.Transparent,
                    IsAntialias = true
                };

                strokePaint = new SKPaint
                {
                    Color = SKColor.Parse("#999999"),
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 2f,
                    IsAntialias = true,
                    PathEffect = SKPathEffect.CreateDash(new float[] { 5f, 5f }, 0f)
                };
            }
            else // Solid mode
            {
                fillPaint = new SKPaint
                {
                    Color = SKColor.Parse("#2D2D2D"),
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                };

                strokePaint = new SKPaint
                {
                    Color = SKColors.LightGray,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 2f,
                    IsAntialias = true
                };
            }

            // Render all occupied hexagons
            foreach (var hexPos in mesh.GetAllOccupiedPositions())
            {
                var (x, y) = hexPos.ToPixel(hexSize);
                DrawHexagon(canvas, x, y, hexSize, fillPaint, strokePaint);
            }

            fillPaint.Dispose();
            strokePaint.Dispose();
        }

        private void DrawHexagon(SKCanvas canvas, float centerX, float centerY, float radius, SKPaint fillPaint, SKPaint strokePaint)
        {
            using var path = new SKPath();

            // Create flat-top hexagon
            for (int i = 0; i < 6; i++)
            {
                float angle = (float)(Math.PI / 180 * (60 * i - 30));
                float x = centerX + radius * (float)Math.Cos(angle);
                float y = centerY + radius * (float)Math.Sin(angle);

                if (i == 0)
                    path.MoveTo(x, y);
                else
                    path.LineTo(x, y);
            }
            path.Close();

            canvas.DrawPath(path, fillPaint);
            canvas.DrawPath(path, strokePaint);
        }
    }
}

