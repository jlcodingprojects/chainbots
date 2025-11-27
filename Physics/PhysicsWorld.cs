using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Shared;
using Chainbots.HexBlocks;
using Chainbots.Interfaces;

namespace Chainbots.Physics;

public class PhysicsWorld : IPhysicsWorld
{
    public World World { get; private set; }
    public Body? GroundBody { get; private set; }

    private const float FixedTimeStep = 1f / 60f;
    private float _accumulator = 0f;

    public PhysicsWorld()
    {
        // zero gravity for testing
        //World = new World(new Vector2(0f, 0f)); 
        World = new World(new Vector2(0f, 9.8f));
    }

    public void CreateGround()
    {
        // Create a static ground body at the bottom
        float groundY = 7.88f; // 8 meters down from center
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

