using Chainbots.Interfaces;
using Microsoft.Xna.Framework;

namespace Chainbots.Rendering;

public class Camera : ICamera
{
    public Vector2 Position { get; set; }
    public float Zoom { get; set; }
    public Matrix View { get; private set; }
    public Matrix Projection { get; private set; }

    private readonly float _pixelsPerMeter;
    private int _viewportWidth;
    private int _viewportHeight;

    public Camera(float pixelsPerMeter)
    {
        _pixelsPerMeter = pixelsPerMeter;
        Position = Vector2.Zero;
        Zoom = 1.0f;
        View = Matrix.Identity;
    }

    public void UpdateViewport(int width, int height)
    {
        _viewportWidth = width;
        _viewportHeight = height;
        UpdateMatrices();
    }

    public void UpdateMatrices()
    {
        float viewportWidth = _viewportWidth / _pixelsPerMeter;
        float viewportHeight = _viewportHeight / _pixelsPerMeter;

        Projection = Matrix.CreateOrthographicOffCenter(
            -viewportWidth / 2f * Zoom + Position.X,
            viewportWidth / 2f * Zoom + Position.X,
            viewportHeight / 2f * Zoom + Position.Y,
            -viewportHeight / 2f * Zoom + Position.Y,
            -1f,
            1f
        );

        View = Matrix.Identity;
    }

    public Vector2 WorldToScreen(Vector2 worldPos)
    {
        float screenX = _viewportWidth / 2f + (worldPos.X - Position.X) * _pixelsPerMeter / Zoom;
        float screenY = _viewportHeight / 2f + (worldPos.Y - Position.Y) * _pixelsPerMeter / Zoom;
        return new Vector2(screenX, screenY);
    }

    public Vector2 ScreenToWorld(Vector2 screenPos)
    {
        float worldX = (screenPos.X - _viewportWidth / 2f) * Zoom / _pixelsPerMeter + Position.X;
        float worldY = (screenPos.Y - _viewportHeight / 2f) * Zoom / _pixelsPerMeter + Position.Y;
        return new Vector2(worldX, worldY);
    }

    public void Pan(Vector2 delta)
    {
        Position += delta;
        UpdateMatrices();
    }

    public void AdjustZoom(float factor)
    {
        Zoom *= factor;
        UpdateMatrices();
    }

    public void Reset()
    {
        Position = Vector2.Zero;
        Zoom = 1.0f;
        UpdateMatrices();
    }
}

