using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public partial struct ZombiePartData : IComponentData
{
    public Entity zombie_brain_entity;

}

public class ZombiePartDataAuthoring : MonoBehaviour
{

    public GameObject zombie_brain_entity;

    public class ZombiePartDataBaker : Baker<ZombiePartDataAuthoring>
    {

        public override void Bake(ZombiePartDataAuthoring authoring)
        {

            AddComponent(new ZombiePartData { 
                zombie_brain_entity = GetEntity(authoring.zombie_brain_entity)
            });

        }


    }
}
