
using Unity.Entities;
using UnityEngine;
using Unity.Jobs;
using Unity.Physics;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine.UI;
using Unity.Physics.Extensions;
using System.Numerics;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
using Unity.Physics.Systems;
using Unity.Physics.Authoring;
using Unity.Core;
using Unity.VisualScripting;
using Unity.Entities.UniversalDelegates;
using Unity.Rendering;
using static Unity.Physics.Math;


//[AlwaysSynchronizeSystem]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
///[UpdateBefore(typeof(FixedStepSimulationSystemGroup))]
//[UpdateAfter(typeof(EndFramePhysicsSystem))]
public partial class HandTriggerEventSystem : SystemBase
{

    private EntityManager entityManager;

    private EntityCommandBuffer ecb;


    protected override void OnCreate()
    {
        base.OnCreate();
        entityManager = World.EntityManager;

    }





    // [BurstCompile]
    struct HandTriggerEventSystemJo : ITriggerEventsJobBase
    {
        [ReadOnly] public ComponentLookup<RightHandTag> allRHand;
        [ReadOnly] public ComponentLookup<HandTriggerEventData> allTrigger;


        public NativeReference<Entity> Trigger_entity;
        public NativeReference<Entity> RHand_entity;

        public void Execute(TriggerEvent triggerEvent)
        {


            Entity entityA = triggerEvent.EntityA;
            Entity entityB = triggerEvent.EntityB;




            if (allTrigger.HasComponent(entityA) && allTrigger.HasComponent(entityB))
            {
                Debug.Log("test_0");
                return;

            }



            if (allRHand.HasComponent(entityB) && allTrigger.HasComponent(entityA))
            {
                //Debug.Log("contactA");

                Trigger_entity.Value = entityA;

                RHand_entity.Value = entityB;






            }
            else if (allRHand.HasComponent(entityA) && allTrigger.HasComponent(entityB))
            {

                //Debug.Log("contactB");

                Trigger_entity.Value = entityB;

                RHand_entity.Value = entityA;

            }

        }
    }

    protected override void OnUpdate()
    {

        HandTriggerEventSystemJo triggerJob = new HandTriggerEventSystemJo()
        {
            allRHand = GetComponentLookup<RightHandTag>(true),
            allTrigger = GetComponentLookup<HandTriggerEventData>(true),
            RHand_entity = new NativeReference<Entity>(Allocator.TempJob),
            Trigger_entity = new NativeReference<Entity>(Allocator.TempJob)
        };


        Dependency = triggerJob.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), Dependency);

        Dependency.Complete();


        if (entityManager.Exists(triggerJob.RHand_entity.Value))
        {
            //Debug.Log("enter");

            // A METTRE DANS UN JOB

            ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            Entity HandEntity = triggerJob.RHand_entity.Value;
            Entity TriggerEntity = triggerJob.Trigger_entity.Value;

            ecb.RemoveComponent<Disabled>(entityManager.GetComponentData<HandTriggerEventData>(TriggerEntity).on_visual_entity);
            ecb.AddComponent<Disabled>(entityManager.GetComponentData<HandTriggerEventData>(TriggerEntity).off_visual_entity);


            DoorData new_door_data = new DoorData();

            new_door_data.starting_world_y = entityManager.GetComponentData<LocalToWorld>(entityManager.GetComponentData<HandTriggerEventData>(TriggerEntity).target_entity).Position.y;
            new_door_data.target_local_y = 2.5f;
            new_door_data.open_speed = entityManager.GetComponentData<HandTriggerEventData>(TriggerEntity).action_speed;

            ecb.AddComponent<DoorData>(entityManager.GetComponentData<HandTriggerEventData>(TriggerEntity).target_entity, new_door_data);

            ecb.RemoveComponent<HandTriggerEventData>(TriggerEntity);


        }






        triggerJob.RHand_entity.Dispose();
        triggerJob.Trigger_entity.Dispose();

    }
}