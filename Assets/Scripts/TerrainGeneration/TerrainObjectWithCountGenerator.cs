using UnityEngine;
using System;

public abstract class TerrainObjectWithCountGenerator : TerrainObjectGenerator
{
    [Range(0, 3000)] public int count = 0;
}