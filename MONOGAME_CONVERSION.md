# Chainbots MonoGame Conversion Summary

## Overview

This document summarizes the complete rewrite of the Chainbots application from WPF/SkiaSharp to MonoGame with VelcroPhysics.

## Key Changes

### Technology Stack Migration

**From:**
- .NET 8.0 Windows (WPF)
- SkiaSharp for rendering
- Custom physics implementation

**To:**
- .NET 8.0 (Cross-platform)
- MonoGame 3.8 (DesktopGL)
- VelcroPhysics for rigid body physics simulation

### Package Dependencies

```xml
<PackageReference Include="MonoGame.Framework.DesktopGL" Version="3.8.*" />
<PackageReference Include="MonoGame.Content.Builder.Task" Version="3.8.*" />
<PackageReference Include="Genbox.VelcroPhysics.MonoGame" Version="0.1.0-alpha.2" />
<PackageReference Include="Genbox.VelcroPhysics.MonoGame.DebugView" Version="0.1.0-alpha.2" />
```

## Architecture

### Core Classes

#### `Game1.cs` - Main Game Loop
- Manages the MonoGame game loop (Initialize, LoadContent, Update, Draw)
- Sets up VelcroPhysics world with gravity (9.8 m/s²)
- Manages three types of hex block collections:
  - **Target blocks**: Skeleton view showing where to build (visual only)
  - **Material blocks**: Physical blocks with gravity and dynamics
  - **Anchor blocks**: Static blocks locked to ground near robot emitter
- Implements camera controls (pan, zoom, reset)
- Uses VelcroPhysics DebugView for visualization

#### `HexBlock.cs` - Physics-Enabled Hex Blocks
- Represents a hexagonal block as a VelcroPhysics rigid body
- Three types: Target, Material, Anchor
- Features:
  - Dynamic or static physics bodies
  - Polygon shape (6-sided regular hexagon)
  - Sprite-based rendering
  - Collision categories for physics groups
  - Anchor joints to connect neighboring blocks

#### `HexCoordinate.cs` - Coordinate System
- Axial coordinate system (q, r) for hex grid
- Conversion between hex coordinates and world positions
- Neighbor calculations (6 directions: E, SE, SW, W, NW, NE)

#### `HexMesh.cs` - Grid State Management (Preserved)
- Tracks which hexagonal positions are occupied
- Queries for neighbors and connectivity

#### `Program.cs` - Entry Point
- Standard MonoGame entry point

## Physics Implementation

### Gravity and Dynamics
- World gravity: `Vector2(0f, 9.8f)` m/s²
- Material blocks are dynamic bodies (respond to gravity)
- Anchor blocks are static (locked to ground)
- Target blocks are static (visual only, no collisions)

### Collision Groups
Three separate physics groups using collision categories:

1. **Target Space** (Category 0x0001)
   - Collides with: Nothing
   - Purpose: Visual reference only

2. **Material Space** (Category 0x0002)
   - Collides with: Material (0x0002) and Anchor (0x0004)
   - Purpose: Physical blocks that can move

3. **Anchor Space** (Category 0x0004)
   - Collides with: Material (0x0002) and Anchor (0x0004)
   - Purpose: Fixed attachment points near robot emitter

### Joint Connections
- Material blocks are connected to neighbors via **WeldJoints**
- WeldJoints provide rigid connections between blocks
- Anchor blocks connect to nearby material blocks
- Connections automatically created for neighboring hexagons

## Grid Layout

### Target Grid (Skeleton)
- Larger structure showing build goal
- Range: q, r ∈ [-4, 4] where |q| + |r| ≤ 4
- Total blocks: 61 hexagons
- Rendered with transparency

### Material Grid (Active Physics)
- Smaller starting structure
- Range: q, r ∈ [-2, 2] where |q| + |r| ≤ 2
- Total blocks: 19 hexagons
- Full physics simulation

### Anchor Blocks
- 3 blocks positioned at coordinates (-8, -1), (-8, 0), (-8, 1)
- Static (cannot move)
- Connect to nearby material blocks within range

## Rendering

### Sprite System
- Hexagonal blocks rendered as textured quads
- Texture created programmatically (white hexagon shape)
- Color tinting for different block types:
  - Target: `Color(150, 150, 150, 80)` - Semi-transparent gray
  - Anchor: `Color(100, 100, 100, 255)` - Dark gray
  - Material: `Color(45, 45, 45, 255)` - Nearly black

### Camera System
- Orthographic projection
- Configurable zoom and pan
- Coordinate conversion between screen space and physics world
- Centered on origin with adjustable scale

### Debug View
- VelcroPhysics DebugView shows:
  - Physics body outlines
  - Joint connections
  - Collision shapes
- Useful for debugging physics behavior

## Controls

| Input | Action |
|-------|--------|
| Arrow Keys | Pan camera |
| +/- Keys | Zoom in/out |
| R Key | Reset camera |
| Escape | Exit application |

## Removed Components

The following WPF-specific components were removed:

- `App.xaml` / `App.xaml.cs` - WPF application
- `MainWindow.xaml` / `MainWindow.xaml.cs` - WPF window
- `Toolbar.cs` - SkiaSharp toolbar
- `FreeDraw.cs` - Freehand drawing (Ctrl+Drag)
- `DemoHexBlock.cs` - Old hex block without physics
- `PhysicsBlock.cs` - Custom physics implementation
- `Chainbot.cs`, `ChainStructure.cs`, `Leg.cs`, `Joint.cs` - Chainbot implementation (to be reimplemented later)

## Future Work

### Chainbot Implementation (Not Yet Included)
Per user request, chainbot functionality will be reimplemented separately with:
- VelcroPhysics bodies for each segment
- Joint-based locomotion
- Attachment system using physics constraints
- State machine for walking behavior

### Enhancements
- Interactive block placement
- Mouse picking and dragging
- Custom shaders for better hexagon rendering
- Particle effects
- Sound effects
- More sophisticated joint configurations
- Robot emitter visualization

## Technical Notes

### Fixed Timestep Physics
- Physics updates at fixed 60 Hz
- Accumulator pattern prevents physics jitter
- Rendering decoupled from physics updates

### Coordinate Systems
- Physics world: Meters (0.5m hex radius)
- Screen space: Pixels (60 pixels per meter)
- Hex coordinates: Axial (q, r) integers

### Memory Management
- VelcroPhysics bodies and joints are managed by the World
- Texture created once at LoadContent
- No per-frame allocations in critical path

## Build & Run

```bash
dotnet restore
dotnet build
dotnet run
```

## Compatibility

- **Platform**: Cross-platform (Windows, Linux, macOS via MonoGame DesktopGL)
- **Runtime**: .NET 8.0
- **Graphics**: OpenGL via MonoGame

## References

- [VelcroPhysics Documentation](https://github.com/Genbox/VelcroPhysics)
- [MonoGame Documentation](https://docs.monogame.net/)
- [Hexagonal Grids Guide](https://www.redblobgames.com/grids/hexagons/)

