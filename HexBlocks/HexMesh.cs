using System.Collections.Generic;
using System.Linq;
using Chainbots.Models;

namespace Chainbots.HexBlocks
{
    /// <summary>
    /// Manages the hexagonal mesh state - which positions have hexagons and which don't.
    /// State management only - no rendering logic.
    /// </summary>
    public class HexMesh
    {
        private readonly HashSet<HexCoordinate> occupiedPositions;

        public HexMesh()
        {
            occupiedPositions = new HashSet<HexCoordinate>();
        }

        /// <summary>
        /// Adds a hexagon at the specified position.
        /// </summary>
        public void AddHexagon(HexCoordinate position)
        {
            occupiedPositions.Add(position);
        }

        /// <summary>
        /// Removes a hexagon at the specified position.
        /// </summary>
        public void RemoveHexagon(HexCoordinate position)
        {
            occupiedPositions.Remove(position);
        }

        /// <summary>
        /// Checks if a hexagon exists at the specified position.
        /// </summary>
        public bool HasHexagon(HexCoordinate position)
        {
            return occupiedPositions.Contains(position);
        }

        /// <summary>
        /// Gets the state of all six surrounding hexagons for a given position.
        /// Returns an array of 6 booleans indicating presence (true) or absence (false)
        /// of hexagons in directions 0-5 (E, SE, SW, W, NW, NE).
        /// </summary>
        public bool[] GetSurroundingStates(HexCoordinate position)
        {
            var states = new bool[6];
            for (int i = 0; i < 6; i++)
            {
                var neighbor = position.GetNeighbor(i);
                states[i] = HasHexagon(neighbor);
            }
            return states;
        }

        /// <summary>
        /// Gets all neighbor positions that have hexagons.
        /// </summary>
        public List<HexCoordinate> GetOccupiedNeighbors(HexCoordinate position)
        {
            var neighbors = new List<HexCoordinate>();
            for (int i = 0; i < 6; i++)
            {
                var neighbor = position.GetNeighbor(i);
                if (HasHexagon(neighbor))
                {
                    neighbors.Add(neighbor);
                }
            }
            return neighbors;
        }

        /// <summary>
        /// Gets the count of occupied neighbor positions.
        /// </summary>
        public int GetOccupiedNeighborCount(HexCoordinate position)
        {
            int count = 0;
            for (int i = 0; i < 6; i++)
            {
                if (HasHexagon(position.GetNeighbor(i)))
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Gets all occupied positions in the mesh.
        /// </summary>
        public IEnumerable<HexCoordinate> GetAllOccupiedPositions()
        {
            return occupiedPositions.ToList();
        }

        /// <summary>
        /// Clears all hexagons from the mesh.
        /// </summary>
        public void Clear()
        {
            occupiedPositions.Clear();
        }

        /// <summary>
        /// Gets the total count of hexagons in the mesh.
        /// </summary>
        public int Count => occupiedPositions.Count;
    }
}

