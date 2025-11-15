using System;
using Microsoft.Xna.Framework;

namespace Chainbots.Models
{
    /// <summary>
    /// Represents a hexagon position using axial coordinates (q, r).
    /// </summary>
    public struct HexCoordinate : IEquatable<HexCoordinate>
    {
        public int Q { get; }  // Column
        public int R { get; }  // Row

        public HexCoordinate(int q, int r)
        {
            Q = q;
            R = r;
        }

        /// <summary>
        /// Gets the neighbor offset for a given direction (0-5).
        /// Direction 0 (E):  (+1,  0)
        /// Direction 1 (SE): ( 0, +1)
        /// Direction 2 (SW): (-1, +1)
        /// Direction 3 (W):  (-1,  0)
        /// Direction 4 (NW): ( 0, -1)
        /// Direction 5 (NE): (+1, -1)
        /// </summary>
        public static readonly HexCoordinate[] Directions = new HexCoordinate[]
        {
            new HexCoordinate(1, 0),   // E
            new HexCoordinate(0, 1),   // SE
            new HexCoordinate(-1, 1),  // SW
            new HexCoordinate(-1, 0),  // W
            new HexCoordinate(0, -1),  // NW
            new HexCoordinate(1, -1)   // NE
        };

        /// <summary>
        /// Gets the neighbor in the specified direction (0-5).
        /// </summary>
        public HexCoordinate GetNeighbor(int direction)
        {
            var offset = Directions[direction % 6];
            return new HexCoordinate(Q + offset.Q, R + offset.R);
        }

        /// <summary>
        /// Converts axial coordinates to pixel position.
        /// </summary>
        public Vector2 ToPixel(float hexSize)
        {
            float sqrt3 = 1.73205080757f;
            float x = hexSize * (sqrt3 * Q + sqrt3 / 2 * R);
            float y = hexSize * (3f / 2f * R);
            return new Vector2(x, y);
        }

        /// <summary>
        /// Converts pixel position to hex coordinates.
        /// </summary>
        public static HexCoordinate FromPixel(Vector2 pixel, float hexSize)
        {
            float sqrt3 = 1.73205080757f;
            float q = (sqrt3 / 3f * pixel.X - 1f / 3f * pixel.Y) / hexSize;
            float r = (2f / 3f * pixel.Y) / hexSize;
            
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
}

