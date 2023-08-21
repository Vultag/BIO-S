using System.Numerics;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Timeline;


public struct FootBalancePushData : IComponentData
{

    public UnityEngine.Vector3 push_location;
    public Entity parent_chest;
    public Entity target_pos_entity;
    public float foot_speed;
    public bool grounded;
}
public class FootBalancePushDataAuthoring : MonoBehaviour
{

    public UnityEngine.Vector3 push_location;
    public GameObject parent_chest;
    public GameObject target_pos_entity;
    public float foot_speed;

    class FootBalancePushDataBaker : Baker<FootBalancePushDataAuthoring>
    {
        public override void Bake(FootBalancePushDataAuthoring authoring)
        {
            AddComponent(new FootBalancePushData
            {
                push_location = authoring.push_location,
                parent_chest = GetEntity(authoring.parent_chest),
                target_pos_entity = GetEntity(authoring.target_pos_entity),
                foot_speed = authoring.foot_speed

            });
        }
    }
}