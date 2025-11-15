using System;
using Microsoft.Extensions.DependencyInjection;
using Chainbots.Physics;
using Chainbots.HexBlocks;
using Chainbots.Rendering;
using Chainbots.Input;

namespace Chainbots.Core
{
    /// <summary>
    /// The main entry point for the Chainbots application.
    /// </summary>
    public static class Program
    {
        private const float HexSize = 0.5f;
        private const float PixelsPerMeter = 60f;

        [STAThread]
        static void Main()
        {
            // Set up dependency injection
            var services = new ServiceCollection();
            ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            // Get the game instance from DI container and run it
            using (var game = serviceProvider.GetRequiredService<Game1>())
            {
                game.Run();
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Register core services as singletons
            services.AddSingleton<IPhysicsWorld, PhysicsWorld>();
            services.AddSingleton<ICamera>(sp => new Camera(PixelsPerMeter));
            services.AddSingleton<IHexGridManager>(sp =>
            {
                var physicsWorld = sp.GetRequiredService<IPhysicsWorld>();
                return new HexGridManager(physicsWorld, HexSize);
            });
            services.AddSingleton<IInputHandler, InputHandler>();
            
            // Register the game
            services.AddSingleton<Game1>();
        }
    }
}

