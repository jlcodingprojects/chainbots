using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.Dynamics.Joints;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Collision.Filtering;
using Chainbots.Models;

namespace Chainbots.HexBlocks;

public class HexBlock
{
    private static int _nextId = 1;
    
    public int Id { get; }
    public HexCoordinate Coordinate { get; }
    public Body? Body { get; private set; }
    public float Size { get; }
    public bool IsAnchoredToGround { get; set; }
    public Vector2 WorldPosition => Body?.Position ?? visualPosition;
    
    // For visual-only blocks (like Target), store position without physics
    private Vector2 visualPosition;
    private float visualRotation;

    public static void ResetIds()
    {
        _nextId = 1;
    }

    public HexBlock(World world, HexCoordinate coordinate, float size, bool isStatic = false)
    {
        Id = _nextId++;
        Coordinate = coordinate;
        Size = size;
        IsAnchoredToGround = false;
        
        Vector2 position = coordinate.ToPixel(size);
                
        // Create physics body for Material blocks
        Body = BodyFactory.CreateBody(world, position, 0f, isStatic ? BodyType.Static : BodyType.Dynamic);
        
        if (!isStatic)
        {
            Body.Mass = 1.0f;
            Body.LinearDamping = 0.5f;
            Body.AngularDamping = 0.5f;
        }
        
        // Create hexagon fixture (approximated as regular polygon)
        var vertices = CreateHexagonVertices(size);
        
        // Create polygon shape
        var shape = new PolygonShape(vertices, 1f);
        var fixture = Body.CreateFixture(shape);
        fixture.Friction = 0.5f;
        fixture.Restitution = 0.3f;
    }

    /// <summary>
    /// Creates the vertices for a hexagon shape.
    /// </summary>
    private Vertices CreateHexagonVertices(float radius)
    {
        var vertices = new Vertices(6);
        
        // Flat-top hexagon: vertices at 0°, 60°, 120°, 180°, 240°, 300°
        for (int i = 0; i < 6; i++)
        {
            float angle = MathHelper.ToRadians(60 * i);
            float x = radius * (float)Math.Cos(angle);
            float y = radius * (float)Math.Sin(angle);
            vertices.Add(new Vector2(x, y));
        }
        
        return vertices;
    }

    public static WeldJoint? CreateGroundAnchorJoint(World world, HexBlock block, Body groundBody)
    {
        if (block.Body == null || groundBody == null || world == null)
            return null;
        
        block.IsAnchoredToGround = true;
        
        // Convert the block's world position to local coordinates for each body
        Vector2 anchorWorld = block.Body.Position;
        Vector2 localAnchorOnGround = groundBody.GetLocalPoint(anchorWorld);
        Vector2 localAnchorOnBlock = Vector2.Zero; // Center of the block in local space
        
        // Create a weld joint to rigidly connect the block to ground
        var joint = JointFactory.CreateWeldJoint(
            world,
            groundBody,
            block.Body, 
            localAnchorOnGround,  // Local anchor point on ground body
            localAnchorOnBlock    // Local anchor point on block (its center)
        );
        
        joint.CollideConnected = false;
        
        return joint;
    }

    /// <summary>
    /// Sets the position of a hex block to a precise X/Y coordinate.
    /// This ensures blocks are positioned such that their edges perfectly align with neighboring blocks.
    /// </summary>
    /// <param name="position">The world position to place the block at</param>
    /// <param name="rotation">Optional rotation in radians (default: 0)</param>
    public void SetPrecisePosition(Vector2 position, float rotation = 0f)
    {
        if (Body != null)
        {
            Body.Position = position;
            Body.Rotation = rotation;
        }
        else
        {
            // For visual-only blocks (like Target)
            visualPosition = position;
            visualRotation = rotation;
        }
    }

    /// <summary>
    /// Helper method to create a block at a precise X/Y position with edges perfectly aligned.
    /// </summary>
    public static HexBlock CreateAtPosition(World world, Vector2 position, float size, bool isStatic = false, float rotation = 0f)
    {
        // Create a temporary coordinate (will be overridden by precise position)
        var tempCoord = HexCoordinate.FromPixel(position, size);
        var block = new HexBlock(world, tempCoord, size, isStatic);
        block.SetPrecisePosition(position, rotation);
        return block;
    }

    /// <summary>
    /// Gets the world position of a specific face (edge) of the hexagon.
    /// Face indices: 0-5, where 0 is the right edge (0°) and indices increase counter-clockwise.
    /// </summary>
    /// <param name="faceIndex">The face index (0-5)</param>
    /// <returns>The world position at the center of the specified face</returns>
    public Vector2 GetFacePosition(int faceIndex)
    {
        Vector2 blockPos = Body?.Position ?? visualPosition;
        float blockRot = Body?.Rotation ?? visualRotation;
        
        // Face is at the midpoint between two vertices
        // For a flat-top hexagon, faces are at 30°, 90°, 150°, 210°, 270°, 330°
        float faceAngle = MathHelper.ToRadians(30 + 60 * faceIndex) + blockRot;
        float faceDistance = Size * 0.866025404f; // sqrt(3)/2 * size = distance from center to face
        
        return new Vector2(
            blockPos.X + faceDistance * (float)Math.Cos(faceAngle),
            blockPos.Y + faceDistance * (float)Math.Sin(faceAngle)
        );
    }

    /// <summary>
    /// Finds the two closest faces between this block and another block.
    /// </summary>
    /// <param name="other">The other hex block</param>
    /// <returns>A tuple containing (thisFaceIndex, otherFaceIndex)</returns>
    //public (int thisFace, int otherFace) FindClosestFaces(HexBlock other)
    //{
    //    float minDistance = float.MaxValue;
    //    int closestThisFace = 0;
    //    int closestOtherFace = 0;

    //    // Check all combinations of faces
    //    for (int i = 0; i < 6; i++)
    //    {
    //        Vector2 thisFacePos = GetFacePosition(i);
            
    //        for (int j = 0; j < 6; j++)
    //        {
    //            Vector2 otherFacePos = other.GetFacePosition(j);
    //            float distance = Vector2.Distance(thisFacePos, otherFacePos);
                
    //            if (distance < minDistance)
    //            {
    //                minDistance = distance;
    //                closestThisFace = i;
    //                closestOtherFace = j;
    //            }
    //        }
    //    }

    //    return (closestThisFace, closestOtherFace);
    //}
}

