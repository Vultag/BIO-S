
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
//[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
///[UpdateBefore(typeof(FixedStepSimulationSystemGroup))]
//[UpdateAfter(typeof(EndFramePhysicsSystem))]
public partial class StoreOnTriggerSystem : SystemBase
{

    private XRInputsActionSystem XRInputSystem;

    private EntityManager entityManager;

    private EntityCommandBuffer ecb;

    protected override void OnCreate()
    {
        base.OnCreate();
        XRInputSystem = World.GetExistingSystemManaged<XRInputsActionSystem>();
        entityManager = World.EntityManager;

    }





    // [BurstCompile]
    struct StoreOnTriggerSystemJo : ITriggerEventsJobBase
    {
        [ReadOnly] public ComponentLookup<StorableTag> allStorable;
        [ReadOnly] public ComponentLookup<SlotTag> allslots;


        public NativeReference<Entity> slot_entity;
        public NativeReference<Entity> storable_entity;

        public void Execute(TriggerEvent triggerEvent)
        {


            Entity entityA = triggerEvent.EntityA;
            Entity entityB = triggerEvent.EntityB;




            if (allslots.HasComponent(entityA) && allslots.HasComponent(entityB))
            {
                Debug.Log("test_0");
                return;

            }



            if (allStorable.HasComponent(entityB) && allslots.HasComponent(entityA))
            {
                //Debug.Log("contactA");

                slot_entity.Value = entityA;

                storable_entity.Value = entityB;






            }
            else if (allStorable.HasComponent(entityA) && allslots.HasComponent(entityB))
            {

                //Debug.Log("contactB");

                slot_entity.Value = entityB;

                storable_entity.Value = entityA;

            }

        }
    }

    protected override void OnUpdate()
    {

        StoreOnTriggerSystemJo triggerJob = new StoreOnTriggerSystemJo()
        {
            allStorable = GetComponentLookup<StorableTag>(true),
            allslots = GetComponentLookup<SlotTag>(true),
            storable_entity = new NativeReference<Entity>(Allocator.TempJob),
            slot_entity = new NativeReference<Entity>(Allocator.TempJob)
        };


        Dependency = triggerJob.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), Dependency);

        Dependency.Complete();


        if (entityManager.Exists(triggerJob.storable_entity.Value))
        {

            // A METTRE DANS UN JOB

            ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            Entity SlotEntity = triggerJob.slot_entity.Value;
            Entity StorableEntity = triggerJob.storable_entity.Value;


            // sert a empecher de re store instant quand on sort l item
            if(!entityManager.HasComponent<HasToExitData>(triggerJob.slot_entity.Value))
            {
                //pass
            }
            else
            {
                return;
            }


            // pour que un slot puisse contenir des item differents
            var item_store_id = entityManager.GetComponentData<ItemData>(StorableEntity).item_type_id;
            if (item_store_id == 4)
                item_store_id = 1;


            //check que se sois bien le slot correspondant a l item
            if (entityManager.GetComponentData<SlotData>(SlotEntity).slot_type_id == item_store_id)
            {
                //Debug.Log("execute");

                StorableData new_Storable_data = new StorableData();
                new_Storable_data.storing_entity = SlotEntity;
                

                Entity grabing_hand = Entity.Null;

                //force hand drop
                if (entityManager.GetComponentData<ItemData>(StorableEntity).hand_grabbing == 1)
                {
                    grabing_hand = GetEntityQuery(typeof(RightHandTag), typeof(GrabDetectData)).ToEntityArray(Allocator.TempJob)[0];
                    XRInputSystem.drop_item(grabing_hand);
                }
                else if (entityManager.GetComponentData<ItemData>(StorableEntity).hand_grabbing == 2)
                {
                    grabing_hand = GetEntityQuery(typeof(LeftHandTag), typeof(GrabDetectData)).ToEntityArray(Allocator.TempJob)[0];
                    XRInputSystem.drop_item(grabing_hand);
                }



                PhysicsJoint store_joint;

                Quaternion slot_local_rot;
                float3 slot_local_pos;


                ComponentType[] store_joint_componentTypes;


                var store_joint_constraints = new FixedList512Bytes<Constraint>();

                Entity joint_entity;

                SlotData new_Slot_data = new SlotData();


                //specifique a l instanciation du joint 
                switch (entityManager.GetComponentData<SlotData>(SlotEntity).slot_type_id)
                {


                    //Katana
                    case 0:


                        slot_local_rot =  Quaternion.Inverse(entityManager.GetComponentData<LocalToWorld>(entityManager.GetComponentData<SlotData>(SlotEntity).Storing_entity).Rotation) * entityManager.GetComponentData<LocalToWorld>(SlotEntity).Rotation;
                        slot_local_pos = entityManager.GetComponentData<LocalToWorld>(SlotEntity).Position - entityManager.GetComponentData<LocalToWorld>(entityManager.GetComponentData<SlotData>(SlotEntity).Storing_entity).Position;


                        PhysicsJoint.CreateRagdoll(
                            new RigidTransform(slot_local_rot, (Quaternion)Quaternion.Inverse(entityManager.GetComponentData<LocalToWorld>(entityManager.GetComponentData<SlotData>(SlotEntity).Storing_entity).Rotation) * slot_local_pos),
                            new RigidTransform(Quaternion.identity, float3.zero),
                            math.radians(10), 
                            math.radians(new FloatRange(0, 0)),
                            math.radians(new FloatRange(0,0)),
                            out var ragdoll0,
                            out var ragdoll1


                        );


                        store_joint_componentTypes = new ComponentType[]
                        {
                        typeof(PhysicsConstrainedBodyPair),
                        typeof(PhysicsJoint)
                        };


                        Entity ragjoint0_entity = entityManager.CreateEntity(store_joint_componentTypes);
                        Entity ragjoint1_entity = entityManager.CreateEntity(store_joint_componentTypes);

                        entityManager.SetComponentData(ragjoint0_entity, new PhysicsConstrainedBodyPair(entityManager.GetComponentData<SlotData>(SlotEntity).Storing_entity, StorableEntity, false));
                        entityManager.SetComponentData(ragjoint1_entity, new PhysicsConstrainedBodyPair(entityManager.GetComponentData<SlotData>(SlotEntity).Storing_entity, StorableEntity, false));

                        entityManager.SetComponentData(ragjoint0_entity, ragdoll0);
                        entityManager.SetComponentData(ragjoint1_entity, ragdoll1);

                        ecb.AddSharedComponent(ragjoint0_entity, new PhysicsWorldIndex());
                        ecb.AddSharedComponent(ragjoint1_entity, new PhysicsWorldIndex());

                        new_Storable_data.store_joint_0 = ragjoint0_entity;
                        new_Storable_data.store_joint_1 = ragjoint1_entity;


                        new_Slot_data = entityManager.GetComponentData<SlotData>(SlotEntity);
                        new_Slot_data.has_to_exit = true;
                        ecb.SetComponent<SlotData>(SlotEntity, new_Slot_data);

                        ecb.SetComponent<StorableData>(StorableEntity, new_Storable_data);


                        break;

                    //USP
                    case 1:

                        slot_local_rot = Quaternion.Inverse(entityManager.GetComponentData<LocalToWorld>(entityManager.GetComponentData<SlotData>(SlotEntity).Storing_entity).Rotation) * entityManager.GetComponentData<LocalToWorld>(SlotEntity).Rotation;
                        slot_local_pos = entityManager.GetComponentData<LocalToWorld>(SlotEntity).Position - entityManager.GetComponentData<LocalToWorld>(entityManager.GetComponentData<SlotData>(SlotEntity).Storing_entity).Position;


                        store_joint = PhysicsJoint.CreateFixed(
                            new RigidTransform(slot_local_rot, (Quaternion)Quaternion.Inverse(entityManager.GetComponentData<LocalToWorld>(entityManager.GetComponentData<SlotData>(SlotEntity).Storing_entity).Rotation) * slot_local_pos),
                            new RigidTransform(Quaternion.identity, float3.zero)

                        );


                        store_joint_componentTypes = new ComponentType[]
                        {
                        typeof(PhysicsConstrainedBodyPair),
                        typeof(PhysicsJoint)
                        };


                        store_joint_constraints.Add(new Constraint
                        {
                            ConstrainedAxes = new bool3(true),//slide,pull,starf
                            Type = ConstraintType.Linear,
                            Min = 0,
                            Max = 0,
                            SpringFrequency = 800000f,//74341.31f,
                            SpringDamping = 2530.126f,
                            MaxImpulse = float.PositiveInfinity,//new float3(math.INFINITY, math.INFINITY, math.INFINITY),
                        });
                        store_joint_constraints.Add(new Constraint
                        {
                            ConstrainedAxes = new bool3(true), // ptich, roll, yaw
                            Type = ConstraintType.Angular,
                            Min = 0,
                            Max = 0,
                            SpringFrequency = 800000f,//74341.31f,
                            SpringDamping = 2530.126f,
                            MaxImpulse = float.PositiveInfinity,//new float3(math.INFINITY, math.INFINITY, math.INFINITY),
                        });
                   
                        store_joint.SetConstraints(store_joint_constraints);

                        joint_entity = entityManager.CreateEntity(store_joint_componentTypes);

                        entityManager.SetComponentData(joint_entity, new PhysicsConstrainedBodyPair(entityManager.GetComponentData<SlotData>(SlotEntity).Storing_entity, StorableEntity, false));
                        entityManager.SetComponentData(joint_entity, store_joint);

                        ecb.AddSharedComponent(joint_entity, new PhysicsWorldIndex());

                        new_Storable_data.store_joint_0 = joint_entity;


                        new_Slot_data = entityManager.GetComponentData<SlotData>(SlotEntity);
                        new_Slot_data.has_to_exit = true;
                        ecb.SetComponent<SlotData>(SlotEntity, new_Slot_data);

                        ecb.SetComponent<StorableData>(StorableEntity, new_Storable_data);


                        break;

                    //MAG
                    case 3:

                        slot_local_rot =  Quaternion.Inverse(entityManager.GetComponentData<LocalToWorld>(entityManager.GetComponentData<SlotData>(SlotEntity).Storing_entity).Rotation) * entityManager.GetComponentData<LocalToWorld>(SlotEntity).Rotation;
                        slot_local_pos = entityManager.GetComponentData<LocalToWorld>(SlotEntity).Position - entityManager.GetComponentData<LocalToWorld>(entityManager.GetComponentData<SlotData>(SlotEntity).Storing_entity).Position;


                        store_joint = PhysicsJoint.CreateFixed(
                            new RigidTransform(slot_local_rot, (Quaternion)Quaternion.Inverse(entityManager.GetComponentData<LocalToWorld>(entityManager.GetComponentData<SlotData>(SlotEntity).Storing_entity).Rotation) * slot_local_pos),
                            new RigidTransform(Quaternion.identity, float3.zero)

                        );


                        store_joint_componentTypes = new ComponentType[]
                        {
                        typeof(PhysicsConstrainedBodyPair),
                        typeof(PhysicsJoint)
                        };


                        store_joint_constraints.Add(new Constraint
                        {
                            ConstrainedAxes = new bool3(true),//slide,pull,starf
                            Type = ConstraintType.Linear,
                            Min = 0,
                            Max = 0,
                            SpringFrequency = 800000f,//74341.31f,
                            SpringDamping = 2530.126f,
                            MaxImpulse = float.PositiveInfinity,//new float3(math.INFINITY, math.INFINITY, math.INFINITY),
                        });
                        store_joint_constraints.Add(new Constraint
                        {
                            ConstrainedAxes = new bool3(true), // ptich, roll, yaw
                            Type = ConstraintType.Angular,
                            Min = 0,
                            Max = 0,
                            SpringFrequency = 800000f,//74341.31f,
                            SpringDamping = 2530.126f,
                            MaxImpulse = float.PositiveInfinity,//new float3(math.INFINITY, math.INFINITY, math.INFINITY),
                        });

                        store_joint.SetConstraints(store_joint_constraints);

                        joint_entity = entityManager.CreateEntity(store_joint_componentTypes);


                        entityManager.SetComponentData(joint_entity, new PhysicsConstrainedBodyPair(entityManager.GetComponentData<SlotData>(SlotEntity).Storing_entity, StorableEntity, false));
                        entityManager.SetComponentData(joint_entity, store_joint);

                        ecb.AddSharedComponent(joint_entity, new PhysicsWorldIndex());


                        new_Storable_data.store_joint_0 = joint_entity;


                        new_Slot_data = entityManager.GetComponentData<SlotData>(SlotEntity);
                        new_Slot_data.has_to_exit = true;
                        ecb.SetComponent<SlotData>(SlotEntity, new_Slot_data);

                        ecb.SetComponent<StorableData>(StorableEntity, new_Storable_data);


                        break;

                        //SKORPION
                    case 4:

                        Debug.Log("skorp");

                        slot_local_rot = Quaternion.Inverse(entityManager.GetComponentData<LocalToWorld>(entityManager.GetComponentData<SlotData>(SlotEntity).Storing_entity).Rotation) * entityManager.GetComponentData<LocalToWorld>(SlotEntity).Rotation;
                        slot_local_pos = entityManager.GetComponentData<LocalToWorld>(SlotEntity).Position - entityManager.GetComponentData<LocalToWorld>(entityManager.GetComponentData<SlotData>(SlotEntity).Storing_entity).Position;


                        store_joint = PhysicsJoint.CreateFixed(
                            new RigidTransform(slot_local_rot, (Quaternion)Quaternion.Inverse(entityManager.GetComponentData<LocalToWorld>(entityManager.GetComponentData<SlotData>(SlotEntity).Storing_entity).Rotation) * slot_local_pos),
                            new RigidTransform(Quaternion.identity, float3.zero)

                        );


                        store_joint_componentTypes = new ComponentType[]
                        {
                        typeof(PhysicsConstrainedBodyPair),
                        typeof(PhysicsJoint)
                        };


                        store_joint_constraints.Add(new Constraint
                        {
                            ConstrainedAxes = new bool3(true),//slide,pull,starf
                            Type = ConstraintType.Linear,
                            Min = 0,
                            Max = 0,
                            SpringFrequency = 800000f,//74341.31f,
                            SpringDamping = 2530.126f,
                            MaxImpulse = float.PositiveInfinity,//new float3(math.INFINITY, math.INFINITY, math.INFINITY),
                        });
                        store_joint_constraints.Add(new Constraint
                        {
                            ConstrainedAxes = new bool3(true), // ptich, roll, yaw
                            Type = ConstraintType.Angular,
                            Min = 0,
                            Max = 0,
                            SpringFrequency = 800000f,//74341.31f,
                            SpringDamping = 2530.126f,
                            MaxImpulse = float.PositiveInfinity,//new float3(math.INFINITY, math.INFINITY, math.INFINITY),
                        });

                        store_joint.SetConstraints(store_joint_constraints);

                        joint_entity = entityManager.CreateEntity(store_joint_componentTypes);

                        entityManager.SetComponentData(joint_entity, new PhysicsConstrainedBodyPair(entityManager.GetComponentData<SlotData>(SlotEntity).Storing_entity, StorableEntity, false));
                        entityManager.SetComponentData(joint_entity, store_joint);

                        ecb.AddSharedComponent(joint_entity, new PhysicsWorldIndex());

                        new_Storable_data.store_joint_0 = joint_entity;


                        new_Slot_data = entityManager.GetComponentData<SlotData>(SlotEntity);
                        new_Slot_data.has_to_exit = true;
                        ecb.SetComponent<SlotData>(SlotEntity, new_Slot_data);

                        ecb.SetComponent<StorableData>(StorableEntity, new_Storable_data);


                        break;

                    //flashlight
                    case 5:


                        slot_local_rot = Quaternion.Inverse(entityManager.GetComponentData<LocalToWorld>(entityManager.GetComponentData<SlotData>(SlotEntity).Storing_entity).Rotation) * entityManager.GetComponentData<LocalToWorld>(SlotEntity).Rotation;
                        slot_local_pos = entityManager.GetComponentData<LocalToWorld>(SlotEntity).Position - entityManager.GetComponentData<LocalToWorld>(entityManager.GetComponentData<SlotData>(SlotEntity).Storing_entity).Position;


                        store_joint = PhysicsJoint.CreateFixed(
                            new RigidTransform(slot_local_rot, (Quaternion)Quaternion.Inverse(entityManager.GetComponentData<LocalToWorld>(entityManager.GetComponentData<SlotData>(SlotEntity).Storing_entity).Rotation) * slot_local_pos),
                            new RigidTransform(Quaternion.identity, float3.zero)

                        );


                        store_joint_componentTypes = new ComponentType[]
                        {
                        typeof(PhysicsConstrainedBodyPair),
                        typeof(PhysicsJoint)
                        };


                        store_joint_constraints.Add(new Constraint
                        {
                            ConstrainedAxes = new bool3(true),//slide,pull,starf
                            Type = ConstraintType.Linear,
                            Min = 0,
                            Max = 0,
                            SpringFrequency = 800000f,//74341.31f,
                            SpringDamping = 2530.126f,
                            MaxImpulse = float.PositiveInfinity,//new float3(math.INFINITY, math.INFINITY, math.INFINITY),
                        });
                        store_joint_constraints.Add(new Constraint
                        {
                            ConstrainedAxes = new bool3(true), // ptich, roll, yaw
                            Type = ConstraintType.Angular,
                            Min = 0,
                            Max = 0,
                            SpringFrequency = 800000f,//74341.31f,
                            SpringDamping = 2530.126f,
                            MaxImpulse = float.PositiveInfinity,//new float3(math.INFINITY, math.INFINITY, math.INFINITY),
                        });

                        store_joint.SetConstraints(store_joint_constraints);

                        joint_entity = entityManager.CreateEntity(store_joint_componentTypes);


                        entityManager.SetComponentData(joint_entity, new PhysicsConstrainedBodyPair(entityManager.GetComponentData<SlotData>(SlotEntity).Storing_entity, StorableEntity, false));
                        entityManager.SetComponentData(joint_entity, store_joint);

                        ecb.AddSharedComponent(joint_entity, new PhysicsWorldIndex());


                        new_Storable_data.store_joint_0 = joint_entity;


                        new_Slot_data = entityManager.GetComponentData<SlotData>(SlotEntity);
                        new_Slot_data.has_to_exit = true;
                        ecb.SetComponent<SlotData>(SlotEntity, new_Slot_data);

                        ecb.SetComponent<StorableData>(StorableEntity, new_Storable_data);

                        break;
                }

                ecb.RemoveComponent<SlotTag>(SlotEntity);
                ecb.RemoveComponent<StorableTag>(StorableEntity);

            }


        }






        triggerJob.storable_entity.Dispose();
        triggerJob.slot_entity.Dispose();

    }
}
