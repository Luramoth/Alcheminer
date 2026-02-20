using System.Reflection;
using AlchemyEngine.Core.Ecs;

namespace Alcheminer;

using AlchemyEngine;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        AlchemyEngine.Runtime.Init();

        Entity player = Runtime.EntityManager.Create();
        Entity enemy = Runtime.EntityManager.Create();
        
        Console.WriteLine($"player: {player} enemy: {enemy}");
        
        Runtime.EntityManager.Destroy(enemy);
        Console.WriteLine($"is enemy valid? {Runtime.EntityManager.IsAlive(enemy)}");
    }
}