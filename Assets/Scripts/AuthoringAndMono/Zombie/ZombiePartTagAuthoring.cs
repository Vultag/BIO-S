using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public partial struct ZombiePartTag : IComponentData
{


}

public class ZombiePartTagAuthoring : MonoBehaviour
{

    public class ZombiePartTagBaker : Baker<ZombiePartTagAuthoring>
    {

        public override void Bake(ZombiePartTagAuthoring authoring)
        {

            AddComponent(new ZombiePartTag{ });

        }


    }
}
