using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public partial struct UITag : IComponentData
{


}

public class UITagAuthoring : MonoBehaviour
{

}

public class UITagBaker : Baker<UITagAuthoring>
{

    public override void Bake(UITagAuthoring authoring)
    {

        AddComponent(new UITag
        {
        });
    }


}


