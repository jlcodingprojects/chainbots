using Chainbots.ChainBots;
using Chainbots.HexBlocks;
using Chainbots.Input;
using Chainbots.Interfaces;
using Chainbots.Physics;
using Chainbots.Rendering;
using Chainbots.UI;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Chainbots.Core;

public static class Program
{
    public const float HexSize = 0.5f;
    private const float PixelsPerMeter = 60f;

    [STAThread]
    static void Main()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();

        using (var game = serviceProvider.GetRequiredService<Game1>())
        {
            game.Run();
        }
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IPhysicsWorld, PhysicsWorld>();
        services.AddSingleton<ICamera>(sp => new Camera(PixelsPerMeter));

        services.AddSingleton<ChainBot>(sp =>
        {
            var physicsWorld = sp.GetRequiredService<IPhysicsWorld>();
            return new ChainBot(physicsWorld, HexSize);
        });

        services.AddSingleton<IHexGridManager>(sp =>
        {
            var physicsWorld = sp.GetRequiredService<IPhysicsWorld>();
            return new HexGridManager(physicsWorld, HexSize);
        });

        services.AddSingleton<IInputHandler, InputHandler>();

        // Register the game
        services.AddSingleton<Game1>();
        services.AddSingleton<TextureStore>();
        services.AddSingleton<IHexRenderer, HexRenderer>();
        services.AddSingleton<IToolbar, Toolbar>();
    }
}

