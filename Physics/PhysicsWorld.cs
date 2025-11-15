using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Shared;
using Chainbots.HexBlocks;

namespace Chainbots.Physics;

/// <summary>
/// Manages the VelcroPhysics world and physics bodies.
/// </summary>
public class PhysicsWorld : IPhysicsWorld
{
    public World World { get; private set; }
    public Body? GroundBody { get; private set; }

    private const float FixedTimeStep = 1f / 60f;
    private float _accumulator = 0f;

    public PhysicsWorld()
    {
        // Initialize physics world with gravity
        World = new World(new Vector2(0f, 9.8f)); // 9.8 m/s^2 gravity
    }

    public void CreateGround()
    {
        // Create a static ground body at the bottom
        float groundY = 8f; // 8 meters down from center
        GroundBody = BodyFactory.CreateBody(World, new Vector2(0f, groundY), 0f, BodyType.Static);

        // Create a wide rectangle for the ground (100m wide, 1m tall)
        Vertices groundVerts = new Vertices(4);
        groundVerts.Add(new Vector2(-50f, -0.5f));
        groundVerts.Add(new Vector2(50f, -0.5f));
        groundVerts.Add(new Vector2(50f, 0.5f));
        groundVerts.Add(new Vector2(-50f, 0.5f));

        var groundShape = new PolygonShape(groundVerts, 1f);
        var groundFixture = GroundBody.CreateFixture(groundShape);
        groundFixture.Friction = 0.8f;
        groundFixture.Restitution = 0.2f;
    }

    public void SetupCollisionGroups(
        List<HexBlock> targetBlocks,
        List<HexBlock> materialBlocks)
    {
        // Set up collision categories and masks
        // Target blocks: Category 0x0001 (only collide with nothing - visual only)
        // Material blocks: Category 0x0002 (collide with each other and ground)
        // Ground: Category 0x0008 (collide with material blocks)

        foreach (var block in targetBlocks)
        {
            block.SetCollisionCategory(0x0001, 0x0000);
        }

        foreach (var block in materialBlocks)
        {
            block.SetCollisionCategory(0x0002, 0x0002 | 0x0008); // Collide with material and ground
        }

        // Set ground collision category
        if (GroundBody != null)
        {
            foreach (var fixture in GroundBody.FixtureList)
            {
                fixture.CollisionCategories = (Genbox.VelcroPhysics.Collision.Filtering.Category)0x0008;
                fixture.CollidesWith = (Genbox.VelcroPhysics.Collision.Filtering.Category)0x0002; // Only collide with material blocks
            }
        }
    }

    public void Update(float deltaTime)
    {
        // Fixed timestep physics update
        _accumulator += deltaTime;
        while (_accumulator >= FixedTimeStep)
        {
            World.Step(FixedTimeStep);
            _accumulator -= FixedTimeStep;
        }
    }

    public void Clear()
    {
        World.Clear();
        GroundBody = null;
        _accumulator = 0f;
    }
}

