using Microsoft.Xna.Framework;

namespace Chainbots.Rendering
{
    /// <summary>
    /// Interface for camera management.
    /// </summary>
    public interface ICamera
    {
        Vector2 Position { get; set; }
        float Zoom { get; set; }
        Matrix View { get; }
        Matrix Projection { get; }
        void UpdateViewport(int width, int height);
        void UpdateMatrices();
        Vector2 WorldToScreen(Vector2 worldPos);
        void Pan(Vector2 delta);
        void AdjustZoom(float factor);
        void Reset();
    }
}

