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


//[AlwaysSynchronizeSystem]
[UpdateBefore(typeof(FixedStepSimulationSystemGroup))]
//[UpdateAfter(typeof(EndFramePhysicsSystem))]
public partial class GrabOnTriggerSystem : SystemBase
{
    private AvatarFingerSystem Avatarfingersystem;
    private MaterialChangerSystem Materialchangersystem;

    private EntityManager entityManager;

    private EntityCommandBuffer ecb;


    protected override void OnCreate()
    {
        base.OnCreate();
        Avatarfingersystem = World.GetExistingSystemManaged<AvatarFingerSystem>();
        Materialchangersystem = World.GetExistingSystemManaged<MaterialChangerSystem>();
        entityManager = World.EntityManager;

    }





    // [BurstCompile]
    struct GrabOnTriggerSystemJo : ITriggerEventsJobBase
    {
        [ReadOnly] public ComponentLookup<GrabbableTag> allGrabbable;
        [ReadOnly] public ComponentLookup<GrabDetectTag> allGrabDetect;


        public NativeReference<Entity> item_entity;
        public NativeReference<Entity> detect_entity;

        public void Execute(TriggerEvent triggerEvent)
        {

            
            Entity entityA = triggerEvent.EntityA;
            Entity entityB = triggerEvent.EntityB;



            if (allGrabDetect.HasComponent(entityA) && allGrabDetect.HasComponent(entityB))
            {
                Debug.Log("test_0");
                return;

            }



            if (allGrabbable.HasComponent(entityB) && allGrabDetect.HasComponent(entityA))
            {
                item_entity.Value = entityB;

                detect_entity.Value = entityA;


            }
            else if (allGrabbable.HasComponent(entityA) && allGrabDetect.HasComponent(entityB))
            {
                
                item_entity.Value = entityA;
                
                detect_entity.Value = entityB;

            }
            


        }
      

    }
    


    protected override void OnUpdate()
    {

        GrabOnTriggerSystemJo triggerJob = new GrabOnTriggerSystemJo()
        {
            allGrabbable = GetComponentLookup<GrabbableTag>(true),
            allGrabDetect = GetComponentLookup<GrabDetectTag>(true),
            detect_entity = new NativeReference<Entity>(Allocator.TempJob),
            item_entity = new NativeReference<Entity>(Allocator.TempJob)
        };


        Dependency = triggerJob.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), Dependency);

        Dependency.Complete();


        if (entityManager.Exists(triggerJob.detect_entity.Value))
        {

            // A METTRE DANS UN JOB

            ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            //Debug.Log("grab");


            ecb.RemoveComponent<GrabDetectTag>(triggerJob.detect_entity.Value);



            Entity entityA = triggerJob.detect_entity.Value;
            Entity entityB = triggerJob.item_entity.Value;

            ///SORT LES ITEMS DES LINKED GROUP DE ROOM QUI SONT DESTROYED
            /// TRES MAL OPTI MAIS PAS TROUVE MIEUX
            {

                var item_child_buffer = SystemAPI.GetBuffer<Child>(entityB);

                var item_child_Array = item_child_buffer.AsNativeArray();


                if (HasBuffer<LinkedEntityGroup>(SystemAPI.GetSingleton<RoomManagerData>().room_0_instance))
                {
                    var instance_0_buffer = SystemAPI.GetBuffer<LinkedEntityGroup>(SystemAPI.GetSingleton<RoomManagerData>().room_0_instance);
                    var instance_0_buffer_Array = instance_0_buffer.AsNativeArray();



                    for (var i = 0; i < instance_0_buffer_Array.Length;)
                    {

                        bool element_removed = false;

                        //essaie fail de fix le fait de traverser les buffer de room dans deletes les parties de pistol
                        /*
                        if (instance_0_buffer_Array[i].Value.Index == entityManager.GetComponentData<WeaponSliderData>(entityManager.GetComponentData<FirearmData>(entityB).slider_entity).motor_entity.Index)
                        {
                            Debug.Log("test2");
                            instance_0_buffer.RemoveAt(i);
                            element_removed = true;
                        }
                        
                        if (instance_0_buffer_Array[i].Value.Index == entityManager.GetComponentData<FirearmData>(entityB).slider_entity.Index)
                        {
                            Debug.Log("test3");
                            instance_0_buffer.RemoveAt(i);
                            element_removed = true;
                        }
                        */
                        

                        if (instance_0_buffer_Array[i].Value.Index == entityB.Index)
                        {

                            instance_0_buffer.RemoveAt(i);
                            element_removed = true;

                        }

                        foreach (var item_child in item_child_Array)
                        {
    
                            if (instance_0_buffer_Array[i].Value == item_child.Value)
                            {
                                instance_0_buffer.RemoveAt(i);
                                element_removed = true;

                            }

                        }
                        if (!element_removed)
                            i++;
                    }

                    instance_0_buffer_Array.Dispose();

                }
                if (HasBuffer<LinkedEntityGroup>(SystemAPI.GetSingleton<RoomManagerData>().room_1_2_instance))
                {
                    var instance_1_2_buffer = SystemAPI.GetBuffer<LinkedEntityGroup>(SystemAPI.GetSingleton<RoomManagerData>().room_1_2_instance);
                    var instance_1_2_buffer_Array = instance_1_2_buffer.AsNativeArray();

                    for (var i = 0; i < instance_1_2_buffer_Array.Length;)
                    {
                        bool element_removed = false;

                        if (instance_1_2_buffer_Array[i].Value.Index == entityB.Index)
                        {

                            instance_1_2_buffer.RemoveAt(i);
                            element_removed = true;

                        }


                        foreach (var item_child in item_child_Array)
                        {

                            if (instance_1_2_buffer_Array[i].Value == item_child.Value)
                            {
          
                                instance_1_2_buffer.RemoveAt(i);
                                element_removed = true;

                            }

                        }
                        if (!element_removed)
                            i++;
                    }

                    instance_1_2_buffer_Array.Dispose();
                }
                if (HasBuffer<LinkedEntityGroup>(SystemAPI.GetSingleton<RoomManagerData>().room_3_instance))
                {
                    var instance_3_buffer = SystemAPI.GetBuffer<LinkedEntityGroup>(SystemAPI.GetSingleton<RoomManagerData>().room_3_instance);
                    var instance_3_buffer_Array = instance_3_buffer.AsNativeArray();

                    for (var i = 0; i < instance_3_buffer_Array.Length;)
                    {

                        bool element_removed = false;

                        if (instance_3_buffer_Array[i].Value.Index == entityB.Index)
                        {

                            instance_3_buffer.RemoveAt(i);
                            element_removed = true;

                        }

                        foreach (var item_child in item_child_Array)
                        {

                            if (instance_3_buffer_Array[i].Value == item_child.Value)
                            {

                                instance_3_buffer.RemoveAt(i);
                                element_removed = true;

                            }

                        }
                        if (!element_removed)
                            i++;
                    }

                    instance_3_buffer_Array.Dispose();

                }


                item_child_Array.Dispose();

            }

            var grabdetect_from_entity = GetComponentLookup<GrabDetectData>(false);
            var grabbable_from_entity = GetComponentLookup<ItemData>(false);

            Debug.Log("test1");

            //check if the item is stored and if so, break the store join ect...
            if (entityManager.HasComponent<StorableData>(entityB))
            {
                Debug.Log("test2");
                if (entityManager.GetComponentData<StorableData>(entityB).storing_entity != Entity.Null)
                {
                    Debug.Log("test3");
                    HasToExitData exit_data = new HasToExitData();
                    exit_data.physics_layer = 4;
                    exit_data.shape_size = entityManager.GetComponentData<SlotData>(entityManager.GetComponentData<StorableData>(entityB).storing_entity).shape_size;
                    exit_data.shape_entity = entityManager.GetComponentData<StorableData>(entityB).storing_entity;

                    ecb.AddComponent<HasToExitData>(entityManager.GetComponentData<StorableData>(entityB).storing_entity, exit_data);

                    StorableData new_Storable_data = new StorableData();
                    new_Storable_data.storing_entity = Entity.Null;
                    ecb.SetComponent<StorableData>(entityB, new_Storable_data);
                    ecb.DestroyEntity(entityManager.GetComponentData<StorableData>(entityB).store_joint_0);
                    if(entityManager.GetComponentData<StorableData>(entityB).store_joint_1 != Entity.Null)
                        ecb.DestroyEntity(entityManager.GetComponentData<StorableData>(entityB).store_joint_1);
                    ecb.AddComponent<SlotTag>(entityManager.GetComponentData<StorableData>(entityB).storing_entity);
                    ecb.AddComponent<StorableTag>(entityB);
                }
            }




            float3 relative_pos = entityManager.GetComponentData<LocalToWorld>(entityB).Position - entityManager.GetComponentData<LocalToWorld>(entityA).Position;
            Quaternion relative_rot = Quaternion.Inverse((Quaternion)entityManager.GetComponentData<LocalToWorld>(entityA).Rotation) * entityManager.GetComponentData<LocalToWorld>(entityB).Rotation;


            PhysicsJoint snap_joint;
            // faire un switch case pour chaque nv item ?
            if (entityManager.GetComponentData<ItemData>(entityB).snap_type == 0)
            {
                snap_joint = PhysicsJoint.CreateFixed(
                new RigidTransform(relative_rot, (Quaternion)Quaternion.Inverse(entityManager.GetComponentData<LocalToWorld>(entityA).Rotation) * relative_pos),
                new RigidTransform(Quaternion.identity, float3.zero)
                );

            }
            else
            {
                if (entityManager.HasComponent<RightHandTag>(entityA))
                {

                    relative_pos = new Vector3(-entityManager.GetComponentData<ItemData>(entityB).grab_pos_offset.x, entityManager.GetComponentData<ItemData>(entityB).grab_pos_offset.y, entityManager.GetComponentData<ItemData>(entityB).grab_pos_offset.z);
                    relative_rot = Quaternion.Euler(new Vector3(entityManager.GetComponentData<ItemData>(entityB).grab_rot_offset.x, -entityManager.GetComponentData<ItemData>(entityB).grab_rot_offset.y, -entityManager.GetComponentData<ItemData>(entityB).grab_rot_offset.z));//new Vector3( ici pour inverser un axe ou 2 ?

                }
                else
                {
                    relative_pos = entityManager.GetComponentData<ItemData>(entityB).grab_pos_offset;
                    relative_rot = Quaternion.Euler(entityManager.GetComponentData<ItemData>(entityB).grab_rot_offset);
                }

                snap_joint = PhysicsJoint.CreateFixed(
                new RigidTransform(relative_rot, relative_pos),
                new RigidTransform(Quaternion.identity, float3.zero)
                );


                Materialchangersystem.change_material(0, entityManager.HasComponent<RightHandTag>(entityA) ? 0 : 1);
                
                Entities.WithoutBurst()
                .ForEach((Entity entity, ref AvatarFingerData fingerdata) =>
                {


                    if(entityManager.HasComponent<RightHandTag>(entityA))
                    {
                        if (entityManager.HasComponent<RightHandTag>(entity))
                        {

                            ecb.AddComponent(fingerdata.Skin_finger_entity, new Parent { Value = fingerdata.fantome_correspondant });

                            ecb.AddComponent<Disabled>(entity);


                        }

                    }
                    if (entityManager.HasComponent<LeftHandTag>(entityA))
                    {


                        if (entityManager.HasComponent<LeftHandTag>(entity))
                        {

                            //je recupere l entity_tracked pour le skin et je le depalce sur le doigt fantome

                            ecb.AddComponent(fingerdata.Skin_finger_entity, new Parent { Value = fingerdata.fantome_correspondant });
            
                            ecb.AddComponent<Disabled>(entity);

                        }



                    }


                }).Run();
                


            }

            ComponentType[] componentTypes =
            {
            typeof(PhysicsConstrainedBodyPair),
            typeof(PhysicsJoint)
            };
            
            
            var constraints = new FixedList512Bytes<Constraint>();

            constraints.Add(new Constraint
            {
                ConstrainedAxes = new bool3(true, true, true),
                Type = ConstraintType.Linear,
                Min = 0,
                Max = 0,
                SpringFrequency = 8000f,//74341.31f,
                SpringDamping = 0,//2530.126f,
                MaxImpulse = float.PositiveInfinity,//new float3(math.INFINITY, math.INFINITY, math.INFINITY),
            });
            constraints.Add(new Constraint
            {
                ConstrainedAxes = new bool3(true, true, true),
                Type = ConstraintType.Angular,
                Min = 0,
                Max = 0,
                SpringFrequency = 8000f,//74341.31f,
                SpringDamping = 0,//2530.126f,
                MaxImpulse = float.PositiveInfinity,//new float3(math.INFINITY, math.INFINITY, math.INFINITY),
            });
            
            snap_joint.SetConstraints(constraints);
            
            
            Entity jointEntity = entityManager.CreateEntity(componentTypes);


            entityManager.SetComponentData(jointEntity, new PhysicsConstrainedBodyPair(entityA, entityB, false));
            entityManager.SetComponentData(jointEntity, snap_joint);



            ecb.AddSharedComponent(jointEntity, new PhysicsWorldIndex());
           

            if (entityManager.HasComponent<RightHandTag>(entityA))
            {
                var new_grabdetect = grabdetect_from_entity[entityA];
                new_grabdetect.grabbing = true;
                new_grabdetect.hand_grab_joint = jointEntity;
                new_grabdetect.grabbed_item = entityB;
                new_grabdetect.snap_type = entityManager.GetComponentData<ItemData>(entityB).snap_type;

                grabdetect_from_entity[entityA] = new_grabdetect;

                var new_grabable = grabbable_from_entity[entityB];
                new_grabable.grabbed = true;

                if (entityManager.GetComponentData<ItemData>(entityB).hand_grabbing < 1)
                    new_grabable.hand_grabbing = 1;
                else
                    new_grabable.hand_grabbing = 3;

                grabbable_from_entity[entityB] = new_grabable;


            }
            // donc c est la main gauche
            else
            {

                var new_grabdetect = grabdetect_from_entity[entityA];
                new_grabdetect.grabbing = true;
                new_grabdetect.hand_grab_joint = jointEntity;
                new_grabdetect.grabbed_item = entityB;
                new_grabdetect.snap_type = entityManager.GetComponentData<ItemData>(entityB).snap_type;


                grabdetect_from_entity[entityA] = new_grabdetect;

                var new_grabable = grabbable_from_entity[entityB];
                new_grabable.grabbed = true;
                if (entityManager.GetComponentData<ItemData>(entityB).hand_grabbing < 1)
                    new_grabable.hand_grabbing = 2;
                else
                    new_grabable.hand_grabbing = 3;


                grabbable_from_entity[entityB] = new_grabable;


            }

            PhysicsMass new_mass = entityManager.GetComponentData<PhysicsMass>(entityA);


        }


        triggerJob.detect_entity.Dispose();
        triggerJob.item_entity.Dispose();


    }


}