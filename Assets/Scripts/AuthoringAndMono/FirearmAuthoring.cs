
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Physics;

public partial struct FirearmData : IComponentData
{
    public int RPM;
    public float3 bullet_spawn_pos;
    public Entity bullet_impact_body_entity;
    public Entity bullet_impact_metal_entity;
    public Entity muzzle_flash_entity;
    public Entity loaded_magazine;
    public Entity chamber_bullet;
    public Entity slider_entity;
    public Entity muzzle_flash_light_entity;

}

public class FirearmAuthoring : MonoBehaviour
{

    public int RPM;
    public GameObject bullet_spawn_GB;
    public GameObject bullet_impact_body_GB;
    public GameObject bullet_impact_metal_GB;
    public GameObject chamber_bullet_GB;
    public GameObject muzzle_flash_entity;
    public GameObject loaded_mag_entity;
    public GameObject slider_entity;
    public GameObject muzzle_flash_light_entity;


    public class FirearmAuthoringBaker : Baker<FirearmAuthoring>
    {


        public override void Bake(FirearmAuthoring authoring)
        {

            float3 bullet_spawn_pos = (float3)authoring.bullet_spawn_GB.transform.localPosition;

            AddComponent(new FirearmData
            {
                RPM = authoring.RPM,
                bullet_spawn_pos = bullet_spawn_pos,
                muzzle_flash_entity = GetEntity(authoring.muzzle_flash_entity),
                loaded_magazine = GetEntity(authoring.loaded_mag_entity),
                chamber_bullet = GetEntity(authoring.chamber_bullet_GB),
                bullet_impact_body_entity = GetEntity(authoring.bullet_impact_body_GB),
                bullet_impact_metal_entity = GetEntity(authoring.bullet_impact_metal_GB),
                slider_entity = GetEntity(authoring.slider_entity),
                muzzle_flash_light_entity = GetEntity(authoring.muzzle_flash_light_entity)


            });

        }


    }
}
