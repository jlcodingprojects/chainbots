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

This repository includes a 2D simulation of the chainbot locomotion and block placement mechanics.

### Project Setup

**Technology Stack:**
- .NET 8.0 (Windows)
- WPF (Windows Presentation Foundation)
- SkiaSharp 2.88.8 for hardware-accelerated 2D rendering

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

The project scaffold includes:
- ✅ WPF project with .NET 8.0
- ✅ SkiaSharp.Views.WPF integrated
- ✅ 60 FPS render loop
- ✅ Chainbots.xo logo displayed
- ⬜ Hexagonal geometry system (planned)
- ⬜ Walking simulation (planned)
- ⬜ Block placement (planned)

See `SIMULATION_PLAN.md` for the complete implementation roadmap.