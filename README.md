# Chainbots

Autonomous robots that build physical structures using hexagonal blocks.

The fundamental idea is inspired by nature, specifically plants. The thought struck me that trees grow into complex structures, which we cut down, furnish into planks of wood, then reassemble as houses.

Why are we unable to build structures which grow themselves instead of our expensive wasteful process? We can build such fantastically complex machines such as spaceships and data centres but not simple self growing structures?

This led me down a path of exploring biology, emergence, organic chemistry, and current research. 

## What is necessary, what is sufficient, and what do we have?

Living beings arent the only example of self growing structures. Two that spring to mind are crystals and cities. You could probably also throw in social trends and galaxies. These are all examples where an initial state + energy = a more varied and complex state.

The question on my mind was what is the principle behind it all and how can we build systems which take advantage of it? Specifically with the goal of building self-growing physical structures - ideally with a practical application in construction.

What all of these have in common is the following:
- Constituent parts which engage with each other according to certain criteria
- Some means of transport to move these parts around.
- A source of energy which drives the prior two points

For example, trees:
- Constituent parts are cells - all derive from stem cells but after differentiation engage with each other in complex ways
- Transport is its vascular system
- Source of energy is *metabolism*. Yes the ultimate source is photosynthesis but that's less the concern here. We want to understand how the constituent parts engage with each other.

The other examples have the same traits, albeit in a more abstract way for most of them.

## Why we cant make synthetic trees

A synchronic perspective answers this quite simple. Trees are complex state machines which rely on immensely complex chemical interactions. 
Even if we could build miniture machines that function like stem cells, the development of the tree itself depends on seemingly random interactions between components. The task of programming the interactions between the constituent parts (eg programming when a stem cell should differentiate, and programming the behaviours of the diverged cells) is very very complex.
It relies heavily on the concept of emergence, that a set of individual parts each following a discrete set of behaviours can result in beautiful complex super-structures.

The individual cells act like state machines, and somehow the complex interactions between these machines results in the growth of the tree. Perhaps the most fascinating part of this is that the cells are acting in their own interest and for their own survival.

### How we *could* make a synthetic tree

- Understand organic chemistry well enough to build a vascular system (complex interactions with fluids, soft bodies)
- Understand emergence well enough to build cells which can simultaneously travel through this transport system, find where they need to go, then do the right thing when they get there
- Build complex cells which can under take many different goals in a complex system - including reversible actions

So yeah about a thousand years away, give or take a couple orders of magnitude.

# The reasoning flaw

It's tempting to give up here, and I expect many do hence why there seems to be so little effective research on the topic, let alone practical outcomes. 

I believe the mistake is taking a synchronic perspective instead of a diachronic perspective. Instead of looking at biology today, consider why and how it works that way. 
Why is the vascular system so complex? Why do livings things rely on such complex chemical interactions? Why are the emergent state machines so complex? Why is metabolism so complex?

The key understanding is that it is not this way because the *end result* inherently requires it to be this way. It's like this as a side effect of the mechanisms by which is came to be.

- The transport system does not have to be vascular, it does not require fluid dynamics and soft bodies.
- The constituent parts to not need to be exceedingly complex state machines with the ability to morph into other machines. They simply have to lock into place at the right time.
- The source of energy does not need to be self contained in the blocks. We do not need to create a metabolism

# What we DO need

- The transport system needs to be dynamic. It needs to be able to grow with the structure
- Constituent parts DO need to be able to lock into place under specific target conditions
- Source of energy DOES need to be available at the point of each block

So lets come up with a new architecture which does all this but brings this a little closer to reality.


## Key Concept

**Constituent Parts**: Tesselating blocks that lock together to form structures
- Simple locking mechanism for assembly
- Very simple and cheap to manufactor 
- Hexagons in 2D, probably Rhombic Dodecahedrons in 3D
- Conductive (alternative edges can carry positive and negative)

**Chainbots**: Mobile robots that navigate and build on the hex lattice
- Walk across the surface using chain-like locomotion
- Carry and place blocks
- Powered by the structure itself (conductive contacts or rails)
- Simple finite-state machine logic

It supplies all the requirements as follows

- Transport: Surface is predicatble due to highly constrained degrees of freedom. Chainbots can walk along the surface (or climb or hang)
- Metabolism: Surface itself is conductive so bots to not require a metabolism. 
- Constituent parts: The logic which decides how the parts interacts is dedicated by a dedicated controller (eg computer). Individual parts do not need to carry complex logic around with them.


## Possible chainbot structure MSPaint diagram

 ![Chainbot example](/brainstorm/walkers.png)
 
In this case the chainbots are simple triangles which connect at their corners. In a the real world they would have an internal mechanism for rotating around their verticies. This could be a motor or perhaps magnetic.

The surface could be grippy to allow the bots to cling to the underside by providing inner pressure. This is an example where only certain hex blocks might have some grippy surfaces (the undersides).
 
 A collection of chainbots will group together and carry the target to its destination.
 - Six to Eight bots could carry a hex block long simple surfaces
 - Twelve bots configured together could carry it on complex surfaces such as upside down or crossing gaps

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

 
## Next steps

1. Update the simulation to support triangular interlocking chainbots. Implement the FSM.
2. Brainstorm mechanics for 


### Project Setup

**Technology Stack:**
- .NET 8.0
- MonoGame 3.8 (DesktopGL)
- VelcroPhysics for 2D rigid body physics simulation