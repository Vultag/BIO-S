
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;



public partial struct HasToExitData : IComponentData
{
    public uint physics_layer;
    public Entity shape_entity;
    public Vector3 shape_size;

}