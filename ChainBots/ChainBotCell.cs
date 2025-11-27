using Chainbots.Models;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Dynamics.Joints;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.Shared;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Chainbots.ChainBots;

public enum MotorState
{
    Forward,
    Backward,
    Off,
}


public struct CellState
{
    public MotorState V0;
    public MotorState V1;
    public MotorState V2;
}


public class ChainBotCell
{
    private static int _nextId = 0;
    public int Id { get; }
    public Body Body { get; private set; }
    public float Size { get; }

    public CellState State { get; private set; }

    //private 

    public static void ResetIds()
    {
        _nextId = 0;
    }

    public ChainBotCell(World world, Vector2 position, float size)
    {
        Id = _nextId++;
        Size = size;

        Body = BodyFactory.CreateBody(world, position, 0f, BodyType.Dynamic);

        Body.Mass = 1.0f;
        Body.LinearDamping = 0.5f;
        Body.AngularDamping = 0.5f;

        var vertices = CreateTriangleVerticies(size);

        // Create polygon shape
        var shape = new PolygonShape(vertices, 1f);
        var fixture = Body.CreateFixture(shape);
        fixture.Friction = 0.1f;
        fixture.Restitution = 0.00f;

    }

    private Vertices CreateTriangleVerticies(float radius)
    {
        var vertices = new Vertices(3);

        float[] angles = { -90f, 150f, 30f };

        for (int i = 0; i < 3; i++)
        {
            float rad = MathHelper.ToRadians(angles[i]);
            vertices.Add(new Vector2(
                radius * (float)Math.Cos(rad),
                radius * (float)Math.Sin(rad)
            ));
        }

        return vertices;
    }

    public void SetPrecisePosition(Vector2 position, float rotation = 0f)
    {
        Body.Position = position;
        Body.Rotation = rotation;
    }

    public void SetState(CellState newState)
    {
        State = newState;
    }
}

