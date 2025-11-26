using System;
using Microsoft.Xna.Framework;

namespace Chainbots.Models;

public struct HexCoordinate : IEquatable<HexCoordinate>
{
    public int Q { get; }  // Column
    public int R { get; }  // Row

    public HexCoordinate(int q, int r)
    {
        Q = q;
        R = r;
    }

    public static readonly HexCoordinate[] Directions = new HexCoordinate[]
    {
        new HexCoordinate(1, 0),   // E
        new HexCoordinate(0, 1),   // SE
        new HexCoordinate(-1, 1),  // SW
        new HexCoordinate(-1, 0),  // W
        new HexCoordinate(0, -1),  // NW
        new HexCoordinate(1, -1)   // NE
    };

    public HexCoordinate GetNeighbor(int direction)
    {
        var offset = Directions[direction % 6];
        return new HexCoordinate(Q + offset.Q, R + offset.R);
    }

    public Vector2 ToPixel(float hexSize)
    {
        float sqrt3 = 1.7320508075688772f;
        // Flat-top hexagon coordinate conversion
        float x = hexSize * (3f / 2f * Q);
        float y = hexSize * (sqrt3 / 2f * Q + sqrt3 * R);
        return new Vector2(x, y);
    }

    public static HexCoordinate FromPixel(Vector2 pixel, float hexSize)
    {
        float sqrt3 = 1.7320508075688772f;
        float q = (2f / 3f * pixel.X) / hexSize;
        float r = (-1f / 3f * pixel.X + sqrt3 / 3f * pixel.Y) / hexSize;
        
        // Round to nearest hex coordinate
        return RoundToHex(q, r);
    }

    private static HexCoordinate RoundToHex(float q, float r)
    {
        float s = -q - r;
        
        int rq = (int)Math.Round(q);
        int rr = (int)Math.Round(r);
        int rs = (int)Math.Round(s);
        
        float qDiff = Math.Abs(rq - q);
        float rDiff = Math.Abs(rr - r);
        float sDiff = Math.Abs(rs - s);
        
        if (qDiff > rDiff && qDiff > sDiff)
            rq = -rr - rs;
        else if (rDiff > sDiff)
            rr = -rq - rs;
        
        return new HexCoordinate(rq, rr);
    }

    public bool Equals(HexCoordinate other)
    {
        return Q == other.Q && R == other.R;
    }

    public override bool Equals(object? obj)
    {
        return obj is HexCoordinate other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Q, R);
    }

    public static bool operator ==(HexCoordinate left, HexCoordinate right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(HexCoordinate left, HexCoordinate right)
    {
        return !left.Equals(right);
    }
}

