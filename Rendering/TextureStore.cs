using Chainbots.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Chainbots.Rendering;

public class TextureStore
{
    public Texture2D Hexagon { get; private set; }
    public Texture2D Triangle { get; private set; }
    public Texture2D Square { get; private set; }

    private Color[] HexagonData { get; }
    private Color[] SquareData { get; }

    public TextureStore()
    {
        HexagonData = TextureFactory.CreateHexagonTexture(128);
        SquareData = TextureFactory.CreateSquareTexture(128);
    }


    public void Init(IGraphicsDeviceService service)
    {
        Hexagon = new Texture2D(service.GraphicsDevice, 256, 256);
        Hexagon.SetData(HexagonData);

        Square = new Texture2D(service.GraphicsDevice, 128, 128);
        Square.SetData(SquareData);
    }
}

