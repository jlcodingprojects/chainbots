using Chainbots.ChainBots;
using Chainbots.Core;
using Genbox.VelcroPhysics.Dynamics;
using Microsoft.Xna.Framework; // Assuming MonoGame/FNA framework
using System;
using System.Collections.Generic;

namespace Chainbots.Physics;

public class MagnetForceApplier
{
    // Tuning parameters
    private readonly float _baseForce;
    private readonly float _maxRange;
    private readonly float _minDistance;

    // Geometry Cache (Local Space)
    private readonly Vector2[] _localVertices;
    private readonly Vector2[] _localEdgeCenters;

    // Helper struct to hold calculation data
    private struct MagnetPoint
    {
        public Vector2 WorldPosition;
        public Polarity Polarity;
        public PointType PointType;
    }

    public MagnetForceApplier(
        float baseForce = 0.5f,
        float maxRange = 3f,
        float minDistance = 0.2f)
    {
        _baseForce = baseForce;
        _maxRange = maxRange;
        _minDistance = minDistance;

        float radius = Program.HexSize * 0.5f;

        // 1. Pre-calculate geometry in Local Space to save CPU
        _localVertices = new Vector2[3];
        _localEdgeCenters = new Vector2[3];

        float[] angles = { -90f, 150f, 30f };

        // Calculate Vertex positions
        for (int i = 0; i < 3; i++)
        {
            float rad = MathHelper.ToRadians(angles[i]);
            _localVertices[i] = new Vector2(
                radius * (float)Math.Cos(rad),
                radius * (float)Math.Sin(rad)
            );
        }

        // Calculate Edge Center positions (midpoint between vertices)
        // E0 connects V0-V1, E1 connects V1-V2, E2 connects V2-V0
        _localEdgeCenters[0] = (_localVertices[0] + _localVertices[1]) / 2f;
        _localEdgeCenters[1] = (_localVertices[1] + _localVertices[2]) / 2f;
        _localEdgeCenters[2] = (_localVertices[2] + _localVertices[0]) / 2f;
    }

    public void ApplyForces(List<ChainBotCell> cells)
    {
        int count = cells.Count;

        // Pairwise forces
        for (int i = 0; i < count; i++)
        {
            for (int j = i + 1; j < count; j++)
            {
                ApplyPointToPointForces(cells[i], cells[j]);
            }
        }
    }

    private void ApplyPointToPointForces(ChainBotCell cellA, ChainBotCell cellB)
    {
        // 1. Get all active magnetic points in World Space for both cells
        var pointsA = GetActivePoints(cellA);
        var pointsB = GetActivePoints(cellB);

        // 2. Iterate through every combination (up to 6x6 = 36 interactions)
        foreach (var pA in pointsA)
        {
            foreach (var pB in pointsB)
            {
                ApplyForceBetweenPoints(cellA.Body, pA, cellB.Body, pB);
            }
        }
    }

    private void ApplyForceBetweenPoints(Body bodyA, MagnetPoint pA, Body bodyB, MagnetPoint pB)
    {
        // Vector from A's point to B's point
        Vector2 dir = pB.WorldPosition - pA.WorldPosition;
        float dist = dir.Length();

        // Edges have slightly weaker influence
        if (pA.PointType == PointType.Edge)
        {
            dist *= 2f;
        }
        if (pB.PointType == PointType.Edge)
        {
            dist *= 2f;
        }

        if (dist > _maxRange) return;

        // Prevent singularity/explosion at very close range
        if (dist < _minDistance) dist = _minDistance;

        // Normalize direction
        dir /= dist;

        // Determine Polarity Factor
        float polarityFactor = GetPolarityFactor(pA.Polarity, pB.Polarity);

        // If 0, they don't interact (should be caught by GetActivePoints, but safety check)
        if (polarityFactor == 0f) return;

        // Inverse square falloff: F = k * (q1*q2) / r^2
        float strength = _baseForce * (polarityFactor / (dist * dist));

        Vector2 forceVector = dir * strength;

        // Apply forces at the specific world points (creates Torque)
        // Newton's 3rd Law: A gets pulled towards B, B gets pulled towards A
        // Note: Check if your physics engine ApplyForce takes (Force, WorldPosition)

        // If polarityFactor is positive (Attraction):
        // Force on A should be towards B (positive dir).
        // Force on B should be towards A (negative dir).

        bodyA.ApplyForce(forceVector, pA.WorldPosition);
        bodyB.ApplyForce(-forceVector, pB.WorldPosition);
    }

    // Helper to gather only the enabled points transformed to World Space
    private List<MagnetPoint> GetActivePoints(ChainBotCell cell)
    {
        var points = new List<MagnetPoint>(6); // Max capacity 6
        var state = cell.State;

        // Transform helpers
        Vector2 pos = cell.Body.Position;
        float rot = cell.Body.Rotation;
        float cos = (float)Math.Cos(rot);
        float sin = (float)Math.Sin(rot);

        // --- Vertices ---
        AddIfActive(points, state.V0, _localVertices[0], pos, cos, sin, PointType.Vertex);
        AddIfActive(points, state.V1, _localVertices[1], pos, cos, sin, PointType.Vertex);
        AddIfActive(points, state.V2, _localVertices[2], pos, cos, sin, PointType.Vertex);

        // --- Edges ---
        AddIfActive(points, state.E0, _localEdgeCenters[0], pos, cos, sin, PointType.Edge);
        AddIfActive(points, state.E1, _localEdgeCenters[1], pos, cos, sin, PointType.Edge);
        AddIfActive(points, state.E2, _localEdgeCenters[2], pos, cos, sin, PointType.Edge);

        return points;
    }

    private void AddIfActive(List<MagnetPoint> list, Polarity p, Vector2 localPos, Vector2 bodyPos, float cos, float sin, PointType pointType)
    {
        if (p == Polarity.Off) return;

        // Rotate local vector + translate to world
        float worldX = (localPos.X * cos - localPos.Y * sin) + bodyPos.X;
        float worldY = (localPos.X * sin + localPos.Y * cos) + bodyPos.Y;

        list.Add(new MagnetPoint
        {
            WorldPosition = new Vector2(worldX, worldY),
            Polarity = p,
            PointType = pointType,
        });
    }

    private float GetPolarityFactor(Polarity a, Polarity b)
    {
        if (a == b) return -1f;
        return 1f;
    }
}