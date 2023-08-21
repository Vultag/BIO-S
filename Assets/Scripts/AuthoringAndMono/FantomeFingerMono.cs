
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class FantomeFingerMono : MonoBehaviour
{
    //  0 = right hand / 1 = left hand
    public int hand_ID;
    public GameObject hand_root;
    public Vector3 open_hand_rot;
    public Vector3 close_hand_rot;
    public Vector3 hold_USP_0_hand_rot; // gachette leve
    public Vector3 hold_USP_1_hand_rot; // gachette appuye
}

public class FantomeFingerBaker : Baker<FantomeFingerMono>
{

    public override void Bake(FantomeFingerMono authoring)
    {

        AddComponent(new FantomeFingerData
        {
            hand_ID = authoring.hand_ID,
            hand_root = GetEntity(authoring.hand_root),
            open_hand_rot = authoring.open_hand_rot,
            close_hand_rot = authoring.close_hand_rot,
            hold_USP_0_hand_rot = authoring.hold_USP_0_hand_rot,
            hold_USP_1_hand_rot = authoring.hold_USP_1_hand_rot
});
    }


}