# Chainbots 2D Simulation Plan

## Project Goal

Create a real-time 2D simulation of chainbots walking on a hexagonal lattice and placing blocks. The simulation will visualize the core locomotion mechanics and block placement using C# with WPF and SkiaSharp for hardware-accelerated rendering.

## Technology Stack

- **Language**: C# (.NET 8+)
- **UI Framework**: WPF (Windows Presentation Foundation)
- **Graphics**: SkiaSharp for 2D rendering
- **Architecture**: MVVM pattern for separation of concerns

## 2D Geometry Design

### Hexagonal Blocks

**Flat-Top Hexagon Orientation**
```
    Top Edge
   /        \
  /          \
 |            |  <- Sides
  \          /
   \        /
    Bottom Edge
```

**Key Parameters**:
- **Radius (R)**: Distance from center to vertex (e.g., 30 pixels)
- **Flat Width**: 2R (60 pixels)
- **Height**: R × √3 (≈52 pixels)
- **Edge Length**: R

**Vertex Positions** (0° starts at top-right, clockwise):
1. Vertex 0: (R, 0) - Right
2. Vertex 1: (R/2, R×√3/2) - Bottom-right
3. Vertex 2: (-R/2, R×√3/2) - Bottom-left
4. Vertex 3: (-R, 0) - Left
5. Vertex 4: (-R/2, -R×√3/2) - Top-left
6. Vertex 5: (R/2, -R×√3/2) - Top-right

**Connection Points**: 
- Each edge midpoint is a potential connection point
- 6 connection points per hex (one per face)

### Hex Grid Coordinate System

Use **Axial Coordinates** (q, r):
- q: column (horizontal shift)
- r: row (diagonal shift)

**Pixel Position Calculation**:
```
x = size × (√3 × q + √3/2 × r)
y = size × (3/2 × r)
```

**Neighbor Offsets**:
```
Direction 0 (E):  (+1,  0)
Direction 1 (SE): ( 0, +1)
Direction 2 (SW): (-1, +1)
Direction 3 (W):  (-1,  0)
Direction 4 (NW): ( 0, -1)
Direction 5 (NE): (+1, -1)
```

## Chainbot Structure

### 2-Leg Chainbot Design

The chainbot consists of:
1. **Body Core**: Central processing unit (small hexagon, 0.6× scale)
2. **Leg A**: Full-size hexagonal block
3. **Leg B**: Full-size hexagonal block
4. **Connectors**: Flexible joints connecting body to legs

**Visual Representation**:
```
     [Leg A]
       |
     {Body}
       |
     [Leg B]
```

When attached to surface:
```
Surface: [=][=][=][=][=][=]
              ↑       ↑
            Leg B   Leg A
              ↓
            {Body}
```

### Walking Mechanism

**Attachment States**:
- Each leg can be: `Attached` or `Free`
- At least one leg must always be attached for stability
- Body position is calculated from attached leg(s)

**Walking Cycle** (4 phases):

1. **Both legs attached** (stable)
   ```
   Surface: [=][X][=][X][=]
             Leg-B  Leg-A
   ```

2. **Lift Leg A** (pivoting on Leg B)
   ```
   Surface: [=][X][=][ ][=]
             Leg-B    Leg-A(free)
   ```

3. **Move Leg A to target** (swing phase)
   ```
   Surface: [=][X][=][ ][X]
             Leg-B       Leg-A
   ```

4. **Attach Leg A** (both attached again)
   ```
   Surface: [=][X][=][ ][X]
             Leg-B    Leg-A
   ```

5. **Lift Leg B** (pivoting on Leg A)
   ```
   Surface: [=][ ][=][ ][X]
            Leg-B(free)  Leg-A
   ```

6. **Move Leg B forward** (swing phase)
   ```
   Surface: [=][ ][X][ ][X]
                Leg-B  Leg-A
   ```

7. **Attach Leg B** → Return to state 1 (advanced one position)

### Body Positioning Logic

**When both legs attached**:
- Body center = midpoint between Leg A and Leg B centers
- Body orientation = perpendicular to line between legs

**When one leg attached**:
- Body orbits around the attached leg
- Distance = (leg radius + body radius + connector length)
- Body can rotate around the pivot point

## Implementation Steps

### Phase 1: Core Infrastructure (Days 1-2)

**1.1 Project Setup**
- [ ] Create WPF project (.NET 6+)
- [ ] Add SkiaSharp.Views.WPF NuGet package
- [ ] Set up MVVM folder structure (Models, ViewModels, Views)
- [ ] Configure SKElement for rendering canvas

**1.2 Geometry Foundation**
- [ ] Create `HexGeometry` class
  - Axial coordinate system (q, r)
  - Pixel position conversion
  - Neighbor calculation
  - Edge/vertex helpers
- [ ] Create `Vector2D` utility class
- [ ] Implement hex-to-pixel and pixel-to-hex conversions

**1.3 Rendering System**
- [ ] Create `IRenderable` interface
- [ ] Implement `SkiaRenderer` class
  - Camera/viewport system
  - Pan and zoom controls
  - Coordinate transformations
- [ ] Set up render loop (60 FPS target)

### Phase 2: Hexagonal Blocks (Days 3-4)

**2.1 Block Model**
- [ ] Create `HexBlock` class
  - Position (axial coordinates)
  - State (placed, held, free)
  - ID/color properties
  - Connection points
- [ ] Implement block rendering
  - Filled hexagon with border
  - Color variations
  - Grid snapping visualization

**2.2 Block Grid**
- [ ] Create `BlockGrid` class
  - Dictionary-based storage: `Dictionary<(int q, int r), HexBlock>`
  - Add/remove blocks
  - Query blocks at position
  - Check valid placement
- [ ] Implement grid rendering
  - Ghost grid (faint hexagons)
  - Placed blocks (solid)
  - Highlights for valid positions

**2.3 Block Interaction**
- [ ] Mouse hover to show grid position
- [ ] Click to place blocks manually (for testing)
- [ ] Display axial coordinates on hover

### Phase 3: Chainbot Basics (Days 5-7)

**3.1 Chainbot Model**
- [ ] Create `ChainbotLeg` class
  - Position (axial coordinates)
  - Attachment state (attached/free)
  - Target position (for movement)
  - Animation progress
- [ ] Create `Chainbot` class
  - Body core properties
  - Reference to Leg A and Leg B
  - Current state (idle, walking, placing)
  - Payload (carried block)

**3.2 Leg Attachment Logic**
- [ ] Implement `AttachLeg(leg, position)` method
  - Validate target is a valid block surface
  - Update leg state
  - Recalculate body position
- [ ] Implement `DetachLeg(leg)` method
  - Validate at least one leg remains attached
  - Update leg state
- [ ] Body position calculator
  - Two-leg case: midpoint
  - One-leg case: orbital position

**3.3 Chainbot Rendering**
- [ ] Render body (smaller hex, distinct color)
- [ ] Render legs (full hex, different color)
- [ ] Render connectors (lines/curves between body and legs)
- [ ] Render attachment indicators (circle/dot at connection point)
- [ ] Render payload (if carrying block)

### Phase 4: Walking Animation (Days 8-10)

**4.1 Movement Animation**
- [ ] Create `AnimationController` class
  - Interpolation methods (linear, ease-in-out)
  - Time-based animation state
- [ ] Implement leg swing animation
  - Start position → target position
  - Curved path (arc over surface)
  - Duration parameter (e.g., 0.5 seconds)
- [ ] Implement body movement during walking
  - Follow interpolated position between legs
  - Smooth rotation

**4.2 Walking State Machine**
- [ ] Create `WalkingState` enum:
  - `Idle`: Both legs attached, no movement
  - `LiftingLegA`: Detaching Leg A
  - `SwingingLegA`: Moving Leg A to target
  - `AttachingLegA`: Attaching Leg A
  - `LiftingLegB`: Detaching Leg B
  - `SwingingLegB`: Moving Leg B to target
  - `AttachingLegB`: Attaching Leg B
- [ ] Implement state transitions
  - Auto-progress through states
  - Trigger animations on state entry
- [ ] Create `Walk(direction)` method
  - Calculate target positions for legs
  - Initiate walking state machine

**4.3 Path Following**
- [ ] Create `Path` class
  - List of waypoints (axial coordinates)
  - Current waypoint index
- [ ] Implement path following logic
  - Determine next target from current position
  - Choose which leg to move based on distance
  - Automatic progression along path

### Phase 5: Block Placement (Days 11-12)

**5.1 Payload Management**
- [ ] Implement `PickupBlock(block)` method
  - Store block reference in chainbot
  - Mark block as "held"
  - Update rendering (block attached to body)
- [ ] Implement `ReleaseBlock(position)` method
  - Validate placement position
  - Add block to grid
  - Clear payload reference

**5.2 Placement Animation**
- [ ] Create placement state machine
  - `Positioning`: Moving body to target location
  - `Lowering`: Extending payload down
  - `Releasing`: Detaching payload
  - `Retracting`: Returning to idle
- [ ] Animate block descent from body to surface
- [ ] Show placement preview (ghost block)

**5.3 Placement Validation**
- [ ] Check if target position is adjacent to existing blocks
- [ ] Ensure chainbot can reach target (leg range check)
- [ ] Prevent placing on occupied positions

### Phase 6: User Interface & Controls (Days 13-14)

**6.1 Control Panel**
- [ ] Create WPF control panel
  - Play/Pause simulation
  - Speed slider (0.1× to 5×)
  - Step-by-step mode
  - Reset simulation
- [ ] Camera controls
  - Pan (mouse drag or arrow keys)
  - Zoom (mouse wheel or +/- keys)
  - Reset view button

**6.2 Chainbot Controls**
- [ ] Manual control mode
  - Select chainbot
  - Click target to walk
  - Keyboard shortcuts (WASD for directions)
- [ ] Spawn new blocks in payload area
- [ ] Delete blocks (right-click)

**6.3 Visualization Options**
- [ ] Toggle grid display
- [ ] Toggle coordinate labels
- [ ] Toggle chainbot debug info (state, leg positions)
- [ ] Color themes (dark/light mode)
- [ ] Show connection points
- [ ] Highlight walkable surfaces

### Phase 7: Automation & Intelligence (Days 15-16)

**7.1 Task Queue System**
- [ ] Create `Task` class
  - Task type (Walk, PickUp, PlaceBlock, Wait)
  - Parameters (position, duration)
- [ ] Create `TaskQueue` for chainbot
  - Enqueue tasks
  - Execute current task
  - Auto-advance on completion

**7.2 Path Planning**
- [ ] Implement A* pathfinding on hex grid
  - Valid positions: existing block surfaces
  - Cost function: distance to target
- [ ] Generate walk commands from path
- [ ] Handle unreachable targets (error state)

**7.3 Build Sequences**
- [ ] Create predefined build patterns
  - Line of blocks
  - Triangle formation
  - Square/hexagon perimeter
- [ ] Generate task queues from patterns
- [ ] Multi-chainbot coordination (basic)

### Phase 8: Polish & Features (Days 17-18)

**8.1 Visual Polish**
- [ ] Add shadows under chainbots
- [ ] Particle effects (dust when walking)
- [ ] Smooth lighting/shading on blocks
- [ ] Chainbot eyes/indicators (direction facing)

**8.2 Information Display**
- [ ] Status bar
  - Chainbot count
  - Block count (placed/available)
  - Current simulation time
- [ ] Chainbot info panel (when selected)
  - Current state
  - Task queue
  - Position
  - Energy (optional)

**8.3 Save/Load**
- [ ] Serialize grid state to JSON
- [ ] Serialize chainbot states
- [ ] Load simulation from file
- [ ] Export viewport as image (PNG)

### Phase 9: Testing & Optimization (Days 19-20)

**9.1 Testing**
- [ ] Test walking on various surfaces
  - Flat surfaces
  - Edges and corners
  - Isolated blocks (unreachable scenarios)
- [ ] Test block placement
  - Adjacent to existing blocks
  - Creating structures
- [ ] Test multiple chainbots (collision avoidance basic)

**9.2 Performance Optimization**
- [ ] Profile rendering performance
- [ ] Implement spatial culling (only render visible blocks)
- [ ] Optimize collision checks
- [ ] Batch rendering for many blocks

**9.3 Documentation**
- [ ] Code documentation (XML comments)
- [ ] User guide in README
- [ ] Architecture diagram
- [ ] Demo scenarios/screenshots

## Key Classes Overview

### Core Geometry
```csharp
public class HexCoordinate
{
    public int Q { get; set; }  // Column
    public int R { get; set; }  // Row
    
    public Vector2D ToPixel(float size);
    public static HexCoordinate FromPixel(Vector2D pixel, float size);
    public HexCoordinate GetNeighbor(int direction);
    public float DistanceTo(HexCoordinate other);
}

public class Vector2D
{
    public float X { get; set; }
    public float Y { get; set; }
    
    public float Length();
    public Vector2D Normalize();
    public static Vector2D Lerp(Vector2D a, Vector2D b, float t);
}
```

### Block System
```csharp
public class HexBlock
{
    public HexCoordinate Position { get; set; }
    public Guid Id { get; set; }
    public SKColor Color { get; set; }
    public BlockState State { get; set; }
    
    public void Render(SKCanvas canvas, float hexSize);
}

public class BlockGrid
{
    private Dictionary<(int q, int r), HexBlock> blocks;
    
    public void PlaceBlock(HexBlock block);
    public void RemoveBlock(HexCoordinate position);
    public HexBlock GetBlock(HexCoordinate position);
    public bool IsOccupied(HexCoordinate position);
    public List<HexCoordinate> GetWalkableSurfaces();
}
```

### Chainbot System
```csharp
public class ChainbotLeg
{
    public HexCoordinate Position { get; set; }
    public HexCoordinate TargetPosition { get; set; }
    public bool IsAttached { get; set; }
    public float AnimationProgress { get; set; }
    
    public Vector2D GetCurrentPixelPosition(float hexSize);
}

public class Chainbot
{
    public Guid Id { get; set; }
    public ChainbotLeg LegA { get; set; }
    public ChainbotLeg LegB { get; set; }
    public Vector2D BodyPosition { get; set; }
    public float BodyRotation { get; set; }
    public HexBlock Payload { get; set; }
    public WalkingState CurrentState { get; set; }
    
    public void Update(float deltaTime);
    public void Walk(HexCoordinate targetDirection);
    public void PickupBlock(HexBlock block);
    public void PlaceBlock(HexCoordinate position);
    public void Render(SKCanvas canvas, float hexSize);
}

public enum WalkingState
{
    Idle,
    LiftingLegA,
    SwingingLegA,
    AttachingLegA,
    LiftingLegB,
    SwingingLegB,
    AttachingLegB
}
```

### Simulation Manager
```csharp
public class SimulationManager
{
    public BlockGrid Grid { get; set; }
    public List<Chainbot> Chainbots { get; set; }
    public float SimulationSpeed { get; set; }
    public bool IsPaused { get; set; }
    
    public void Update(float deltaTime);
    public void Render(SKCanvas canvas);
    public void AddChainbot(Chainbot bot);
    public void Reset();
}
```

## Success Criteria

The simulation is complete when:
1. ✓ Hexagonal blocks render correctly with proper geometry
2. ✓ Grid coordinate system works (axial coordinates)
3. ✓ Chainbot with 2 hex legs renders and animates
4. ✓ Walking mechanism works (alternating leg attachment)
5. ✓ Chainbot can navigate across a surface of connected blocks
6. ✓ Chainbot can pick up and place blocks
7. ✓ Smooth animations for all movements
8. ✓ Real-time rendering at 60 FPS with multiple bots
9. ✓ User can control simulation (play/pause/speed)
10. ✓ User can manually command chainbots or use automated tasks

## Future Enhancements

- Multiple chainbots with collision avoidance
- Energy/power system simulation
- Block locking animations
- 3D visualization (migrate to Unity or add 3D view)
- Network coordination (swarm behavior)
- Real-time path replanning
- Physics simulation (stability, loads)
- VR/AR preview mode

