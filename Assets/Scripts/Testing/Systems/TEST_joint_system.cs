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




[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial class TEST_joint_system : SystemBase
{


    private EntityManager entityManager;

    private EntityCommandBuffer ecb;


    protected override void OnCreate()
    {
        base.OnCreate();
        entityManager = World.EntityManager;
    }

    protected override void OnStartRunning()
    {
        //a reactiver pour tester
        /*
        Entity entityA = Entity.Null;
        Entity entityB = Entity.Null;


        Entities
            .WithoutBurst()
            .WithStructuralChanges()
        .ForEach((ref Entity entity,in TEST_joint_data data) =>
        {

            entityA = entity;
            entityB = data.body_to_joint;

        }).Run();

        ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);



            float3 relative_pos = entityManager.GetComponentData<LocalToWorld>(entityB).Position - entityManager.GetComponentData<LocalToWorld>(entityA).Position;
            Quaternion relative_rot = Quaternion.Inverse((Quaternion)entityManager.GetComponentData<LocalToWorld>(entityA).Rotation) * entityManager.GetComponentData<LocalToWorld>(entityB).Rotation;

            //Debug.Log((float3)Vector3.Distance(entityManager.GetComponentData<LocalToWorld>(entityB).Position, entityManager.GetComponentData<LocalToWorld>(entityA).Position));
            //Debug.Log(entityManager.GetComponentData<LocalToWorld>(entityB).Position - entityManager.GetComponentData<LocalToWorld>(entityA).Position);

            //Debug.DrawLine(entityManager.GetComponentData<LocalToWorld>(entityA).Position, entityManager.GetComponentData<LocalToWorld>(entityB).Position, Color.blue, 3f);

            PhysicsJoint snap_joint = PhysicsJoint.CreateFixed(
                new RigidTransform(relative_rot, (Quaternion)Quaternion.Inverse(entityManager.GetComponentData<LocalToWorld>(entityA).Rotation) * relative_pos),
                new RigidTransform(Quaternion.identity, 0)
                );

            //var constraints = snap_joint.GetConstraints();

            ComponentType[] componentTypes =
            {
            typeof(PhysicsConstrainedBodyPair),
            typeof(PhysicsJoint)
            };

            snap_joint.JointType = JointType.Custom;
            snap_joint.SetConstraints(new FixedList512Bytes<Constraint>
            {
                Constraint.BallAndSocket()
                 new Constraint
                {
                    Type = ConstraintType.Linear,
                    ConstrainedAxes = new bool3(true,true,true),
                    SpringDamping = Constraint.DefaultSpringDamping,
                    SpringFrequency = Constraint.DefaultSpringFrequency,
                    Min = 0,
                    Max = 0,
                    MaxImpulse = 0,//new float3(math.INFINITY, math.INFINITY, math.INFINITY),
                    EnableImpulseEvents = false

                }
                ,
                new Constraint
                {
                    Type = ConstraintType.Angular,
                    ConstrainedAxes = new bool3(true,true,true),
                    SpringDamping = Constraint.DefaultSpringDamping,
                    SpringFrequency = Constraint.DefaultSpringFrequency,
                    Min = 0,
                    Max = 0,
                    MaxImpulse = 0,//new float3(math.INFINITY, math.INFINITY, math.INFINITY),
                    EnableImpulseEvents = false
                },


            });

            Entity jointEntity = entityManager.CreateEntity(componentTypes);


            entityManager.SetComponentData(jointEntity, new PhysicsConstrainedBodyPair(entityA, entityB, false));
            entityManager.SetComponentData(jointEntity, snap_joint);

            ecb.AddSharedComponent(jointEntity, new PhysicsWorldIndex());



            //GrabDetectData new_grabdata = entityManager.GetComponentData<GrabDetectData>(entityA);
            //new_grabdata.hand_grab_joint = jointEntity;

            //entityManager.SetComponentData<GrabDetectData>(entityA, new_grabdata);
        

        */
    }



    protected override void OnUpdate()
    {
        

    }


}
