
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
using Unity.VisualScripting;
using Unity.Physics.Systems;
using Unity.Rendering;
using Unity.Entities.UniversalDelegates;
using Matrix4x4 = UnityEngine.Matrix4x4;
using RaycastHit = Unity.Physics.RaycastHit;
using UnityEngine.VFX;

//[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]


//[UpdateInGroup(typeof(LateSimulationSystemGroup))]


public partial class ItemSystem : SystemBase
{



    private EntityManager entityManager;
    private EntityCommandBuffer ecb;


    protected override void OnCreate()
    {
        entityManager = World.EntityManager;

    }


    protected override void OnStartRunning()
    {
        /*
        ///Get all sliders and assign full WeaponSliderData
        Entities.WithoutBurst()
        .ForEach((Entity entity, ref PhysicsJoint motor, in Parent parent) =>//, ref WeaponSliderData slider_data) =>
        {
            //Debug.Log(motor.JointType);

            if (motor.JointType == JointType.LinearVelocityMotor)
            {
                Debug.Log(parent.Value);

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


        }).Run();
        */
    }

    protected override void OnUpdate()
    {


    }
    public void item_trigger_release_action(Entity item)
    {

        ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

        switch (entityManager.GetComponentData<ItemData>(item).item_type_id)
        {
            //katana
            case 0:
                //n a pas d action
                break;
            //USP
            case 1:
                //n a pas d action
                break;
            //mag
            case 3:
                //n a pas d action
                break;
            //Skorpion
            case 4:

                //Debug.Log("shoot");

                if (entityManager.HasComponent<AutomaticFiringData>(item))
                {

                    ecb.RemoveComponent<AutomaticFiringData>(item);

                }
                break;

        }

    }

    public void item_trigger_action(Entity item)
    {


        ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

        switch (entityManager.GetComponentData<ItemData>(item).item_type_id)
        {
            //katana
            case 0:
                //n a pas d action
                break;
            //USP
            case 1:
                //Debug.Log("shoot");





                if (entityManager.GetComponentData<FirearmData>(item).loaded_magazine != Entity.Null)
                {

                    Entity mag = entityManager.GetComponentData<FirearmData>(item).loaded_magazine;

                    //Debug.Log("has_mag");

                    ///A REACTIVER AVEC LE ELSE IF !!!
                    if(entityManager.GetComponentData<MagazineData>(mag).bullets_in_clip > 0)
                    {


                        MagazineData new_mag_data;

                        new_mag_data = entityManager.GetComponentData<MagazineData>(mag);

                        ///REACTIVE UNLOADING THE MAG
                        new_mag_data.bullets_in_clip -= 1;

                        entityManager.SetComponentData<MagazineData>(mag, new_mag_data);

                        if(entityManager.GetComponentData<MagazineData>(mag).bullets_in_clip == 2)
                            ecb.AddComponent<Disabled>(entityManager.GetComponentData<MagazineData>(mag).last_bullet_1);
                        if (entityManager.GetComponentData<MagazineData>(mag).bullets_in_clip == 1)
                            ecb.AddComponent<Disabled>(entityManager.GetComponentData<MagazineData>(mag).last_bullet_0);



                        if(new_mag_data.bullets_in_clip == 0)
                        {

                            WeaponSliderData slider_data = entityManager.GetComponentData<WeaponSliderData>(entityManager.GetComponentData<FirearmData>(item).slider_entity);

                            var new_motor = PhysicsJoint.CreateLinearVelocityMotor(
                                   slider_data.Afromjoint,
                                   slider_data.Bfromjoint,
                                   -slider_data.motor_target_speed,
                                   slider_data.motor_max_impulse_applided
                               );

                            ///Debug.Log(slider_data.motor_entity);

                            ecb.SetComponent<PhysicsJoint>(slider_data.motor_entity, new_motor);

                        }


                        _shoot_firearm(item, 0);

                    }
                    
                    else if (!entityManager.HasComponent<Disabled>(entityManager.GetComponentData<FirearmData>(item).chamber_bullet))
                        ecb.AddComponent<Disabled>(entityManager.GetComponentData<FirearmData>(item).chamber_bullet);
                    

                }
                else
                {

                    //Debug.Log("no_mag");
                }

                break;
            //mag
            case 3:
                //n a pas d action
                break; 
            //Skorpion
            case 4:

                //Debug.Log("shoot");

                if (entityManager.GetComponentData<FirearmData>(item).loaded_magazine != Entity.Null)
                {
                    //Debug.Log("has_mag");

                    ///A REACTIVER AVEC LE ELSE IF !!!
                    if (entityManager.GetComponentData<MagazineData>(entityManager.GetComponentData<FirearmData>(item).loaded_magazine).bullets_in_clip > 0)
                    {

                        AutomaticFiringData new_fire_data= new AutomaticFiringData();
                        //new_fire_data.firing_CD = 60f / (float)entityManager.GetComponentData<FirearmData>(item).RPM;

                        //Debug.Log(new_fire_data.firing_CD);
                        ecb.AddComponent<AutomaticFiringData>(item, new_fire_data);

                    }

                }
                else
                {

                    //Debug.Log("no_mag");
                }
                break;     
                
                //flashlight
            case 5:

                Entities
                .WithoutBurst()
                .WithAll<ItemData>()
                .ForEach((Entity entity, Light light) =>
                {

                    if (light.intensity == 20)
                    {
                        light.intensity = 0;
                    }
                    else if(light.intensity == 0)
                    {
                        light.intensity = 20;
                    }

                    if (light.intensity == 0.2f)
                    {
                        light.intensity = 0.0002f;
                    }
                    else if (light.intensity == 0.0002f)
                    {
                        light.intensity = 0.2f;
                    }


                })
                .Run();

                break;

        }


    }

    public void _shoot_firearm(Entity item,int firearm_id)
    {
        PhysicsWorldSingleton phyworld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

        //vraiment pas ouf pour faire le vfx mais pas trouve d autre soluce
        Entities.WithoutBurst().ForEach((Entity entity, VisualEffect vfx) =>
        {
            //Debug.Log(entity);
            if(entity == entityManager.GetComponentData<FirearmData>(item).muzzle_flash_entity)
            {
                vfx.SetInt("firearm_id", firearm_id);
                vfx.Play();
            }

        }).Run();


        //muzzle flash light
        //Debug.Log(entityManager.GetComponentData<FirearmData>(item).muzzle_flash_light_entity);
        ///disable que tt les childs
        ///ecb.RemoveComponent<Disabled>(entityManager.GetComponentData<FirearmData>(item).muzzle_flash_light_entity);
        foreach (Child child_light in SystemAPI.GetBuffer<Child>(entityManager.GetComponentData<FirearmData>(item).muzzle_flash_light_entity))
        {
            ecb.RemoveComponent<Disabled>(child_light.Value);
        }
        ecb.AddComponent<DelayedDisableData>(entityManager.GetComponentData<FirearmData>(item).muzzle_flash_light_entity, new DelayedDisableData {disable_timer = 0.05f});



        var raycastinputs = new RaycastInput
        {
            Start = (Vector3)entityManager.GetComponentData<LocalToWorld>(item).Position,
            End = entityManager.GetComponentData<LocalToWorld>(item).Position + (float3)((Quaternion)entityManager.GetComponentData<LocalToWorld>(item).Rotation * entityManager.GetComponentData<FirearmData>(item).bullet_spawn_pos) + (float3)(entityManager.GetComponentData<LocalToWorld>(item).Forward * 100f),//(Vector3)target_reference_pos,
            Filter = new CollisionFilter
            {
                BelongsTo = 2,
                CollidesWith = 1 | 2,
                GroupIndex = 0,
            }
        };


        if (phyworld.CastRay(raycastinputs, out var hit))
        {


            //Debug.Log(hit.Entity);                              


            Debug.DrawRay(entityManager.GetComponentData<LocalToWorld>(item).Position + (float3)((Quaternion)entityManager.GetComponentData<LocalToWorld>(item).Rotation * entityManager.GetComponentData<FirearmData>(item).bullet_spawn_pos), entityManager.GetComponentData<LocalToWorld>(item).Forward * 100f, Color.red, 2f);


            if (entityManager.HasComponent<ZombiePartTag>(hit.Entity))
            {

                //Debug.Log("hit_zombie");            
                ///instencie bullet_impact
                Entity bullet_impact = ecb.Instantiate(entityManager.GetComponentData<FirearmData>(item).bullet_impact_body_entity);


                Matrix4x4 matrix4x4 = entityManager.GetComponentData<LocalToWorld>(hit.Entity).Value;


                //reparente
                ecb.AddComponent(bullet_impact, new Parent { Value = hit.Entity });


                ecb.SetComponent<Translation>(bullet_impact, new Translation() { Value = matrix4x4.inverse.MultiplyPoint3x4(hit.Position) });
                ///ecb.SetComponent<Rotation>(bullet_impact, new Rotation() { Value = Quaternion.Inverse((entityManager.GetComponentData<LocalToWorld>(hit.Entity).Rotation) * Quaternion.FromToRotation(Vector3.up, hit.SurfaceNormal) * Quaternion.Euler(90, 0, 0)) });
                ecb.SetComponent<Rotation>(bullet_impact, new Rotation() { Value = Quaternion.Inverse(entityManager.GetComponentData<LocalToWorld>(hit.Entity).Rotation) * Quaternion.Euler(hit.SurfaceNormal.z * 90 + 90 - hit.SurfaceNormal.x * 90, hit.SurfaceNormal.y * 90 + hit.SurfaceNormal.x * 90, 0) });


                ecb.AddComponent(bullet_impact, new LocalToParent { });


                if(entityManager.HasBuffer<LinkedEntityGroup>(hit.Entity))
                {
                    var buffer = entityManager.GetBuffer<LinkedEntityGroup>(hit.Entity);
                    buffer.Add(bullet_impact);
                }
                else
                {
                    var buffer = ecb.AddBuffer<LinkedEntityGroup>(hit.Entity);
                    buffer.Add(bullet_impact);
                }


                PhysicsVelocity new_target_vel = entityManager.GetComponentData<PhysicsVelocity>(hit.Entity);

                PhysicsComponentExtensions.ApplyLinearImpulse(ref new_target_vel, entityManager.GetComponentData<PhysicsMass>(hit.Entity), entityManager.GetComponentData<LocalToWorld>(item).Forward * 10f);

                ecb.SetComponent<PhysicsVelocity>(hit.Entity, new_target_vel);

                _deal_bullet_damage(hit.Entity, 3);



            }
            else
            {


                var sphere_Filter = new CollisionFilter
                {
                    BelongsTo = 1 << 7,
                    CollidesWith = 1 << 7,
                    GroupIndex = 0,
                };

                NativeList<DistanceHit> outHits = new NativeList<DistanceHit>(Allocator.TempJob);

                if (phyworld.OverlapSphere((Vector3)entityManager.GetComponentData<LocalToWorld>(item).Position, 7, ref outHits, sphere_Filter))
                {
                    //Debug.Log("zombie_trigger");
                    //Debug.Log(outHits.Length);

                    foreach (var sphere_hit in outHits)
                    {
                        Debug.Log(outHits[0].Entity.Index);
                        Debug.Log(outHits.Length);
                        Player_detected(sphere_hit.Entity);
                        //Debug.Break();
                    }


                }



                //instencie bullet_impact
                Entity bullet_impact = ecb.Instantiate(entityManager.GetComponentData<FirearmData>(item).bullet_impact_metal_entity);


                Matrix4x4 matrix4x4 = entityManager.GetComponentData<LocalToWorld>(hit.Entity).Value;

                //reparente
                ecb.AddComponent(bullet_impact, new Parent { Value = hit.Entity });



                ecb.SetComponent<Translation>(bullet_impact, new Translation() { Value = matrix4x4.inverse.MultiplyPoint3x4(hit.Position) });
                
                Vector3 hit_normal = new Vector3(hit.SurfaceNormal.x, hit.SurfaceNormal.y, hit.SurfaceNormal.z);

                //Debug.Log(hit_normal);

                ecb.SetComponent<Rotation>(bullet_impact, new Rotation() { Value = Quaternion.Inverse(entityManager.GetComponentData<LocalToWorld>(hit.Entity).Rotation) * Quaternion.Euler(hit_normal.z * 90+90 - hit_normal.x * 90, hit_normal.y * 90 + hit_normal.x * 90, 0) });

                ecb.AddComponent(bullet_impact, new LocalToParent {  });


                ///RETIRE PAR DEFAULT CAR LES ZOMBIES INSTANCIER AVEC LES ROOMS N ONT PAS DE LINKED GROUP A EUX
                /*
                    DelayedDestroyData destroy_data = new DelayedDestroyData();

                    destroy_data.destroy_timer = 20f;

                    ecb.AddComponent<DelayedDestroyData>(bullet_impact, destroy_data);
                */


            }

        }


        var new_base_vel = entityManager.GetComponentData<PhysicsVelocity>(item);
        var new_slide_vel = entityManager.GetComponentData<PhysicsVelocity>(entityManager.GetComponentData<ItemData>(item).composite_sub_part);

        PhysicsComponentExtensions.ApplyLinearImpulse(ref new_base_vel, entityManager.GetComponentData<PhysicsMass>(item), -entityManager.GetComponentData<LocalToWorld>(item).Forward * 5);//effet de recul sur base
        PhysicsComponentExtensions.ApplyAngularImpulse(ref new_base_vel, entityManager.GetComponentData<PhysicsMass>(item), new float3(-1,0,0) * 2.5f); //-entityManager.GetComponentData<LocalToWorld>(item).Right * 2.5f);
        PhysicsComponentExtensions.ApplyLinearImpulse(ref new_slide_vel, entityManager.GetComponentData<PhysicsMass>(item), -entityManager.GetComponentData<LocalToWorld>(item).Forward * 20);//effet de recul sur slide

        entityManager.SetComponentData<PhysicsVelocity>(item, new_base_vel);
        entityManager.SetComponentData<PhysicsVelocity>(entityManager.GetComponentData<ItemData>(item).composite_sub_part, new_slide_vel);
    }



    private void _deal_bullet_damage(Entity zombie_part, float impulse)
    {

        float damage = impulse * 0.1f;


        foreach (ZombieBrainElementData body_part in entityManager.GetBuffer<ZombieBrainElementData>(entityManager.GetComponentData<ZombiePartData>(zombie_part).zombie_brain_entity))
        {

            float localized_Ik_factor = 1f;


            if (entityManager.HasComponent<IKBodyPartData>(body_part.AllZombieEntities))
            {

                if (zombie_part == body_part.AllZombieEntities)
                {
                    localized_Ik_factor = 2f;
                }

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


                ZombieBalanceData new_zombie_balance_data;

                new_zombie_balance_data = entityManager.GetComponentData<ZombieBalanceData>(body_part.AllZombieEntities);

                /// PROGRESSIVE DISABLE 
                ///new_zombie_balance_data.limb_hp_ratio = Mathf.Clamp(new_zombie_balance_data.limb_hp_ratio - damage * 0.75f * localized_Ik_factor, 0, 1f);

                if (new_zombie_balance_data.zombie_target == Entity.Null)
                {

                    Entity zombie = entityManager.GetComponentData<ZombieBrainData>(entityManager.GetComponentData<ZombiePartData>(body_part.AllZombieEntities).zombie_brain_entity).zombie_detect;

                    ///Entity player = triggerJob.Player_entity.Value;
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

                    new_zombie_balance_data.zombie_target = player;

                    ecb.DestroyEntity(zombie);



                    Head_query.Dispose();
                }

                ecb.SetComponent<ZombieBalanceData>(body_part.AllZombieEntities, new_zombie_balance_data);

            }


            URPMaterialPropertyBaseColor full_new_color;

            full_new_color = entityManager.GetComponentData<URPMaterialPropertyBaseColor>(body_part.AllZombieEntities);

            full_new_color.Value.x += damage * localized_Ik_factor;
            full_new_color.Value.y -= damage * localized_Ik_factor;


            ecb.SetComponent<URPMaterialPropertyBaseColor>(body_part.AllZombieEntities, full_new_color);

        }



        ZombieBrainData new_brain_data;

        new_brain_data = entityManager.GetComponentData<ZombieBrainData>(entityManager.GetComponentData<ZombiePartData>(zombie_part).zombie_brain_entity);

        new_brain_data.zombie_HP = Mathf.Clamp(new_brain_data.zombie_HP - damage * 100, 0, 100);




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

            ///RETIRE PAR DEFAULT CAR LES ZOMBIES INSTANCIER AVEC LES ROOMS N ONT PAS DE LINKED GROUP A EUX
            /*
            DelayedDestroyData destroy_data = new DelayedDestroyData();

            destroy_data.destroy_timer = 10f;
            destroy_data.zombie_brain_entity = entityManager.GetComponentData<ZombiePartData>(zombie_part).zombie_brain_entity;

            ecb.AddComponent<DelayedDestroyData>(new_brain_data.zombie_root, destroy_data);
            */


        }



    }


    private void Player_detected(Entity zombie)
    {
        // A METTRE DANS UN JOB

        //ecb = new EntityCommandBuffer(Allocator.Temp);
        ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

        ///Entity player = triggerJob.Player_entity.Value;
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
