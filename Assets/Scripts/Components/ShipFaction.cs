﻿using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct ShipFaction : ISharedComponentData {
    public int FactionId;
}
