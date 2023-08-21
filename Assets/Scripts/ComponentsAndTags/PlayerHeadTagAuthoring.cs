using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;



public struct PlayerHeadTag : IComponentData
{

}

public class PlayerHeadTagAuthoring : MonoBehaviour
{

}

public class PlayerHeadTagBaker : Baker<PlayerHeadTagAuthoring>
{

    public override void Bake(PlayerHeadTagAuthoring authoring)
    {

        AddComponent(new PlayerHeadTag
        {
        });
    }


}