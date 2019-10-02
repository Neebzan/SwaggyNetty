using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PositionDataPackage
{
    public uint PlayerID;
    public Vector2 Position;
}

[Serializable]
public class GridDataPackage
{
    public int X;
    public int Y;
    public Color Color;
}

[Serializable]
public class DataCollectionPackage
{
    public List<PositionDataPackage> PositionDataPackages = new List<PositionDataPackage>();
    public List<GridDataPackage> GridDataPackages = new List<GridDataPackage>();
}
