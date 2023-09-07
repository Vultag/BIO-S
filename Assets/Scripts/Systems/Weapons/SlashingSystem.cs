
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
using UnityEditor;
using UnityEngine.VFX;
using Matrix4x4 = UnityEngine.Matrix4x4;


//[AlwaysSynchronizeSystem]
//[UpdateInGroup(typeof(FixedStepSimulationSystemGroup), OrderLast = true)]
///change qq chose ? j avais un warning qui dit ca sert a rien
///[UpdateAfter(typeof(PhysicsSimulationGroup))]

public partial class SlashingSystem : SystemBase
{
    private BuildPhysicsWorld buildPhysicsWorld;
    //private StepPhysicsWorld stepPhysicsWorld;
    private AvatarFingerSystem Avatarfingersystem;
    private MaterialChangerSystem Materialchangersystem;

    private EntityManager entityManager;

    private EntityCommandBuffer ecb;
    private EntityCommandBuffer ecbBS;


    protected override void OnCreate()
    {
        base.OnCreate();
        //buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        Avatarfingersystem = World.GetExistingSystemManaged<AvatarFingerSystem>();
        Materialchangersystem = World.GetExistingSystemManaged<MaterialChangerSystem>();
        entityManager = World.EntityManager;
        //commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

    }



    // [BurstCompile]
    struct SlashingSystemJob : ICollisionEventsJobBase
    {
        [ReadOnly] public ComponentLookup<ZombiePartTag> all_zombie_part;
        [ReadOnly] public ComponentLookup<SlashingItemTag> all_can_slash;

        [ReadOnly] public PhysicsWorld PhysicsWorld;

        public NativeReference<Entity> zombie_entity;
        public NativeReference<Entity> item_entity;

        public NativeReference<float3> collision_point;
        public NativeReference<float3> collision_impulse;

        public void Execute(CollisionEvent collisionEvent)
        {

            Entity entityA = collisionEvent.EntityA;
            Entity entityB = collisionEvent.EntityB;


            if (all_zombie_part.HasComponent(entityA) && all_zombie_part.HasComponent(entityB))
            {
                Debug.Log("test_0");
                return;
            }


            if (all_zombie_part.HasComponent(entityA) && all_can_slash.HasComponent(entityB))
            {
                //Debug.Log("HIT");
                zombie_entity.Value = entityA;
                item_entity.Value = entityB;

                collision_point.Value = collisionEvent.CalculateDetails(ref PhysicsWorld).EstimatedContactPointPositions[0];
                collision_impulse.Value = collisionEvent.CalculateDetails(ref PhysicsWorld).EstimatedImpulse;

                //Debug.DrawLine(collisionEvent.CalculateDetails(ref PhysicsWorld).EstimatedContactPointPositions[0], (Vector3)float3.zero,Color.red,5f);

            }
            else if (all_zombie_part.HasComponent(entityB) && all_can_slash.HasComponent(entityA))
            {

                //Debug.Log("HIT");
                zombie_entity.Value = entityB;
                item_entity.Value = entityA;

                collision_point.Value = collisionEvent.CalculateDetails(ref PhysicsWorld).EstimatedContactPointPositions[0];
                collision_impulse.Value = collisionEvent.CalculateDetails(ref PhysicsWorld).EstimatedImpulse;

                //Debug.DrawLine(collisionEvent.CalculateDetails(ref PhysicsWorld).EstimatedContactPointPositions[0], (Vector3)float3.zero, Color.red, 5f);

            }
        }

    }






    protected override void OnUpdate()
    {

        SlashingSystemJob triggerJob = new SlashingSystemJob()
        {
            all_zombie_part = GetComponentLookup<ZombiePartTag>(true),
            all_can_slash = GetComponentLookup<SlashingItemTag>(true),
            PhysicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld,
            item_entity = new NativeReference<Entity>(Allocator.TempJob),
            zombie_entity = new NativeReference<Entity>(Allocator.TempJob),
            collision_point = new NativeReference<float3>(Allocator.TempJob),
            collision_impulse = new NativeReference<float3>(Allocator.TempJob)
        };


        Dependency = triggerJob.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), Dependency);


        Dependency.Complete();

        //Debug.Log(triggerJob.detect_entity.Value);


        if (entityManager.Exists(triggerJob.zombie_entity.Value))
        {

            // A METTRE DANS UN JOB

            foreach ((var katana_slash_data, Entity entity) in SystemAPI.Query<RefRO<SlashingItemData>>().WithEntityAccess())
            {

                Entity blood_ps = katana_slash_data.ValueRO.blood_particle;
                Entity blood_vfx = katana_slash_data.ValueRO.splashing_vfx;
                Entity kat_entity = entity;

                Entities.WithoutBurst()
               .ForEach((Entity entity, ParticleSystem particle, ref Translation trans) =>
               {
                   if (entity == blood_ps)
                   {

                       Matrix4x4 matrix4x4 = entityManager.GetComponentData<LocalToWorld>(kat_entity).Value;

                       trans.Value = matrix4x4.inverse.MultiplyPoint3x4(triggerJob.collision_point.Value);
                       particle.Play();
                   }

               }).Run();

                ///VFX graph est completement bugger pt etre reessayer a l avenir
                /*
                Entities.WithoutBurst()
                .ForEach((Entity entity, VisualEffect vfx, ref Translation trans) =>
                {
                    if (entity == blood_vfx)
                    {
                        Debug.Log("test");

                        //particle.transform.localScale 

                        Matrix4x4 matrix4x4 = entityManager.GetComponentData<LocalToWorld>(kat_entity).Value;

                        trans.Value = matrix4x4.inverse.MultiplyPoint3x4(triggerJob.collision_point.Value);
                        vfx.Play();
                    }

                }).Run();
                */

            }


            ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            ecbBS = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);


            //ecb.RemoveComponent<SlashingItemTag>(triggerJob.item_entity.Value);
            entityManager.RemoveComponent<SlashingItemTag>(triggerJob.item_entity.Value);


            Entity entityA = triggerJob.item_entity.Value;
            Entity entityB = triggerJob.zombie_entity.Value;

            Entity zombie_target = entityManager.GetComponentData<ZombieBalanceData>(entityManager.GetComponentData<ZombieBrainData>(entityManager.GetComponentData<ZombiePartData>(entityB).zombie_brain_entity).propeled_bodypart).zombie_target;

            if (!entityManager.HasComponent<PlayerHeadTag>(zombie_target))
            {

                Player_detected(entityManager.GetComponentData<ZombieBrainData>(entityManager.GetComponentData<ZombiePartData>(entityB).zombie_brain_entity).zombie_detect);

            }

            _deal_damage(entityB, ((Vector3)triggerJob.collision_impulse.Value).magnitude);


            PhysicsVelocity new_vel = new PhysicsVelocity();

            entityManager.SetComponentData<PhysicsVelocity>(entityB, new_vel);

            CheckForSlashingBodyPresenceData slashing_check = new CheckForSlashingBodyPresenceData();

            float3 relative_pos = entityManager.GetComponentData<LocalToWorld>(entityB).Position - entityManager.GetComponentData<LocalToWorld>(entityA).Position;
            Quaternion relative_rot = Quaternion.Inverse((Quaternion)entityManager.GetComponentData<LocalToWorld>(entityA).Rotation) * entityManager.GetComponentData<LocalToWorld>(entityB).Rotation;


            PhysicsJoint snap_joint;

            snap_joint = PhysicsJoint.CreateFixed(
            new RigidTransform(Quaternion.identity, (Quaternion)Quaternion.Inverse(entityManager.GetComponentData<LocalToWorld>(entityA).Rotation) * (relative_pos) + ((Quaternion)Quaternion.Inverse(entityManager.GetComponentData<LocalToWorld>(entityA).Rotation) * (Vector3)(triggerJob.collision_point.Value - entityManager.GetComponentData<LocalToWorld>(entityB).Position))),
            new RigidTransform(Quaternion.Inverse(relative_rot), Quaternion.Inverse(entityManager.GetComponentData<LocalToWorld>(entityB).Rotation) * (triggerJob.collision_point.Value - entityManager.GetComponentData<LocalToWorld>(entityB).Position)) //la rot c la rotation du joint ? et la pos c la pos relative a l autre 
            );




            ComponentType[] componentTypes =
            {
            typeof(PhysicsConstrainedBodyPair),
            typeof(PhysicsJoint)
            };

            var constraints = new FixedList512Bytes<Constraint>();

            constraints.Add(new Constraint
            {
                ConstrainedAxes = new bool3(false, false, true),//slide,pull,starf
                Type = ConstraintType.Linear,
                Min = 0,
                Max = 0,
                SpringFrequency = 800000f,//74341.31f,
                SpringDamping = 0,//2530.126f,
                MaxImpulse = float.PositiveInfinity,//new float3(math.INFINITY, math.INFINITY, math.INFINITY),
            });

            // SOFT CONSTRAINTS

            constraints.Add(new Constraint
            {
                ConstrainedAxes = new bool3(true, true, false),//slide,pull,starf
                Type = ConstraintType.Linear,
                Min = 0,
                Max = 0,
                SpringFrequency = 0.01f,//0.05f,
                SpringDamping = 1200f,//2530.126f,
                MaxImpulse = float.PositiveInfinity,//new float3(math.INFINITY, math.INFINITY, math.INFINITY),
            });

            // SOFT CONSTRAINTS
            constraints.Add(new Constraint
            {
                ConstrainedAxes = new bool3(true, true, true), // ptich, roll, yaw
                Type = ConstraintType.Angular,
                Min = 0,
                Max = 0,
                SpringFrequency = 0.01f,//74341.31f,
                SpringDamping = 120f,//2530.126f,
                MaxImpulse = float.PositiveInfinity,//new float3(math.INFINITY, math.INFINITY, math.INFINITY),
            });

            snap_joint.SetConstraints(constraints);

            Entity jointEntity = entityManager.CreateEntity(componentTypes);

            entityManager.SetComponentData(jointEntity, new PhysicsConstrainedBodyPair(entityA, entityB, false));
            entityManager.SetComponentData(jointEntity, snap_joint);

            ecb.AddSharedComponent(jointEntity, new PhysicsWorldIndex());


            slashing_check.joint = snap_joint;

            slashing_check.joint_entity = jointEntity;

            slashing_check.slashing_entity = entityA;

            slashing_check.slashed_entity = entityB;

            ecb.AddComponent<CheckForSlashingBodyPresenceData>(entityA, slashing_check);



            triggerJob.zombie_entity.Dispose();
            triggerJob.item_entity.Dispose();
            triggerJob.collision_point.Dispose();
            triggerJob.collision_impulse.Dispose();


        }

    }

    private void _deal_damage(Entity zombie_part, float impulse)
    {

        //Debug.Log(zombie_part.Index);

        float damage = impulse * 0.01f;

        ///PROGRESSIVE DYING REACTIVATE ?
        /*
        if (entityManager.HasComponent<IKBodyPartData>(zombie_part))
        {

            //Debug.Log(entityManager.GetComponentData<IKBodyPartData>(zombie_part).IK_master_entity);

            //new_ik_master_data = entityManager.GetComponentData<IKMasterBodyPartData>(entityManager.GetComponentData<IKBodyPartData>(zombie_part).IK_master_entity);

            //new_ik_master_data.full_limb_hp_ratio = Mathf.Clamp(new_ik_master_data.full_limb_hp_ratio - damage*2f,0,1f);

            //Debug.Log(new_ik_master_data.full_limb_hp_ratio);

            //ecb.SetComponent<IKMasterBodyPartData>(entityManager.GetComponentData<IKBodyPartData>(zombie_part).IK_master_entity, new_ik_master_data);

            new_localized_Ik_dmg += damage;



        }
        else if (entityManager.HasComponent<ZombieBalanceData>(zombie_part))
        {


            ZombieBalanceData new_zombie_balance_data;

            new_zombie_balance_data = entityManager.GetComponentData<ZombieBalanceData>(zombie_part);

            new_zombie_balance_data.limb_hp_ratio = Mathf.Clamp(new_zombie_balance_data.limb_hp_ratio - damage, 0, 1f);

            //Debug.Log(new_zombie_balance_data.limb_hp_ratio);

            ecb.SetComponent<ZombieBalanceData>(zombie_part, new_zombie_balance_data);

        }
        */




        foreach (ZombieBrainElementData body_part in entityManager.GetBuffer<ZombieBrainElementData>(entityManager.GetComponentData<ZombiePartData>(zombie_part).zombie_brain_entity))
        {

            float localized_Ik_factor = 1f;


            if (entityManager.HasComponent<IKBodyPartData>(body_part.AllZombieEntities))
            {

                if (zombie_part == body_part.AllZombieEntities)
                {
                    localized_Ik_factor = 2f;
                }

                //Debug.Log(entityManager.GetComponentData<IKBodyPartData>(body_part.AllZombieEntities).IK_master_entity);

                /// PROGRESSIVE DISABLE 
                /*
                IKMasterBodyPartData new_ik_master_data;

                new_ik_master_data = entityManager.GetComponentData<IKMasterBodyPartData>(entityManager.GetComponentData<IKBodyPartData>(body_part.AllZombieEntities).IK_master_entity);

                new_ik_master_data.full_limb_hp_ratio = Mathf.Clamp(new_ik_master_data.full_limb_hp_ratio - damage * localized_Ik_factor, 0, 1f); // A DIVISER PAR LE NB DE IK BODYPART DE CHAQUE MASTER : 2?
                

                //Debug.Log(new_ik_master_data.full_limb_hp_ratio);

                ecb.SetComponent<IKMasterBodyPartData>(entityManager.GetComponentData<IKBodyPartData>(body_part.AllZombieEntities).IK_master_entity, new_ik_master_data);
                 
                 */
            }
            else if (entityManager.HasComponent<ZombieBalanceData>(body_part.AllZombieEntities))
            {

                if (zombie_part == body_part.AllZombieEntities)
                {
                    localized_Ik_factor = 2f;
                }

                /// PROGRESSIVE DISABLE 
                /*
                ZombieBalanceData new_zombie_balance_data;

                new_zombie_balance_data = entityManager.GetComponentData<ZombieBalanceData>(body_part.AllZombieEntities);

                ///new_zombie_balance_data.limb_hp_ratio = Mathf.Clamp(new_zombie_balance_data.limb_hp_ratio - damage *0.75f * localized_Ik_factor, 0, 1f);

                //Debug.Log(new_zombie_balance_data.limb_hp_ratio);

                ecb.SetComponent<ZombieBalanceData>(body_part.AllZombieEntities, new_zombie_balance_data);
                */
            }


            URPMaterialPropertyBaseColor full_new_color;

            full_new_color = entityManager.GetComponentData<URPMaterialPropertyBaseColor>(body_part.AllZombieEntities);

            full_new_color.Value.x += damage * localized_Ik_factor;
            full_new_color.Value.y -= damage * localized_Ik_factor;


            ecb.SetComponent<URPMaterialPropertyBaseColor>(body_part.AllZombieEntities, full_new_color);

        }


        //Debug.Log(impulse);


        // couleur ajoute au partie visee 
        /*
        URPMaterialPropertyBaseColor part_new_color;

        part_new_color = entityManager.GetComponentData<URPMaterialPropertyBaseColor>(zombie_part);

        part_new_color.Value.x += damage*2f;
        part_new_color.Value.y -= damage*2f;

        ecb.SetComponent<URPMaterialPropertyBaseColor>(zombie_part, part_new_color);
        */



        ZombieBrainData new_brain_data;

        new_brain_data = entityManager.GetComponentData<ZombieBrainData>(entityManager.GetComponentData<ZombiePartData>(zombie_part).zombie_brain_entity);

        new_brain_data.zombie_HP = Mathf.Clamp(new_brain_data.zombie_HP - damage * 100, 0, 100);

        //Debug.Log(new_brain_data.zombie_HP);



        ecb.SetComponent<ZombieBrainData>(entityManager.GetComponentData<ZombiePartData>(zombie_part).zombie_brain_entity, new_brain_data);

        if (new_brain_data.zombie_HP < 10f && !new_brain_data.is_dead)
        {

            foreach (ZombieBrainElementData body_part in entityManager.GetBuffer<ZombieBrainElementData>(entityManager.GetComponentData<ZombiePartData>(zombie_part).zombie_brain_entity))
            {

                if (entityManager.HasComponent<IKBodyPartData>(body_part.AllZombieEntities))
                {

                    IKMasterBodyPartData new_ik_master_data;

                    new_ik_master_data = entityManager.GetComponentData<IKMasterBodyPartData>(entityManager.GetComponentData<IKBodyPartData>(body_part.AllZombieEntities).IK_master_entity);

                    new_ik_master_data.full_limb_hp_ratio = 0; // A DIVISER PAR LE NB DE IK BODYPART DE CHAQUE MASTER : 2?


                    //Debug.Log(new_ik_master_data.full_limb_hp_ratio);

                    ecb.SetComponent<IKMasterBodyPartData>(entityManager.GetComponentData<IKBodyPartData>(body_part.AllZombieEntities).IK_master_entity, new_ik_master_data);


                }
                else if (entityManager.HasComponent<ZombieBalanceData>(body_part.AllZombieEntities))
                {

                    ZombieBalanceData new_zombie_balance_data;

                    new_zombie_balance_data = entityManager.GetComponentData<ZombieBalanceData>(body_part.AllZombieEntities);

                    new_zombie_balance_data.limb_hp_ratio = 0;

                    //Debug.Log(new_zombie_balance_data.limb_hp_ratio);

                    ecb.SetComponent<ZombieBalanceData>(body_part.AllZombieEntities, new_zombie_balance_data);

                }

            }

            new_brain_data.is_dead = true;

            //ecbBS.DestroyEntity(new_brain_data.zombie_root);


            ///RETIRE PAR DEFAULT CAR LES ZOMBIES INSTANCIER AVEC LES ROOMS N ONT PAS DE LINKED GROUP A EUX
            /*
            DelayedDestroyData destroy_data = new DelayedDestroyData();

            destroy_data.destroy_timer = 10f;
            destroy_data.zombie_brain_entity = entityManager.GetComponentData<ZombiePartData>(zombie_part).zombie_brain_entity;

            ecb.AddComponent<DelayedDestroyData>(new_brain_data.zombie_root, destroy_data);
            */

            //Debug.Break();
            /*
            foreach (ZombieBrainElementData body_part in entityManager.GetBuffer<ZombieBrainElementData>(entityManager.GetComponentData<ZombiePartData>(zombie_part).zombie_brain_entity))
            {

               // ecbBS.DestroyEntity(body_part.AllZombieEntities);

            }
            */

        }



    }


    private void Player_detected(Entity zombie)
    {
        // A METTRE DANS UN JOB

        Entity player = SystemAPI.GetSingletonEntity<PlayerHeadTag>();/// entitynativearray[0];


        IKMasterBodyPartData new_ik_L = new IKMasterBodyPartData();
        new_ik_L = entityManager.GetComponentData<IKMasterBodyPartData>(entityManager.GetComponentData<ZombieDetectData>(zombie).Zombie_L_arm);
        new_ik_L.target_pos_entity = player;

        IKMasterBodyPartData new_ik_R = new IKMasterBodyPartData();
        new_ik_R = entityManager.GetComponentData<IKMasterBodyPartData>(entityManager.GetComponentData<ZombieDetectData>(zombie).Zombie_R_arm);
        new_ik_R.target_pos_entity = player;

        entityManager.SetComponentData<IKMasterBodyPartData>(entityManager.GetComponentData<ZombieDetectData>(zombie).Zombie_L_arm, new_ik_L);
        entityManager.SetComponentData<IKMasterBodyPartData>(entityManager.GetComponentData<ZombieDetectData>(zombie).Zombie_R_arm, new_ik_R);

        entityManager.RemoveComponent<Disabled>(entityManager.GetComponentData<ZombieDetectData>(zombie).Zombie_R_arm);
        entityManager.RemoveComponent<Disabled>(entityManager.GetComponentData<ZombieDetectData>(zombie).Zombie_L_arm);

        ZombieBalanceData new_balance = new ZombieBalanceData();
        new_balance = entityManager.GetComponentData<ZombieBalanceData>(entityManager.GetComponentData<ZombieDetectData>(zombie).Zombie_balance);
        new_balance.zombie_target = player;

        entityManager.SetComponentData<ZombieBalanceData>(entityManager.GetComponentData<ZombieDetectData>(zombie).Zombie_balance, new_balance);

        entityManager.DestroyEntity(zombie);


    }



}
