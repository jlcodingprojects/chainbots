using Chainbots.Models;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.Shared;
using Microsoft.Xna.Framework;
using System;

namespace Chainbots.ChainBots;

public class ChainBotCell
{
    private static int _nextId = 1;

    public int Id { get; }
    public HexCoordinate Coordinate { get; }
    public Body Body { get; private set; }
    public float Size { get; }
    public Vector2 WorldPosition => Body.Position;

    public static void ResetIds()
    {
        _nextId = 1;
    }

    public ChainBotCell(World world, HexCoordinate coordinate, float size)
    {
        Id = _nextId++;
        Coordinate = coordinate;
        Size = size;

        Vector2 position = coordinate.ToPixel(size);

        // Create physics body for Material blocks
        Body = BodyFactory.CreateBody(world, position, 0f, BodyType.Dynamic);

        Body.Mass = 1.0f;
        Body.LinearDamping = 0.5f;
        Body.AngularDamping = 0.5f;

        var vertices = CreateTriangleVerticies(size);

        // Create polygon shape
        var shape = new PolygonShape(vertices, 1f);
        var fixture = Body.CreateFixture(shape);
        fixture.Friction = 0.5f;
        fixture.Restitution = 0.3f;
    }

    private Vertices CreateTriangleVerticies(float radius)
    {
        var vertices = new Vertices(6);

        // Flat-top triangle: vertices at 0°, 120°, 240°
        for (int i = 0; i < 3; i++)
        {
            float angle = MathHelper.ToRadians(120 * i);
            float x = radius * (float)Math.Cos(angle);
            float y = radius * (float)Math.Sin(angle);
            vertices.Add(new Vector2(x, y));
        }

        return vertices;
    }

    public void SetPrecisePosition(Vector2 position, float rotation = 0f)
    {
        Body.Position = position;
        Body.Rotation = rotation;
    }

    public static HexBlock CreateAtPosition(World world, Vector2 position, float size, bool isStatic = false, float rotation = 0f)
    {
        // Create a temporary coordinate (will be overridden by precise position)
        var tempCoord = HexCoordinate.FromPixel(position, size);
        var block = new HexBlock(world, tempCoord, size, isStatic);
        block.SetPrecisePosition(position, rotation);
        return block;
    }
}

