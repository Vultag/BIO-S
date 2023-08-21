
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

// PAS UTILISE ???

public struct SlideTag : IComponentData
{

}

public class SlideTagAuthoring : MonoBehaviour
{

}

public class SlideTagBaker : Baker<SlideTagAuthoring>
{

    public override void Bake(SlideTagAuthoring authoring)
    {

        AddComponent(new SlideTag
        {
        });
    }


}
