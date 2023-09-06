
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
using System;
using Unity.Burst;


[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
///[UpdateBefore(typeof(FixedStepSimulationSystemGroup))]


public partial class LoadMagOnColSystem : SystemBase
{

    private XRInputsActionSystem XRInputSystem;

    private EntityManager entityManager;

    private EntityCommandBuffer ecb;



    protected override void OnCreate()
    {
        base.OnCreate();
        entityManager = World.EntityManager;
        XRInputSystem = World.GetExistingSystemManaged<XRInputsActionSystem>();

    }





    // [BurstCompile]
    struct LoadMagOnTriggerSystemJob : ITriggerEventsJobBase
    {
        [ReadOnly] public ComponentLookup<MagazineChamberTag> all_magazine_chamber;
        [ReadOnly] public ComponentLookup<MagazineTag> all_magazine;

        [ReadOnly] public PhysicsWorld PhysicsWorld;

        public NativeReference<Entity> mag_entity;
        public NativeReference<Entity> item_entity;


        public void Execute(TriggerEvent triggerEvent)
        {

            Entity entityA = triggerEvent.EntityA;
            Entity entityB = triggerEvent.EntityB;


            if (all_magazine_chamber.HasComponent(entityA) && all_magazine_chamber.HasComponent(entityB))
            {
                Debug.Log("test_0");
                return;
            }


            if (all_magazine.HasComponent(entityA) && all_magazine_chamber.HasComponent(entityB))
            {
                //Debug.Log("HIT");
                mag_entity.Value = entityA;
                item_entity.Value = entityB;


                //Debug.DrawLine(collisionEvent.CalculateDetails(ref PhysicsWorld).EstimatedContactPointPositions[0], (Vector3)float3.zero,Color.red,5f);

            }
            else if (all_magazine.HasComponent(entityB) && all_magazine_chamber.HasComponent(entityA))
            {

                //Debug.Log("HIT");
                mag_entity.Value = entityB;
                item_entity.Value = entityA;


                //Debug.DrawLine(collisionEvent.CalculateDetails(ref PhysicsWorld).EstimatedContactPointPositions[0], (Vector3)float3.zero, Color.red, 5f);

            }
        }
    }



    protected override void OnUpdate()
    {

        LoadMagOnTriggerSystemJob triggerJob = new LoadMagOnTriggerSystemJob()
        {
            all_magazine_chamber = GetComponentLookup<MagazineChamberTag>(true),
            all_magazine = GetComponentLookup<MagazineTag>(true),
            PhysicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld,
            //grabdetect_from_entity = GetComponentLookup<GrabDetectData>(false),
            //grabbable_from_entity = GetComponentLookup<ItemData>(false),
            item_entity = new NativeReference<Entity>(Allocator.TempJob),
            mag_entity = new NativeReference<Entity>(Allocator.TempJob)
            //entityCommandBuffer = commandBufferSystem.CreateCommandBuffer()
        };


        Dependency = triggerJob.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), Dependency);

        Dependency.Complete();

        if (entityManager.Exists(triggerJob.item_entity.Value))
        {


            ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            //Debug.Log("HIT");


            Entity entityA = triggerJob.item_entity.Value;
            Entity entityB = triggerJob.mag_entity.Value;

            Entity jointEntity = Entity.Null;

            if (entityManager.HasComponent<StorableData>(entityB))
            {
                if (entityManager.GetComponentData<StorableData>(entityB).storing_entity != Entity.Null)
                {
                    HasToExitData exit_data = new HasToExitData();
                    exit_data.physics_layer = 4;
                    exit_data.shape_size = entityManager.GetComponentData<SlotData>(entityManager.GetComponentData<StorableData>(entityB).storing_entity).shape_size;
                    exit_data.shape_entity = entityManager.GetComponentData<StorableData>(entityB).storing_entity;

                    ecb.AddComponent<HasToExitData>(entityManager.GetComponentData<StorableData>(entityB).storing_entity, exit_data);

                    StorableData new_Storable_data = new StorableData();
                    new_Storable_data.storing_entity = Entity.Null;
                    ecb.SetComponent<StorableData>(entityB, new_Storable_data);
                    ecb.DestroyEntity(entityManager.GetComponentData<StorableData>(entityB).store_joint_0);
                    if (entityManager.GetComponentData<StorableData>(entityB).store_joint_1 != Entity.Null)
                        ecb.DestroyEntity(entityManager.GetComponentData<StorableData>(entityB).store_joint_1);
                    ecb.AddComponent<SlotTag>(entityManager.GetComponentData<StorableData>(entityB).storing_entity);
                    ecb.AddComponent<StorableTag>(entityB);
                }
            }





            switch (entityManager.GetComponentData<MagazineData>(triggerJob.mag_entity.Value).max_bullets_in_clip)
            {

                case 10: //USP

                    //test l angle des deux object pour voir si la jonction se fait            
                    //if (Quaternion.Angle(entityManager.GetComponentData<LocalToWorld>(triggerJob.item_entity.Value).Rotation, entityManager.GetComponentData<LocalToWorld>(triggerJob.mag_entity.Value).Rotation) < 25f)
                    {


                        //ecbBS = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);


                        ecb.RemoveComponent<MagazineChamberTag>(triggerJob.item_entity.Value);
                        ecb.RemoveComponent<MagazineTag>(triggerJob.mag_entity.Value);
                        ecb.RemoveComponent<StorableTag>(triggerJob.mag_entity.Value);
                        ecb.RemoveComponent<GrabbableTag>(entityB);



                        // DISABLE LES COL AVEC LA SLIDE

                        MagazineData mag_data = new MagazineData();

                        mag_data = entityManager.GetComponentData<MagazineData>(entityB);

                        Entity slide_entity = entityManager.GetComponentData<ItemData>(entityA).composite_sub_part;

                        ComponentType[] col_joint_componentTypes =
                        {
                        typeof(PhysicsConstrainedBodyPair),
                        typeof(PhysicsJoint)
                        };

                        var col_joint_constraints = new FixedList512Bytes<Constraint>();
                        col_joint_constraints.Add(new Constraint
                        {
                            ConstrainedAxes = new bool3(false),
                            Type = ConstraintType.Angular,
                            Min = -math.PI,
                            Max = math.PI,
                            SpringFrequency = Constraint.DefaultSpringFrequency,
                            SpringDamping = Constraint.DefaultSpringDamping
                        });
                        var col_joint = new PhysicsJoint()
                        {
                            BodyAFromJoint = BodyFrame.Identity,
                            BodyBFromJoint = BodyFrame.Identity,
                        };
                        col_joint.SetConstraints(col_joint_constraints);

                        Entity col_jointEntity = entityManager.CreateEntity(col_joint_componentTypes);

                        entityManager.SetComponentData(col_jointEntity, new PhysicsConstrainedBodyPair(slide_entity, entityB, false));

                        ecb.AddSharedComponent(col_jointEntity, new PhysicsWorldIndex());

                        mag_data.disable_col_joint = col_jointEntity;

                        ecb.SetComponent<MagazineData>(entityB, mag_data);

                        /*

                        PhysicsCollider new_mag_col = entityManager.GetComponentData<PhysicsCollider>(entityB);

                        CollisionFilter old_filter = new_mag_col.Value.Value.GetCollisionFilter();

                        new_mag_col.Value.Value.SetCollisionFilter(new CollisionFilter
                        {
                            BelongsTo = old_filter.BelongsTo,
                            CollidesWith = (uint)(1<<0) | (uint)(1<<1) | (uint)(1<<2) | (uint)()
                        });

                        ecb.SetComponent<PhysicsCollider>(entityB,);
                        */


                        float3 relative_pos = entityManager.GetComponentData<LocalToWorld>(entityB).Position - entityManager.GetComponentData<LocalToWorld>(entityA).Position;
                        Quaternion relative_rot = Quaternion.Inverse((Quaternion)entityManager.GetComponentData<LocalToWorld>(entityA).Rotation) * entityManager.GetComponentData<LocalToWorld>(entityB).Rotation;


                        //Debug.Log("joint");

                        /*
                        snap_joint = PhysicsJoint.CreatePrismatic(
                        new RigidTransform(Quaternion.identity, new float3(0, 0, 0)),
                        //new RigidTransform(Quaternion.Inverse(relative_rot), (triggerJob.collision_point.Value - entityManager.GetComponentData<LocalToWorld>(entityB).Position)) //la rot c la rotation du joint ? et la pos c la pos relative a l autre 
                        //new RigidTransform(relative_rot, relative_pos ),
                        new RigidTransform(Quaternion.identity, new float3(0, 0, 0)),
                        new Math.FloatRange(0, 0.01f)//la rot c la rotation du joint ? et la pos c la pos relative a l autre 
                        );
                        */


                        PhysicsJoint USP_snap_joint;

                        //var localFrame = new BodyFrame { Axis = new float3(0,1,0.13f), PerpendicularAxis = new float3(0, 0, 1), Position = float3.zero };
                        var USP_localFrame = new BodyFrame { Axis = new float3(0, -0.13f, 0.87f), PerpendicularAxis = new float3(0, 1, 0), Position = float3.zero };
                        var USP_worldFrame = new BodyFrame { Axis = new float3(0, -0.13f, 0.87f), PerpendicularAxis = new float3(0, 1, 0), Position = float3.zero };
                        //float3 pivotTorso = math.transform(math.inverse(GetBodyTransform(torso)), math.transform(GetBodyTransform(head), pivotHead));
                        //var axisTorso = math.rotate(math.inverse(GetBodyTransform(torso).rot), math.rotate(GetBodyTransform(head).rot, axisHead));

                        USP_snap_joint = PhysicsJoint.CreateFixed(
                            USP_localFrame,
                            USP_worldFrame

                        );



                        ComponentType[] USP_componentTypes =
                        {
                        typeof(PhysicsConstrainedBodyPair),
                        typeof(PhysicsJoint)
                        };

                        var USP_constraints = new FixedList512Bytes<Constraint>();

                        //constraints.Add(Constraint.Planar(1,new Math.FloatRange(0, 0.1f)));

                        USP_constraints.Add(new Constraint
                        {
                            ConstrainedAxes = new bool3(true, false, true),//slide,pull,starf
                            Type = ConstraintType.Linear,
                            Min = 0,
                            Max = 0,
                            SpringFrequency = 800000f,//74341.31f,
                            SpringDamping = 2530.126f,
                            MaxImpulse = float.PositiveInfinity,//new float3(math.INFINITY, math.INFINITY, math.INFINITY),
                        });



                        USP_constraints.Add(new Constraint
                        {
                            ConstrainedAxes = new bool3(true, true, true), // ptich, roll, yaw
                            Type = ConstraintType.Angular,
                            Min = 0,
                            Max = 0,
                            SpringFrequency = 800000f,//74341.31f,
                            SpringDamping = 2530.126f,
                            MaxImpulse = float.PositiveInfinity,//new float3(math.INFINITY, math.INFINITY, math.INFINITY),
                        });

                        /*
                        constraints.Add(new Constraint
                        {
                            ConstrainedAxes = new bool3(false, false, true),
                            Type = ConstraintType.Angular,
                            Min = 0,
                            Max = 0,
                            SpringFrequency = 800000f,//74341.31f,
                            SpringDamping = 0,//2530.126f,
                            MaxImpulse = 0,//new float3(math.INFINITY, math.INFINITY, math.INFINITY),
                            EnableImpulseEvents = false
                        });
                        */
                        USP_snap_joint.SetConstraints(USP_constraints);

                        jointEntity = entityManager.CreateEntity(USP_componentTypes);

                        entityManager.SetComponentData(jointEntity, new PhysicsConstrainedBodyPair(entityA, entityB, false));
                        entityManager.SetComponentData(jointEntity, USP_snap_joint);

                        ecb.AddSharedComponent(jointEntity, new PhysicsWorldIndex());



                        MagazineAttachmentData attach_data = new MagazineAttachmentData();

                        attach_data.mag_joint_entity = jointEntity;
                        attach_data.mag_entity = entityB;
                        attach_data.firearm_entity = entityA;


                        ecb.AddComponent(entityB, attach_data);

                    }
                    break;
                case 20: //Skorpion



                    //ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
                    //ecbBS = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);


                    if (entityManager.GetComponentData<MagazineData>(triggerJob.mag_entity.Value).bullets_in_clip > 0)
                    {
                        WeaponSliderData slider_data = entityManager.GetComponentData<WeaponSliderData>(entityManager.GetComponentData<FirearmData>(entityA).slider_entity);

                        var new_motor = PhysicsJoint.CreateLinearVelocityMotor(
                               slider_data.Afromjoint,
                               slider_data.Bfromjoint,
                               slider_data.motor_target_speed,
                               slider_data.motor_max_impulse_applided
                           );

                        Debug.Log(slider_data.motor_target_speed);

                        ecb.SetComponent<PhysicsJoint>(slider_data.motor_entity, new_motor);
                    }



                    ecb.RemoveComponent<MagazineChamberTag>(triggerJob.item_entity.Value);
                    ecb.RemoveComponent<MagazineTag>(triggerJob.mag_entity.Value);
                    ecb.RemoveComponent<StorableTag>(triggerJob.mag_entity.Value);


                    FirearmData new_firearm_data;

                    new_firearm_data = entityManager.GetComponentData<FirearmData>(entityA);

                    new_firearm_data.loaded_magazine = entityB;

                    Entity grabing_hand = Entity.Null;



                    if (entityManager.GetComponentData<ItemData>(entityB).hand_grabbing == 1)
                    {
                        grabing_hand = GetEntityQuery(typeof(RightHandTag), typeof(GrabDetectData)).ToEntityArray(Allocator.TempJob)[0];
                        //Debug.Log(grabing_hand);
                        //Debug.Log(entityManager.GetComponentData<GrabDetectData>(grabing_hand).grabbed_item);
                        XRInputSystem.drop_item(grabing_hand);
                    }
                    else if (entityManager.GetComponentData<ItemData>(entityB).hand_grabbing == 2)
                    {
                        grabing_hand = GetEntityQuery(typeof(LeftHandTag), typeof(GrabDetectData)).ToEntityArray(Allocator.TempJob)[0];
                        XRInputSystem.drop_item(grabing_hand);
                    }




                    //pas la ou sa devrait etre mais pas de mechanique de chamber pour l instant
                    if (entityManager.HasComponent<Disabled>(entityManager.GetComponentData<FirearmData>(entityA).chamber_bullet) && entityManager.GetComponentData<MagazineData>(entityB).bullets_in_clip > 0)
                        ecb.RemoveComponent<Disabled>(entityManager.GetComponentData<FirearmData>(entityA).chamber_bullet);



                    ecb.SetComponent<FirearmData>(entityA, new_firearm_data);

                    ecb.RemoveComponent<GrabbableTag>(entityB);

                    PhysicsJoint snap_joint;


                    var localFrame = new BodyFrame { Axis = new float3(0, -0.13f, 0.87f), PerpendicularAxis = new float3(0, 1, 0), Position = float3.zero };
                    var worldFrame = new BodyFrame { Axis = new float3(0, -0.13f, 0.87f), PerpendicularAxis = new float3(0, 1, 0), Position = float3.zero };

                    snap_joint = PhysicsJoint.CreateFixed(
                        localFrame,
                        worldFrame

                    );

                    ComponentType[] componentTypes =
                    {
                typeof(PhysicsConstrainedBodyPair),
                typeof(PhysicsJoint)
                };

                    var constraints = new FixedList512Bytes<Constraint>();

                    //constraints.Add(Constraint.Planar(1,new Math.FloatRange(0, 0.1f)));

                    constraints.Add(new Constraint
                    {
                        ConstrainedAxes = new bool3(true, true, true),//slide,pull,starf
                        Type = ConstraintType.Linear,
                        Min = 0,
                        Max = 0,
                        SpringFrequency = 800000f,//74341.31f,
                        SpringDamping = 2530.126f,
                        MaxImpulse = float.PositiveInfinity,//new float3(math.INFINITY, math.INFINITY, math.INFINITY),
                    });



                    constraints.Add(new Constraint
                    {
                        ConstrainedAxes = new bool3(true, true, true), // ptich, roll, yaw
                        Type = ConstraintType.Angular,
                        Min = 0,
                        Max = 0,
                        SpringFrequency = 800000f,//74341.31f,
                        SpringDamping = 2530.126f,
                        MaxImpulse = float.PositiveInfinity,//new float3(math.INFINITY, math.INFINITY, math.INFINITY),
                    });


                    snap_joint.SetConstraints(constraints);

                    jointEntity = entityManager.CreateEntity(componentTypes);

                    entityManager.SetComponentData(jointEntity, new PhysicsConstrainedBodyPair(entityB, entityA, false));
                    entityManager.SetComponentData(jointEntity, snap_joint);

                    ecb.AddSharedComponent(jointEntity, new PhysicsWorldIndex());

                    MagazineData new_mag_data = new MagazineData();

                    new_mag_data = entityManager.GetComponentData<MagazineData>(entityB);

                    new_mag_data.loaded_mag_joint = jointEntity;

                    ecb.SetComponent<MagazineData>(entityB, new_mag_data);

                    break;

            }
        }

        triggerJob.mag_entity.Dispose();
        triggerJob.item_entity.Dispose();
    }
}