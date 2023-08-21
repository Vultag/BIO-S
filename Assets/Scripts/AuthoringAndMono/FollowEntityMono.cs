
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;



public class FollowEntityMono : MonoBehaviour
{

    public GameObject Entity_to_follow;

}

public class FollowEntityBaker : Baker<FollowEntityMono>
{

    public override void Bake(FollowEntityMono authoring)
    {

        AddComponent(new FollowEntityData
        {
            entity_to_follow = GetEntity (authoring.Entity_to_follow)
        });
    }


}
