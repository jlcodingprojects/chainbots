using System;
using Microsoft.Xna.Framework;

namespace Chainbots.Models;


readonly struct FlatOrientation
{
    public FlatOrientation()
    {
        this.f0 = 3.0 / 2.0;
        this.f1 = 0.0;
        this.f2 = Math.Sqrt(3.0) / 2.0;
        this.f3 = Math.Sqrt(3.0);
        this.b0 = 2.0 / 3.0;
        this.b1 = 0.0;
        this.b2 = -1.0 / 3.0;
        this.b3 = Math.Sqrt(3.0) / 3.0;
        this.start_angle = 0.0;
    }

    public readonly double f0;
    public readonly double f1;
    public readonly double f2;
    public readonly double f3;
    public readonly double b0;
    public readonly double b1;
    public readonly double b2;
    public readonly double b3;
    public readonly double start_angle;
}

public struct HexCoordinate : IEquatable<HexCoordinate>
{
    public int Q { get; }  // Column
    public int R { get; }  // Row

    //private FlatOrientation _orientation = new FlatOrientation();

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

    private float originX = 50;
    private float originY = 50;
    private float sizeX = 5;
    private float sizeY = 5;

    //public Vector2 ToPixel()
    //{
    //    //var M = _orientation;
    //    double x = (M.f0 * Q + M.f1 * R) * sizeX;
    //    double y = (M.f2 * Q + M.f3 * R) * sizeY;
    //    return new Vector2((float)x + originX, (float)y + originY);
    //}

    //public FractionalHex PixelToHexFractional(Point p)
    //{
    //    Orientation M = orientation;
    //    Point pt = new Point((p.x - origin.x) / size.x, (p.y - origin.y) / size.y);
    //    double q = M.b0 * pt.x + M.b1 * pt.y;
    //    double r = M.b2 * pt.x + M.b3 * pt.y;
    //    return new FractionalHex(q, r, -q - r);
    //}

    //public Hex PixelToHexRounded(Point p)
    //{
    //    return PixelToHexFractional(p).HexRound();
    //}

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

