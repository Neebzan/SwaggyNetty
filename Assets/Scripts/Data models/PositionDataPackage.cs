using System;
using UnityEngine;

[Serializable]
public class PositionDataPackage
{
    public uint PlayerID { get; set; }
    public Vector2 Position { get; set; }
}
