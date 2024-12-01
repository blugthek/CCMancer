using Godot;
using System;
using System.Collections.Generic;

public partial class MonsterSpawningManager : Node2D
{
    [Export] private PackedScene[]? _monsters;
    public void Initialize()
    {
        GD.Print("MonsterSpawningManager initialized");
    }

    public void SpawnMonster(int index)
    {
        GD.Print("Spawning monster", _monsters.GetLength(0), index);
    }
}