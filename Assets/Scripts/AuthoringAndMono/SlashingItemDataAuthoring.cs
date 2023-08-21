
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public partial struct SlashingItemData : IComponentData
{
    public Vector3 slash_center;
    public Vector3 slash_size;
    public Entity splashing_vfx;
    public Entity blood_particle;

}

public class SlashingItemDataAuthoring : MonoBehaviour
{

    public Vector3 slash_center;
    public Vector3 slash_size;
    public GameObject splashing_vfx;
    public GameObject blood_particle;



    public class SlashingItemDataBaker : Baker<SlashingItemDataAuthoring>
    {


        public override void Bake(SlashingItemDataAuthoring authoring)
        {

            AddComponent(new SlashingItemData { 
                slash_center = authoring.slash_center,
                slash_size = authoring.slash_size,
                splashing_vfx = GetEntity(authoring.splashing_vfx),
                blood_particle = GetEntity(authoring.blood_particle)

            });

        }


    }
}
