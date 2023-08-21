
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public partial struct DoorData : IComponentData
{
    public float starting_world_y;
    public float target_local_y;
    public float open_speed;

}
