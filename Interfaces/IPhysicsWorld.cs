using Microsoft.Xna.Framework;
using Genbox.VelcroPhysics.Dynamics;
using System.Collections.Generic;
using Chainbots.HexBlocks;

namespace Chainbots.Interfaces
{
    /// <summary>
    /// Interface for physics world management.
    /// </summary>
    public interface IPhysicsWorld
    {
        World World { get; }
        Body? GroundBody { get; }
        void CreateGround();
        void Update(float deltaTime);
        void Clear();
    }
}

