# Chainbots

Autonomous robots that build physical structures using hexagonal blocks.

## Key Concept

**Blocks**: Tesselating blocks that lock together to form structures
- Simple locking mechanism for assembly
- Very simple and cheap to manufactor 
- Hexagons in 2D, probably Rhombic Dodecahedrons in 3D

**Chainbots**: Mobile robots that navigate and build on the hex lattice
- Walk across the surface using chain-like locomotion
- Carry and place blocks
- Powered by the structure itself (conductive contacts or rails)
- Simple finite-state machine logic

## Key Design Considerations

### Blocks
- Simple geometric blocks wich locking faces
- Locking options: snap-fit, rotating latch, spring-loaded pins, screws
- Must support structural loads (compression, bending)
- Might need ID encoding (visual markers, RFID, or etched patterns)

There is a strong argument to be made for multiple block types as different parts of the structure will have different load bearing requirements.
This does not necessarily mean different block shapes, possible just materials and connectors. EG some might be carbon fibers, others steel, others wood even.

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
Home base will plan out the path that a given bot must take to deliver its payload. Bot will take a preprogrammed route to deliver the block then return to home base.
Bot will use a finite state machine 

## 2D Simulation

This repository includes a 2D simulation of the chainbot locomotion and block placement mechanics using MonoGame and VelcroPhysics.

## Possible chainbot structure MSPaint diagram

 ![Chainbot example](/brainstorm/walkers.png)
 
In this case the chainbots are simple triangles which connect at their corners. In a the real world they would have an internal mechanism for rotating around their verticies. This could be a motor or perhaps magnetic.

The surface could be grippy to allow the bots to cling to the underside by providing inner pressure. This is an example where only certain hex blocks might have some grippy surfaces (the undersides).
 
 A collection of chainbots will group together and carry the target to its destination.
 - Six to Eight bots could carry a hex block long simple surfaces
 - Twelve bots configured together could carry it on complex surfaces such as upside down or crossing gaps
 

### Project Setup

**Technology Stack:**
- .NET 8.0
- MonoGame 3.8 (DesktopGL)
- VelcroPhysics for 2D rigid body physics simulation