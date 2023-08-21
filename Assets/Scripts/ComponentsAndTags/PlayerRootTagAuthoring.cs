using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;



public struct PlayerRootTag : IComponentData
{

}

public class PlayerRootTagAuthoring : MonoBehaviour
{

}

public class PlayerRootTagBaker : Baker<PlayerRootTagAuthoring>
{

    public override void Bake(PlayerRootTagAuthoring authoring)
    {

        AddComponent(new PlayerRootTag
        {
        });
    }


}
