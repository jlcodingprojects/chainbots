namespace Chainbots.Models
{
    /// <summary>
    /// Type of hex block for different behaviors and rendering.
    /// </summary>
    public enum HexBlockType
    {
        Target,   // Visual only - shows where to build
        Material, // Physical blocks that can move and be manipulated
        Anchor    // Fixed to ground, connects to material blocks
    }
}
