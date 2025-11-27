using Chainbots.Models;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.Shared;
using Microsoft.Xna.Framework;
using System;

namespace Chainbots.ChainBots;

public enum Polarity
{
    Positive,
    Negative,
    Off,
}

public enum PointType
{
    Edge,
    Vertex,
}

public struct CellState
{
    public Polarity V0;
    public Polarity V1;
    public Polarity V2;

    public Polarity E0;
    public Polarity E1;
    public Polarity E2;
}

public class ChainBotCell
{
    private static int _nextId = 0;
    public int Id { get; }
    public HexCoordinate Coordinate { get; }
    public Body Body { get; private set; }
    public float Size { get; }

    public CellState State { get; private set; }

    //private 

    public static void ResetIds()
    {
        _nextId = 0;
    }

    public ChainBotCell(World world, HexCoordinate coordinate, float size)
    {
        Id = _nextId++;
        Coordinate = coordinate;
        Size = size;

        Vector2 position = coordinate.ToPixel(size);

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

