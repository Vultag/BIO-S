
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class GrabbableTagMono : MonoBehaviour
{

}

public class GrabbableTagBaker : Baker<GrabbableTagMono>
{

    public override void Bake(GrabbableTagMono authoring)
    {

        AddComponent(new GrabbableTag
        {
        });
    }


}