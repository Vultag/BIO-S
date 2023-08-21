
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ItemMono : MonoBehaviour
{
    public bool grabbed;
    // 0 no hands // 1 right hand // 2 left hand // 3 both hands
    public int hand_grabbing; //pas utilise ?
    // 0 katana // 1 USP // 3 Mag // 
    public int item_type_id;
    public Vector3 grab_pos_offset;
    public Vector3 grab_rot_offset;
    public bool is_composite;
    public int snap_type; // 0 = null ; 1 = USP ...
    public GameObject composite_sub_part;
}

public class ItemBaker : Baker<ItemMono>
{

    public override void Bake(ItemMono authoring)
    {

        AddComponent(new ItemData
        {
            grabbed = authoring.grabbed,
            grab_pos_offset = authoring.grab_pos_offset,
            grab_rot_offset = authoring.grab_rot_offset,
            hand_grabbing = 0,
            is_composite = authoring.is_composite,
            snap_type = authoring.snap_type,
            composite_sub_part = GetEntity(authoring.composite_sub_part),
            item_type_id = authoring.item_type_id
        });
    }


}