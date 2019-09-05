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
public class PositionDataCollectionPackage {
    public PositionDataPackage [ ] PositionDataPackages;
}
