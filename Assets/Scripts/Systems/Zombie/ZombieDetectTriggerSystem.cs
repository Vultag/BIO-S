
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
[UpdateBefore(typeof(FixedStepSimulationSystemGroup))]
//[UpdateAfter(typeof(EndFramePhysicsSystem))]
public partial class ZombieDetectTriggerSystem : SystemBase
{

    private EntityManager entityManager;

    private EntityCommandBuffer ecb;

    //private EndSimulationEntityCommandBufferSystem commandBufferSystem;


    protected override void OnCreate()
    {
        base.OnCreate();
        entityManager = World.EntityManager;

    }





    // [BurstCompile]
    struct HandTriggerEventSystemJo : ITriggerEventsJobBase
    {
        ///[ReadOnly] public ComponentLookup<PlayerHeadTag> allPlayerHead;
        [ReadOnly] public ComponentLookup<ZombieDetectData> allZombieDetect;


        ///public NativeReference<Entity> Player_entity;
        public NativeReference<Entity> ZombieDetect_entity;

        public void Execute(TriggerEvent triggerEvent)
        {


            Entity entityA = triggerEvent.EntityA;
            Entity entityB = triggerEvent.EntityB;




            if (allZombieDetect.HasComponent(entityA) && allZombieDetect.HasComponent(entityB))
            {
                //I DOIT Y AVOIR UN PROBLEME
                //Debug.Log("test_0");
                return;

            }



            if (allZombieDetect.HasComponent(entityA))
            {
                //Debug.Log("contactA");

                ZombieDetect_entity.Value = entityA;


                Debug.Log(entityB.Index);




            }
            else if (allZombieDetect.HasComponent(entityB))
            {

                //Debug.Log("contactB");

                ZombieDetect_entity.Value = entityB;

                Debug.Log(entityA.Index);

            }

        }
    }

    protected override void OnUpdate()
    {

        HandTriggerEventSystemJo triggerJob = new HandTriggerEventSystemJo()
        {
            allZombieDetect = GetComponentLookup<ZombieDetectData>(true),
            ZombieDetect_entity = new NativeReference<Entity>(Allocator.TempJob)
        };


        Dependency = triggerJob.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), Dependency);

        Dependency.Complete();


        if (entityManager.Exists(triggerJob.ZombieDetect_entity.Value))
        {
            //Debug.Log("enter");

            Player_detected(triggerJob.ZombieDetect_entity.Value);

        }


        triggerJob.ZombieDetect_entity.Dispose();

    }

    private void Player_detected(Entity zombie)
    {
        // A METTRE DANS UN JOB

        ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

        EntityQuery Head_query = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(PlayerHeadTag));
        NativeArray<Entity> entitynativearray = Head_query.ToEntityArray(Unity.Collections.Allocator.TempJob);

        Entity player = entitynativearray[0];


        IKMasterBodyPartData new_ik_L = new IKMasterBodyPartData();
        new_ik_L = entityManager.GetComponentData<IKMasterBodyPartData>(entityManager.GetComponentData<ZombieDetectData>(zombie).Zombie_L_arm);
        new_ik_L.target_pos_entity = player;

        IKMasterBodyPartData new_ik_R = new IKMasterBodyPartData();
        new_ik_R = entityManager.GetComponentData<IKMasterBodyPartData>(entityManager.GetComponentData<ZombieDetectData>(zombie).Zombie_R_arm);
        new_ik_R.target_pos_entity = player;

        ecb.SetComponent<IKMasterBodyPartData>(entityManager.GetComponentData<ZombieDetectData>(zombie).Zombie_L_arm, new_ik_L);
        ecb.SetComponent<IKMasterBodyPartData>(entityManager.GetComponentData<ZombieDetectData>(zombie).Zombie_R_arm, new_ik_R);

        ecb.RemoveComponent<Disabled>(entityManager.GetComponentData<ZombieDetectData>(zombie).Zombie_R_arm);
        ecb.RemoveComponent<Disabled>(entityManager.GetComponentData<ZombieDetectData>(zombie).Zombie_L_arm);

        ZombieBalanceData new_balance = new ZombieBalanceData();
        new_balance = entityManager.GetComponentData<ZombieBalanceData>(entityManager.GetComponentData<ZombieDetectData>(zombie).Zombie_balance);
        new_balance.zombie_target = player;

        ecb.SetComponent<ZombieBalanceData>(entityManager.GetComponentData<ZombieDetectData>(zombie).Zombie_balance, new_balance);

        ecb.DestroyEntity(zombie);



        Head_query.Dispose();
    }


}