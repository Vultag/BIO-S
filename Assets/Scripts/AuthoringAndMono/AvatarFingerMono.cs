using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class AvatarFingerMono : MonoBehaviour
{
    public GameObject fantome_correspondant;
    public GameObject upper_finger_bone;
    public GameObject Skin_finger_entity;
    public GameObject hand_root;
}

public class AvatarFingerBaker : Baker<AvatarFingerMono>
{

    public override void Bake(AvatarFingerMono authoring)
    {


        AddComponent(new AvatarFingerData
        {
            Skin_finger_entity = GetEntity(authoring.Skin_finger_entity),
            upper_finger_bone = GetEntity(authoring.upper_finger_bone),
            fantome_correspondant = GetEntity(authoring.fantome_correspondant),
            hand_root = GetEntity(authoring.hand_root)
        });
    }


}