
using System.Numerics;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;


public partial struct DelayedDestroyData : IComponentData
{
    public float destroy_timer;
    public Entity zombie_brain_entity;
}
public class DelayedDestroyDataAuthoring : MonoBehaviour
{

    public float destroy_timer;

    class DelayedDestroyDataBaker : Baker<DelayedDestroyDataAuthoring>
    {
        public override void Bake(DelayedDestroyDataAuthoring authoring)
        {

            AddComponent(new DelayedDestroyData
            {
                destroy_timer = authoring.destroy_timer

            });
        }
    }
}