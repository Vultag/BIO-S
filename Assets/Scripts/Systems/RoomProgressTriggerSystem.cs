
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
///[UpdateBefore(typeof(FixedStepSimulationSystemGroup))]

//[UpdateAfter(typeof(FixedStepSimulationSystemGroup))]

public partial class RoomProgressTriggerSystem : SystemBase
{

    private EntityManager entityManager;

    private EntityCommandBuffer ecb;


    protected override void OnCreate()
    {
        base.OnCreate();
        entityManager = World.EntityManager;
        RequireForUpdate<RoomManagerData>();

    }
 

    protected override void OnStartRunning()
    {

        ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

        var Room_entity = SystemAPI.GetSingletonEntity<RoomManagerData>();

        RoomManagerData new_roomManagerData = entityManager.GetComponentData<RoomManagerData>(Room_entity);

        new_roomManagerData.room_0_instance = entityManager.Instantiate(new_roomManagerData.room_0_prefab);

        new_roomManagerData.room_1_2_instance = entityManager.Instantiate(new_roomManagerData.room_1_2_prefab);

        new_roomManagerData.room_3_instance = entityManager.Instantiate(new_roomManagerData.room_3_prefab);

        entityManager.SetComponentData<RoomManagerData>(Room_entity, new_roomManagerData);

    }



    // [BurstCompile]
    struct RoomTriggerEventSystemJo : ITriggerEventsJobBase
    {
        [ReadOnly] public ComponentLookup<RoomProgressData> allroomprogress;

        public NativeReference<Entity> roomprogress_entity;

        public void Execute(TriggerEvent triggerEvent)
        {


            Entity entityA = triggerEvent.EntityA;
            Entity entityB = triggerEvent.EntityB;




            if (allroomprogress.HasComponent(entityA) && allroomprogress.HasComponent(entityB))
            {
                Debug.Log("test_0");
                return;

            }



            if (allroomprogress.HasComponent(entityB))
            {
                //Debug.Log("contactA");

                roomprogress_entity.Value = entityB;


                //Debug.Log(entityA.Index);




            }
            else if (allroomprogress.HasComponent(entityA))
            {

                roomprogress_entity.Value = entityA;

                //Debug.Log(entityB.Index);

            }

        }
    }

    protected override void OnUpdate()
    {

        RoomTriggerEventSystemJo triggerJob = new RoomTriggerEventSystemJo()
        {
            allroomprogress = GetComponentLookup<RoomProgressData>(true),
            roomprogress_entity = new NativeReference<Entity>(Allocator.TempJob)
            //entityCommandBuffer = commandBufferSystem.CreateCommandBuffer()
        };


        Dependency = triggerJob.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), Dependency);

        Dependency.Complete();


        if (entityManager.Exists(triggerJob.roomprogress_entity.Value))
        {
            //Debug.Log("enter");

            // A METTRE DANS UN JOB

            ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            Entity roomprogressEntity = triggerJob.roomprogress_entity.Value;

            
            ecb.DestroyEntity(roomprogressEntity);

            Room_progress_event(entityManager.GetComponentData<RoomProgressData>(roomprogressEntity).state);



        }

        triggerJob.roomprogress_entity.Dispose();

    }


    public void Room_progress_event(int id)
    {

        var Room_entity = SystemAPI.GetSingletonEntity<RoomManagerData>();

        RoomManagerData roomManagerData = entityManager.GetComponentData<RoomManagerData>(Room_entity);

        foreach (var (door_data, trans) in SystemAPI.Query<RefRO<InteractableDoorData>, RefRW<Translation>>())
        {

            trans.ValueRW.Value = new Vector3(trans.ValueRW.Value.x, door_data.ValueRO.starting_y, trans.ValueRW.Value.z);

        }

        switch (id)
        {
            case 0:

                ecb.DestroyEntity(roomManagerData.room_0_instance);



                break;
            case 1:

                RoomManagerData new_roomManagerData = entityManager.GetComponentData<RoomManagerData>(Room_entity);

                new_roomManagerData.room_4_instance = entityManager.Instantiate(new_roomManagerData.room_4_prefab);

                entityManager.SetComponentData<RoomManagerData>(Room_entity, new_roomManagerData);



                break;
            case 2:


                ecb.DestroyEntity(roomManagerData.room_1_2_instance);

                ///Get all sliders and assign full WeaponSliderData
                Entities.WithoutBurst()
                .ForEach((Entity entity, ref PhysicsJoint motor, in Parent parent) =>//, ref WeaponSliderData slider_data) =>
                {
                    Debug.Log(motor.JointType);
                    /*
                    if (motor.JointType == JointType.LinearVelocityMotor)
                    {
                        //Debug.Log(parent.Value);

                        if (entityManager.HasComponent<WeaponSliderData>(parent.Value))
                        {

                            WeaponSliderData new_slider_data = entityManager.GetComponentData<WeaponSliderData>(parent.Value);


                            new_slider_data.Afromjoint = motor.BodyAFromJoint.AsRigidTransform();
                            new_slider_data.Bfromjoint = motor.BodyBFromJoint.AsRigidTransform();
                            new_slider_data.motor_entity = entity;

                            //Debug.Log(entity);
                            ecb.SetComponent<WeaponSliderData>(parent.Value, new_slider_data);


                        }
                        else
                        {
                            Debug.LogError("velocity motor not a gun slider, missing WeaponSliderData");
                        }
                        
                    }
                    */

                }).Run();


                break;
            ///FINAL SCENE
            case 3:

                Entities.WithoutBurst()
                .ForEach((Light light) =>
                {

                    light.color = Color.red;

                }).Run();


                Entities.WithoutBurst()
                .ForEach((ref ZombieSpawnerData spawn) =>
                {
                    spawn.active = true;

                }).Run();

                break;

        }


    }







}