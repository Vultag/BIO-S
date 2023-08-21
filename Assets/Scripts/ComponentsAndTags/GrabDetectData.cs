using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct GrabDetectData : IComponentData
{

    public bool grabbing;
    public int snap_type; // 0 = null ; 1 = USP ...
    public Entity hand_grab_joint;
    public Entity grabbed_item;

}
