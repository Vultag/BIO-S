
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
[UpdateAfter(typeof(FixedStepSimulationSystemGroup))]
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


                if(mag_data.bullets_in_clip > 0)
                {
                    
                    //inverser le moteur mais super chiant a faire

                    /*
                    float3 axisInB = math.normalize(Vector3.right);

                    RigidTransform aFromB = math.mul(math.inverse(Math.DecomposeRigidBodyTransform(entityManager.GetComponentData<LocalToWorld>(mag_attach_data.firearm_entity).Value)), Math.DecomposeRigidBodyTransform(entityManager.GetComponentData<LocalToWorld>(mag_attach_data.firearm_entity).Value));
                    float3 axisInA = math.mul(aFromB.rot, axisInB); //motor axis relative to bodyA

                    RigidTransform bFromA = math.mul(math.inverse(Math.DecomposeRigidBodyTransform(entityManager.GetComponentData<LocalToWorld>(mag_attach_data.firearm_entity).Value)), Math.DecomposeRigidBodyTransform(entityManager.GetComponentData<LocalToWorld>(mag_attach_data.firearm_entity).Value));
                    float3 PositionInConnectedEntity = math.transform(bFromA, new Vector3(0, 0.079f, 0.0870292f)); //position of motored body relative to Connected Entity in world space
                    float3 AxisInConnectedEntity = axisInB; //motor axis in Connected Entity space

                    // Always calculate the perpendicular axes
                    Math.CalculatePerpendicularNormalized(axisInA, out var perpendicularAxisLocal, out _);
                    float3 PerpendicularAxisInConnectedEntity = math.mul(bFromA.rot, perpendicularAxisLocal); //perp motor axis in Connected Entity space


                    ComponentType[] componentTypes =
                        {
                        typeof(PhysicsConstrainedBodyPair),
                        typeof(PhysicsJoint)
                        };

                    var motor = PhysicsJoint.CreateLinearVelocityMotor(
                    new BodyFrame
                    {
                        Axis = axisInA,
                        PerpendicularAxis = perpendicularAxisLocal,
                        Position = PositionInConnectedEntity
                    },
                    new BodyFrame
                    {
                        Axis = AxisInConnectedEntity,
                        PerpendicularAxis = PerpendicularAxisInConnectedEntity,
                        Position = PositionInConnectedEntity
                    },
                    10,
                    2f
                    );


                    //motor.SetImpulseEventThresholdAllConstraints(0.2f);

                    Entity motorEntity = entityManager.CreateEntity(componentTypes);


                    entityManager.SetComponentData(motorEntity, new PhysicsConstrainedBodyPair(mag_attach_data.firearm_entity, mag_attach_data.mag_entity, false));
                    entityManager.SetComponentData(motorEntity, motor);

                    ecb.AddSharedComponent(motorEntity, new PhysicsWorldIndex());
                    */


                }

                ecb.SetComponent<FirearmData>(mag_attach_data.firearm_entity, new_firearm_data);

                ecb.DestroyEntity(mag_attach_data.mag_joint_entity);

                ecb.RemoveComponent<GrabbableTag>(mag_attach_data.mag_joint_entity);

                loaded_joint(entity, mag_attach_data.firearm_entity);

                ecb.RemoveComponent<MagazineAttachmentData>(mag_attach_data.mag_entity);

            }


            else if (Vector3.Distance(gun_pos,mag_pos)>0.1503f)
            {

                //Debug.Log("mag break");
                ecb.DestroyEntity(entityManager.GetComponentData<MagazineData>(entity).disable_col_joint);
                ecb.DestroyEntity(mag_attach_data.mag_joint_entity);
                ecb.AddComponent<MagazineTag>(mag_attach_data.mag_entity);
                ecb.AddComponent<MagazineChamberTag>(mag_attach_data.firearm_entity);
                ecb.RemoveComponent<MagazineAttachmentData>(mag_attach_data.mag_entity);
                ecb.AddComponent<StorableTag>(mag_attach_data.mag_entity);
            }


        }).Run();
    }

    public void loaded_joint(Entity body_a, Entity body_b)
    {


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