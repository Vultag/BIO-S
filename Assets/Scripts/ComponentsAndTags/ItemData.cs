
using System.Numerics;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;
using Vector3 = UnityEngine.Vector3;

public partial struct ItemData : IComponentData
{

    public bool grabbed;
    public int hand_grabbing;
    public int item_type_id;
    public Vector3 grab_pos_offset;//pas utilise ?
    public Vector3 grab_rot_offset;//pas utilise ?
    //public Entity grab_joint; mis sur les mains
    public int snap_type; // 0 = null ; 1 = USP ...

    //pour item composite comme USP
    public bool is_composite;
    public Entity composite_sub_part;

}

