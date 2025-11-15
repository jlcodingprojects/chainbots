using Microsoft.Xna.Framework;
using Genbox.VelcroPhysics.Dynamics;
using System.Collections.Generic;
using Chainbots.HexBlocks;

namespace Chainbots.Physics
{
    /// <summary>
    /// Interface for physics world management.
    /// </summary>
    public interface IPhysicsWorld
    {
        World World { get; }
        Body? GroundBody { get; }
        void CreateGround();
        void SetupCollisionGroups(List<HexBlock> targetBlocks, List<HexBlock> materialBlocks, List<HexBlock> anchorBlocks);
        void Update(float deltaTime);
        void Clear();
    }
}

