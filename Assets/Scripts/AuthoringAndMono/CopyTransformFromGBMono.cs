using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class CopyTransformFromGBMono : MonoBehaviour
{
    public bool is_right_hand;
    public bool is_head;
    public Quaternion initial_rot;
}

public class CopyTransformFromGBBaker : Baker<CopyTransformFromGBMono>
{

    public override void Bake(CopyTransformFromGBMono authoring)
    {

        AddComponent(new CopyTransformFromGB
        {
            is_right_hand = authoring.is_right_hand,
            is_head = authoring.is_head,
            initial_rot = authoring.initial_rot
        });
    }


}
