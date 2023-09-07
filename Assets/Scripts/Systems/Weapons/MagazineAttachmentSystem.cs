
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

//[AlwaysSynchronizeSystem]
///POUR TRANSFORM V1
//[UpdateAfter(typeof(SimulationSystemGroup))]
/////[UpdateAfter(typeof(FixedStepSimulationSystemGroup))]
//[UpdateInGroup(typeof(BeforePhysicsSystemGroup))]
//[UpdateAfter(typeof(FixedStepSimulationSystemGroup))]
public partial class MagazineAttachmentSystem : SystemBase
{

    private XRInputsActionSystem XRInputSystem;

    private EntityManager entityManager;

    private EntityCommandBuffer ecb;


    protected override void OnCreate()
    {

        RequireForUpdate<MagazineAttachmentData>();
        entityManager = World.EntityManager;

        XRInputSystem = World.GetExistingSystemManaged<XRInputsActionSystem>();
    }

    protected override void OnUpdate()
    {

        ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

        Entities
            .WithoutBurst()
        .ForEach((Entity entity,ref MagazineData mag_data, in MagazineAttachmentData mag_attach_data) =>
        {
            Quaternion gun_rot = entityManager.GetComponentData<LocalToWorld>(mag_attach_data.firearm_entity).Rotation;
            Quaternion mag_rot = entityManager.GetComponentData<LocalToWorld>(mag_attach_data.mag_entity).Rotation;
            float3 gun_pos = entityManager.GetComponentData<LocalToWorld>(mag_attach_data.firearm_entity).Position;
            float3 mag_pos = entityManager.GetComponentData<LocalToWorld>(mag_attach_data.mag_entity).Position;


            //Debug.Log(gun_pos);
            //Debug.Log(mag_pos);

            if (math.transform(math.inverse(entityManager.GetComponentData<LocalToWorld>(mag_attach_data.firearm_entity).Value), entityManager.GetComponentData<LocalToWorld>(mag_attach_data.mag_entity).Position).y > -0.01f)
            {
                //Debug.Log("mag loaded");

                FirearmData new_firearm_data;

                new_firearm_data = entityManager.GetComponentData<FirearmData>(mag_attach_data.firearm_entity);

                new_firearm_data.loaded_magazine = entity;

                Entity grabing_hand = Entity.Null;



                if (entityManager.GetComponentData<ItemData>(entity).hand_grabbing == 1)
                {
                    grabing_hand = GetEntityQuery(typeof(RightHandTag),typeof(GrabDetectData)).ToEntityArray(Allocator.TempJob)[0];
                    XRInputSystem.drop_item(grabing_hand);
                }
                else if (entityManager.GetComponentData<ItemData>(entity).hand_grabbing == 2)
                {
                    grabing_hand = GetEntityQuery(typeof(LeftHandTag), typeof(GrabDetectData)).ToEntityArray(Allocator.TempJob)[0];
                    XRInputSystem.drop_item(grabing_hand);
                }




                //pas la ou sa devrait etre mais pas de mechanique de chamber pour l instant
                if (entityManager.HasComponent<Disabled>(entityManager.GetComponentData<FirearmData>(mag_attach_data.firearm_entity).chamber_bullet) && mag_data.bullets_in_clip>0)
                    ecb.RemoveComponent<Disabled>(entityManager.GetComponentData<FirearmData>(mag_attach_data.firearm_entity).chamber_bullet);



                ecb.SetComponent<FirearmData>(mag_attach_data.firearm_entity, new_firearm_data);

                ecb.DestroyEntity(mag_attach_data.mag_joint_entity);

                ecb.RemoveComponent<GrabbableTag>(mag_attach_data.mag_joint_entity);

                loaded_joint(entity, mag_attach_data.firearm_entity);

                ecb.RemoveComponent<MagazineAttachmentData>(mag_attach_data.mag_entity);

            }


            else if (Vector3.Distance(gun_pos,mag_pos)>0.1503f)
            {

                Debug.Log("mag break");
                ecb.DestroyEntity(entityManager.GetComponentData<MagazineData>(entity).disable_col_joint);
                ecb.DestroyEntity(mag_attach_data.mag_joint_entity);
                ecb.AddComponent<MagazineTag>(mag_attach_data.mag_entity);
                ecb.AddComponent<MagazineChamberTag>(mag_attach_data.firearm_entity);
                ecb.RemoveComponent<MagazineAttachmentData>(mag_attach_data.mag_entity);
                ecb.AddComponent<StorableTag>(mag_attach_data.mag_entity);
                ecb.AddComponent<StorableData>(mag_attach_data.mag_entity);
            }


        }).Run();
    }

    public void loaded_joint(Entity body_a, Entity body_b)
    {


        if (entityManager.GetComponentData<MagazineData>(body_a).bullets_in_clip > 0)
        {
            WeaponSliderData slider_data = entityManager.GetComponentData<WeaponSliderData>(entityManager.GetComponentData<FirearmData>(body_b).slider_entity);

            var new_motor = PhysicsJoint.CreateLinearVelocityMotor(
                   slider_data.Afromjoint,
                   slider_data.Bfromjoint,
                   slider_data.motor_target_speed,
                   slider_data.motor_max_impulse_applided
               );

            //Debug.Log(slider_data.motor_target_speed);

            ecb.SetComponent<PhysicsJoint>(slider_data.motor_entity, new_motor);
        }


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

        Entity jointEntity = entityManager.CreateEntity(componentTypes);

        entityManager.SetComponentData(jointEntity, new PhysicsConstrainedBodyPair(body_a, body_b, false));
        entityManager.SetComponentData(jointEntity, snap_joint);

        ecb.AddSharedComponent(jointEntity, new PhysicsWorldIndex());

        MagazineData new_mag_data = new MagazineData();

        new_mag_data = entityManager.GetComponentData<MagazineData>(body_a);

        new_mag_data.loaded_mag_joint = jointEntity;

        ecb.SetComponent<MagazineData>(body_a, new_mag_data);
    }


}