
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Physics.Authoring;
using UnityEngine;

public class GrabDetectMono : MonoBehaviour
{

    public bool grabbing;

}

public class GrabDetectBaker : Baker<GrabDetectMono>
{

    public override void Bake(GrabDetectMono authoring)
    {

        AddComponent(new GrabDetectData
        {
            grabbing = authoring.grabbing
    });
    }


}