

using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class LeftHandTagMono : MonoBehaviour
{

}

public class LeftHandTagBaker : Baker<LeftHandTagMono>
{

    public override void Bake(LeftHandTagMono authoring)
    {

        AddComponent(new LeftHandTag
        {
        });
    }


}