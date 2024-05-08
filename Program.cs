using System.Diagnostics;
using System.Reflection;
using Silk.NET.SDL;
using TheAdventure.Models;

namespace TheAdventure;

public static class Program
{
    public static void LoadPlugins(Sdl sdl, GameRenderer renderer, Dictionary<int, GameObject> gameObjects)
    {
        LoadQuestalia("C:/Users/Stefan/RiderProjects/TheAdventure/Questalia/bin/Debug/net8.0/Questalia.dll", sdl, renderer, gameObjects);
    }

    public static void LoadQuestalia(string path, Sdl sdl, GameRenderer renderer, Dictionary<int, GameObject> gameObjects)
    {
        var assembly = Assembly.LoadFrom(path);
        Type? plugin = null;
        
        foreach (var type in assembly.GetTypes())
        {
            Console.WriteLine(type);
            if (type.FullName == "Questalia.Questalia")
            {
                plugin = type;
                break;
            }

            if (plugin == null)
            {
                Console.WriteLine("Could not load Questalia plugin! Aborting...");
                return;
            }
        }

        var constructor = plugin.GetConstructor(Type.EmptyTypes);

        if (constructor == null)
        {
            Console.WriteLine("Constructor missing! Aborting...");
            return;
        }
        var pluginInstance = constructor.Invoke(null);
        Console.WriteLine(pluginInstance);
        
        var init = plugin.GetMethod("Init");
        init.Invoke(pluginInstance, new object[] {sdl, renderer, gameObjects});
        
        var addQuest = plugin.GetMethod("AddQuest");
        addQuest.Invoke(pluginInstance, new object[] {"Start walking!", 0, 0});
        
        Console.WriteLine("Questalia loaded!");
        Console.WriteLine(constructor);
    }
    public static void Main()
    {
        var sdl = new Sdl(new SdlContext());
        
        var sdlInitResult = sdl.Init(Sdl.InitVideo | Sdl.InitEvents | Sdl.InitTimer | Sdl.InitGamecontroller |
                                     Sdl.InitJoystick);
        if (sdlInitResult < 0)
        {
            throw new InvalidOperationException("Failed to initialize SDL.");
        }

        using (var window = new GameWindow(sdl, 800, 480))
        {
            var renderer = new GameRenderer(sdl, window);
            var input = new Input(sdl, window, renderer);
            var engine = new Engine(renderer, input);
            Dictionary<int, GameObject> gameObjects = engine.InitializeWorld();
            
            LoadPlugins(sdl, renderer, gameObjects);

            bool quit = false;
            while (!quit)
            {
                quit = input.ProcessInput();
                if (quit) break;
                
                engine.ProcessFrame();
                engine.RenderFrame();
            }
        }
        
        sdl.Quit();
    }
}