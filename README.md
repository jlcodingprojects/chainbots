# Chainbots

Autonomous robots that build physical structures using hexagonal blocks.

## Key Concept

**Blocks**: Hexagonal tiles that lock together to form structures
- Simple locking mechanism for assembly
- Very simple and cheap to manufactor 

**Chainbots**: Mobile robots that navigate and build on the hex lattice
- Walk across the surface using chain-like locomotion
- Carry and place blocks
- Powered by the structure itself (conductive contacts or rails)
- Simple finite-state machine logic

## Key Design Considerations

### Blocks
- Regular hexagon geometry with interlocking faces
- Locking options: snap-fit, rotating latch, magnetic, or spring-loaded pins
- Must support structural loads (compression, bending)
- ID encoding (visual markers, RFID, or etched patterns)

### Chainbots
- Chain-based walking mechanism on hex lattice
- Payload capacity: 1+ blocks
- Power: Surface contacts + small capacitor buffer
- Durable against dust, misalignment, and impacts

### Power System
- Power delivered through surface (conductive vertices or rails)
- Low voltage (12-48V) for safety
- Capacitor buffer for high-power actions

### Navigation & Logic
- Block IDs for position tracking
- FSM-based control
- Follow predetermined routes with fallback logic
- Deliver payload, return to base for next task

## 2D Simulation

This repository includes a 2D simulation of the chainbot locomotion and block placement mechanics using MonoGame and VelcroPhysics.

### Project Setup

**Technology Stack:**
- .NET 8.0
- MonoGame 3.8 (DesktopGL)
- VelcroPhysics for 2D rigid body physics simulation

**Build and Run:**
```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the simulation
dotnet run
```

### Current Implementation

The project includes:
- ✅ MonoGame project with .NET 8.0
- ✅ VelcroPhysics integration for realistic physics
- ✅ Hexagonal geometry system with axial coordinates
- ✅ Target space (skeleton view) showing build goals
- ✅ Material space (solid blocks) with gravity and physics
- ✅ Anchor blocks locked to ground near emitter
- ✅ Hex blocks connected via anchor joints
- ✅ Separate physics groups for target/material spaces
- ✅ VelcroPhysics debug view for visualization
- ⬜ Chainbot implementation (planned)

### Controls

- **Arrow Keys**: Move camera
- **+/-**: Zoom in/out
- **R**: Reset camera
- **Escape**: Exit

See `SIMULATION_PLAN.md` for the complete implementation roadmap.