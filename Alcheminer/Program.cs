using System.Reflection;
using AlchemyEngine.Core.Components;
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

        enemy = Runtime.EntityManager.Create();
        
        Console.WriteLine($"new enemy created {enemy}");

        Chunk<Transform> chunk = new Chunk<Transform>();
        chunk.AddEntity(player, new Transform{ Position = new(1,2,3) } );
        chunk.AddEntity(enemy, new Transform{ Position = new(4,5,6) } );

        for (var i = 0; i < chunk.Count; i++)
        {
            Console.WriteLine($"Entity {chunk.GetEntity(i)} is in position {chunk.GetComponent(i).Position}");
        }
    }
}