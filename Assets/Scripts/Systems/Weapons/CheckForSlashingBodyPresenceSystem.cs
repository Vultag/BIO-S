
using Unity.Entities;
using UnityEngine;
using Unity.Jobs;
using Unity.Physics;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.UI;
using Unity.Entities.UniversalDelegates;
using UnityEngine.UIElements;
using System.Numerics;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;
using Matrix4x4 = UnityEngine.Matrix4x4;
using static UnityEngine.GraphicsBuffer;
using Unity.VisualScripting;
using Unity.Physics.Extensions;
using Unity.Burst;
using Unity.Collections;
using System;
using Unity.Physics.Systems;
using Unity.Rendering;
using BoxCollider = Unity.Physics.BoxCollider;
using UnityEngine.VFX;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
///change qq chose ? j avais un warning qui dit ca sert a rien
///[UpdateAfter(typeof(PhysicsSimulationGroup))]



public partial class CheckForSlashingBodyPresenceSystem : SystemBase
{


    private EntityManager entityManager;

    private EntityCommandBuffer ecb;

    private float3 katana_lenght_pos = new float3(0, 0.79f, 0);

    private float3 katana_offset_pos = new float3(0.021f,0, 0);

    private float rigidity_multipliyer = 10f;
    private float time_before_rigid = 0.8f;




    protected override void OnCreate()
    {
        entityManager = World.EntityManager;

        RequireForUpdate<CheckForSlashingBodyPresenceData>();
    }

    protected override void OnStartRunning()
    {


    }



    protected override void OnUpdate()
    {

        ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

        PhysicsWorldSingleton phyworld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();




        foreach (var checks_slash_data in SystemAPI.Query<RefRW<CheckForSlashingBodyPresenceData>>())
        {

            time_before_rigid = Mathf.Clamp(time_before_rigid - SystemAPI.Time.DeltaTime, 0f, 1.5f);
            if (time_before_rigid == 0f && checks_slash_data.ValueRO.rigid_state == 0)
                checks_slash_data.ValueRW.rigid_state = 1;
           

            NativeList<DistanceHit> hits = new NativeList<DistanceHit>(Allocator.TempJob);

            var Filter = new CollisionFilter
            {
                BelongsTo = 1,
                CollidesWith = 2,
                GroupIndex = 0,
            };

            Debug.DrawLine(entityManager.GetComponentData<LocalToWorld>(checks_slash_data.ValueRO.slashing_entity).Position + (float3)((Quaternion)entityManager.GetComponentData<LocalToWorld>(checks_slash_data.ValueRO.slashing_entity).Rotation * entityManager.GetComponentData<SlashingItemData>(checks_slash_data.ValueRO.slashing_entity).slash_center), entityManager.GetComponentData<LocalToWorld>(checks_slash_data.ValueRO.slashing_entity).Position + (float3)((Quaternion)entityManager.GetComponentData<LocalToWorld>(checks_slash_data.ValueRO.slashing_entity).Rotation * entityManager.GetComponentData<SlashingItemData>(checks_slash_data.ValueRO.slashing_entity).slash_center) + (float3)((Quaternion)entityManager.GetComponentData<LocalToWorld>(checks_slash_data.ValueRO.slashing_entity).Rotation * new float3(entityManager.GetComponentData<SlashingItemData>(checks_slash_data.ValueRO.slashing_entity).slash_size.x*0.5f, 0, 0)),Color.red);

            Debug.DrawLine(entityManager.GetComponentData<LocalToWorld>(checks_slash_data.ValueRO.slashing_entity).Position + (float3)((Quaternion)entityManager.GetComponentData<LocalToWorld>(checks_slash_data.ValueRO.slashing_entity).Rotation * entityManager.GetComponentData<SlashingItemData>(checks_slash_data.ValueRO.slashing_entity).slash_center), entityManager.GetComponentData<LocalToWorld>(checks_slash_data.ValueRO.slashing_entity).Position + (float3)((Quaternion)entityManager.GetComponentData<LocalToWorld>(checks_slash_data.ValueRO.slashing_entity).Rotation * entityManager.GetComponentData<SlashingItemData>(checks_slash_data.ValueRO.slashing_entity).slash_center) + (float3)((Quaternion)entityManager.GetComponentData<LocalToWorld>(checks_slash_data.ValueRO.slashing_entity).Rotation * new float3(0, entityManager.GetComponentData<SlashingItemData>(checks_slash_data.ValueRO.slashing_entity).slash_size.y*0.5f, 0)), Color.red);

            Debug.DrawLine(entityManager.GetComponentData<LocalToWorld>(checks_slash_data.ValueRO.slashing_entity).Position + (float3)((Quaternion)entityManager.GetComponentData<LocalToWorld>(checks_slash_data.ValueRO.slashing_entity).Rotation * entityManager.GetComponentData<SlashingItemData>(checks_slash_data.ValueRO.slashing_entity).slash_center), entityManager.GetComponentData<LocalToWorld>(checks_slash_data.ValueRO.slashing_entity).Position + (float3)((Quaternion)entityManager.GetComponentData<LocalToWorld>(checks_slash_data.ValueRO.slashing_entity).Rotation * entityManager.GetComponentData<SlashingItemData>(checks_slash_data.ValueRO.slashing_entity).slash_center) + (float3)((Quaternion)entityManager.GetComponentData<LocalToWorld>(checks_slash_data.ValueRO.slashing_entity).Rotation * new float3(0, 0, entityManager.GetComponentData<SlashingItemData>(checks_slash_data.ValueRO.slashing_entity).slash_size.z * 0.5f)), Color.red);

            //Debug.Break();

            if (phyworld.OverlapBox(entityManager.GetComponentData<LocalToWorld>(checks_slash_data.ValueRO.slashing_entity).Position + (float3)((Quaternion)entityManager.GetComponentData<LocalToWorld>(checks_slash_data.ValueRO.slashing_entity).Rotation * entityManager.GetComponentData<SlashingItemData>(checks_slash_data.ValueRO.slashing_entity).slash_center), entityManager.GetComponentData<LocalToWorld>(checks_slash_data.ValueRO.slashing_entity).Rotation, entityManager.GetComponentData<SlashingItemData>(checks_slash_data.ValueRO.slashing_entity).slash_size*0.5f,ref hits,Filter))
            {

                //Debug.Log("inside");

                if (checks_slash_data.ValueRO.rigid_state == 0)
                {

                    PhysicsJoint joint = checks_slash_data.ValueRO.joint;

                    var new_constraints = new FixedList512Bytes<Constraint>();

                    new_constraints = joint.GetConstraints();

                    var new_constraint_1 = new_constraints[1];


                    new_constraint_1.SpringDamping *= (rigidity_multipliyer * -(time_before_rigid - 1.5f));

                    //Debug.Log(-(time_before_rigid - 1.5f));

                    new_constraints[1] = new_constraint_1;

                    joint.SetConstraints(new_constraints);

                    entityManager.SetComponentData<PhysicsJoint>(checks_slash_data.ValueRO.joint_entity, joint);



                }
                else if(checks_slash_data.ValueRO.rigid_state == 1)
                {

      

                    PhysicsJoint joint = checks_slash_data.ValueRO.joint;


                    float3 relative_pos = entityManager.GetComponentData<LocalToWorld>(checks_slash_data.ValueRO.slashed_entity).Position - entityManager.GetComponentData<LocalToWorld>(checks_slash_data.ValueRO.slashing_entity).Position;
                    Quaternion relative_rot = Quaternion.Inverse((Quaternion)entityManager.GetComponentData<LocalToWorld>(checks_slash_data.ValueRO.slashing_entity).Rotation) * entityManager.GetComponentData<LocalToWorld>(checks_slash_data.ValueRO.slashed_entity).Rotation;

                    RigidTransform rigid_A = new RigidTransform(Quaternion.identity, (Quaternion)Quaternion.Inverse(entityManager.GetComponentData<LocalToWorld>(checks_slash_data.ValueRO.slashing_entity).Rotation) * (relative_pos) );
                    RigidTransform rigid_B = new RigidTransform(Quaternion.Inverse(relative_rot), Vector3.zero); //la rot c la rotation du joint ? et la pos c la pos relative a l autre 



                    joint.BodyAFromJoint = rigid_A;
                    joint.BodyBFromJoint = rigid_B;


                    var new_constraints = new FixedList512Bytes<Constraint>();


                    new_constraints.Add(new Constraint
                    {
                        ConstrainedAxes = new bool3(true, false, true),//slide,pull,starf
                        Type = ConstraintType.Linear,
                        Min = 0,
                        Max = 0,
                        SpringFrequency = 800000f,//74341.31f,
                        SpringDamping = 0,//2530.126f,
                        MaxImpulse = float.PositiveInfinity,//new float3(math.INFINITY, math.INFINITY, math.INFINITY),
                    });

                    // SOFT CONSTRAINTS

                    new_constraints.Add(new Constraint
                    {
                        ConstrainedAxes = new bool3(false, true, false),//slide,pull,starf
                        Type = ConstraintType.Linear,
                        Min = 0,
                        Max = 0,
                        SpringFrequency = 0.1f,//74341.31f,
                        SpringDamping = 1200f,//2530.126f,
                        MaxImpulse = float.PositiveInfinity,//new float3(math.INFINITY, math.INFINITY, math.INFINITY),
                    });

                    new_constraints.Add(new Constraint
                    {
                        ConstrainedAxes = new bool3(true, true, true), // ptich, roll, yaw
                        Type = ConstraintType.Angular,
                        Min = 0,
                        Max = 0,
                        SpringFrequency = 800000f,//74341.31f,
                        SpringDamping = 0,//2530.126f,
                        MaxImpulse = float.PositiveInfinity,//new float3(math.INFINITY, math.INFINITY, math.INFINITY),
                    });
                    

                    joint.SetConstraints(new_constraints);

                    entityManager.SetComponentData<PhysicsJoint>(checks_slash_data.ValueRO.joint_entity, joint);



                    checks_slash_data.ValueRW.rigid_state = 2;

                 

                }

                


            }
            else
            {
                
                ///Debug.Log("break");


                ecb.DestroyEntity(checks_slash_data.ValueRO.joint_entity);


                ecb.RemoveComponent<CheckForSlashingBodyPresenceData>(checks_slash_data.ValueRO.slashing_entity);


                ecb.AddComponent<SlashingItemTag>(checks_slash_data.ValueRO.slashing_entity);

                time_before_rigid = 1.5f;

                checks_slash_data.ValueRW.rigid_state = 0;

            }


            //placement du vfx de sang
            ///VFX graph est completement bugger pt etre reessayer a l avenir
            /*
            foreach ((var katana_slash_data, Entity entity) in SystemAPI.Query<RefRO<SlashingItemData>>().WithEntityAccess())
            {

                Entity blood_ps = katana_slash_data.ValueRO.blood_particle;
                Entity blood_vfx = katana_slash_data.ValueRO.splashing_vfx;
                Entity kat_entity = entity;

                var raycastinputs = new RaycastInput
                {
                    Start = entityManager.GetComponentData<LocalToWorld>(entity).Position + (float3)((Quaternion)entityManager.GetComponentData<LocalToWorld>(entity).Rotation * katana_offset_pos),
                    End = entityManager.GetComponentData<LocalToWorld>(entity).Position + (float3)((Quaternion)entityManager.GetComponentData<LocalToWorld>(entity).Rotation * katana_lenght_pos) + (float3)((Quaternion)entityManager.GetComponentData<LocalToWorld>(entity).Rotation * katana_offset_pos),
                    Filter = new CollisionFilter
                    {
                        BelongsTo = 2,
                        CollidesWith = 2,
                        GroupIndex = 0,
                    }
                };

                Vector3 vfx_pos = Vector3.zero;

                if (phyworld.CastRay(raycastinputs, out var hit))
                {
                    vfx_pos = hit.Position;


                    Entities.WithoutBurst()
                    .ForEach((Entity entity, VisualEffect vfx, ref Translation trans) =>
                    {
                        if (entity == blood_vfx)
                        {

                           //Debug.Log(Vector3.Magnitude(entityManager.GetComponentData<PhysicsVelocity>(kat_entity).Linear));
                            //particle.transform.localScale 

                            Matrix4x4 matrix4x4 = entityManager.GetComponentData<LocalToWorld>(kat_entity).Value;

                            vfx.transform.position = matrix4x4.inverse.MultiplyPoint3x4(vfx_pos);

                            //vfx.SetFloat("spawn_rate_multiplicator", Vector3.Magnitude(entityManager.GetComponentData<PhysicsVelocity>(kat_entity).Linear)*2);
                            //vfx.SetFloat("velocity_multiplicator", Vector3.Magnitude(entityManager.GetComponentData<PhysicsVelocity>(kat_entity).Linear)*2);

                            //vfx.Play();
                        }

                    }).Run();

                }



            }

            hits.Dispose();
            
            */
        }


    }

}