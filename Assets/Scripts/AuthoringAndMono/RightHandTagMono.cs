

using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class RightHandTagMono : MonoBehaviour
{

}

public class RightHandTagBaker : Baker<RightHandTagMono>
{

    public override void Bake(RightHandTagMono authoring)
    {

        AddComponent(new RightHandTag
        {
        });
    }


}