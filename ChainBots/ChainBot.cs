using Chainbots.Interfaces;
using Chainbots.Models;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Chainbots.ChainBots;

// need to further expand on this coordinate system...
public struct ChainBotCellConfiguration
{
    public HexCoordinate Coordinate { get; }

    public ChainBotCellConfiguration(int q, int r)
    {
        Coordinate = new HexCoordinate(q, r);
    }
}

public struct CellLink
{
    public int CellAId;
    public int CellBId;
    public int CellAVertexIndex;
    public int CellBVertexIndex;

    public RevoluteJoint? Joint;
}



public class ChainBot
{
    public List<ChainBotCell> Cells { get; }

    private readonly IPhysicsWorld _physicsWorld;
    private readonly float _hexSize;
    private Body? _groundBody;
    private Vector2 _worldOffset = Vector2.Zero;

    private readonly Dictionary<int, ChainBotCell> _blocksById = new();
    private readonly Dictionary<HexCoordinate, ChainBotCell> _blocksByCoordinate = new();
    private readonly Dictionary<(int first, int second), WeldJoint> _blockLinkJoints = new();

    public List<CellLink> CellLinks { get; } = new List<CellLink>();


    private readonly ChainBotCellConfiguration initialCoordinate = new ChainBotCellConfiguration(14, 1);

    public ChainBot(IPhysicsWorld physicsWorld, float hexSize)
    {
        _physicsWorld = physicsWorld;
        _hexSize = hexSize;

        Cells = new List<ChainBotCell>();
    }

    public void Initialize()
    {
        Clear();
        CreateMaterialBlocksFromConfiguration();
        //CreateLinksFromConfiguration();
    }

    //public void AddBlock(int q, int r)
    //{
    //    ChainBotCellConfiguration config = new ChainBotCellConfiguration(q, r);
    //    var materialBlock = new ChainBotCell(
    //            _physicsWorld.World,
    //            config.Coordinate,
    //            _hexSize * 0.5f
    //        );

    //    materialBlock.SetPrecisePosition(GetWorldPosition(config.Coordinate));

    //    Cells.Add(materialBlock);
    //    _blocksById[materialBlock.Id] = materialBlock;
    //    _blocksByCoordinate[config.Coordinate] = materialBlock;
    //}

    public void RemoveBlockById(int id)
    {
        if (_blocksById.TryGetValue(id, out var block))
        {
            Cells.Remove(block);
            _blocksById.Remove(id);
        }
    }

    public void Clear()
    {
        Cells.Clear();
        Cells.Clear();
        _blocksById.Clear();
        _blocksByCoordinate.Clear();
        _blockLinkJoints.Clear();
        _groundBody = null;
        _worldOffset = Vector2.Zero;
        ChainBotCell.ResetIds();
    }

    private void CreateMaterialBlocksFromConfiguration()
    {
        Vector2 initialPosition = initialCoordinate.Coordinate.ToPixel(_hexSize * 0.5f);
        Vector2 position = initialPosition;

        for (int i = 0; i < 12; i++)
        {
            var materialBlock = new ChainBotCell(
                _physicsWorld.World,
                position,
                _hexSize * 0.5f
            );

            materialBlock.SetPrecisePosition(position + _worldOffset);

            Cells.Add(materialBlock);
            _blocksById[materialBlock.Id] = materialBlock;

            position.X += _hexSize;
        }

        for (int i = 0; i < 11; i++)
        {
            RevoluteJoint joint = new RevoluteJoint(
            _blocksById[i].Body,
            _blocksById[i + 1].Body,
            new Vector2(_hexSize * 0.5f, _hexSize * 0.3f),
            new Vector2(_hexSize * -0.5f, _hexSize * 0.3f));

            joint.MotorEnabled = true;
            joint.MaxMotorTorque = 10f;

            if (i < 6)
            {
                joint.MotorSpeed = -0.5f;

            }
            else
            {
                joint.MotorSpeed = -0.5f;

            }

            _physicsWorld.World.AddJoint(joint);

            CellLinks.Add(new CellLink
            {
                CellAId = Cells[i].Id,
                CellBId = Cells[i + 1].Id,
                CellAVertexIndex = 2,
                CellBVertexIndex = 3,
                Joint = joint,
            });
        }

    }

    //private void CreateLinksFromConfiguration()
    //{
    //    if (_groundBody == null) return;

    //    foreach (var config in InitialBlockLayout)
    //    {
    //        var block = GetBlockByCoordinate(config.Coordinate);
    //        if (block == null) continue;
    //    }
    //}

    public ChainBotCell? GetBlockById(int id)
    {
        return _blocksById.TryGetValue(id, out var block) ? block : null;
    }

    private ChainBotCell? GetBlockByCoordinate(HexCoordinate coordinate)
    {
        return _blocksByCoordinate.TryGetValue(coordinate, out var block) ? block : null;
    }

    private Vector2 GetWorldPosition(HexCoordinate coordinate)
    {
        return coordinate.ToPixel(_hexSize) + _worldOffset;
    }
}

