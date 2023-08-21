using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public struct CopyTransformFromGB : IComponentData
{

    public bool is_right_hand;
    public bool is_head;
    public Quaternion initial_rot;

}