
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public partial struct HandTriggerEventData : IComponentData
{
    public Entity target_entity;
    public float action_speed;
    public Entity on_visual_entity;
    public Entity off_visual_entity;

}

public class HandTriggerEventAuthoring : MonoBehaviour
{

    public GameObject target_GB;
    public float action_speed;
    public GameObject on_visual_GB;
    public GameObject off_visual_GB;


    public class HandTriggerEventBaker : Baker<HandTriggerEventAuthoring>
    {


        public override void Bake(HandTriggerEventAuthoring authoring)
        {

            AddComponent(new HandTriggerEventData
            {
                target_entity = GetEntity(authoring.target_GB),
                action_speed = authoring.action_speed,
                on_visual_entity = GetEntity(authoring.on_visual_GB),
                off_visual_entity = GetEntity(authoring.off_visual_GB)
            });

        }


    }
}