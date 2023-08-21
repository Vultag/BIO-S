
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class AvatarBodyTrackingMono : MonoBehaviour
{
    public GameObject Tracked_Entity;
    // 0 = head; 1 = right hands; 2 = left hand ?
    public int entity_type;
}

public class AvatarBodyTrackingBaker : Baker<AvatarBodyTrackingMono>
{

    public override void Bake(AvatarBodyTrackingMono authoring)
    {

        AddComponent(new AvatarBodyTrackingData
        {
            Tracked_Entity = GetEntity(authoring.Tracked_Entity),
            entity_type = authoring.entity_type 
        });
    }


}