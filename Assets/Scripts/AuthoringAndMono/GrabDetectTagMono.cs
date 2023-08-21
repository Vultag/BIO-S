
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class GrabDetectTagMono : MonoBehaviour
{

}

public class GrabDetectTagBaker : Baker<GrabDetectTagMono>
{

    public override void Bake(GrabDetectTagMono authoring)
    {

        AddComponent(new GrabDetectTag
        {
        });
    }


}