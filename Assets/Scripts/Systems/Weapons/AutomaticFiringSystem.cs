
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

///[AlwaysSynchronizeSystem]
///POUR TRANSFORM V1
//[UpdateInGroup(typeof(SimulationSystemGroup))]

//retire pour que le follow soit continue -> cause un probleme ?
///[UpdateInGroup(typeof(BeforePhysicsSystemGroup))]

//[UpdateAfter(typeof(FixedStepSimulationSystemGroup))]
public partial class AutomaticFiringSystem : SystemBase
{


    private ItemSystem Itemsystem;

    private EntityManager entityManager;

    private EntityCommandBuffer ecb;

    protected override void OnCreate()
    {
        entityManager = World.EntityManager;
        RequireForUpdate<AutomaticFiringData>();
        Itemsystem = World.GetExistingSystemManaged<ItemSystem>();
    }

    protected override void OnUpdate()
    {

        ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

        foreach (var (firing_data, entity) in SystemAPI.Query<RefRW<AutomaticFiringData>>().WithEntityAccess())
        {

            //Debug.Log(firing_data.ValueRW.firing_CD);


            if (firing_data.ValueRW.firing_CD <= 0)
            {

                 if (entityManager.GetComponentData<FirearmData>(entity).loaded_magazine != Entity.Null)
                    {

                    Entity mag = entityManager.GetComponentData<FirearmData>(entity).loaded_magazine;

                    //Debug.Log("has_mag");

                    if (entityManager.GetComponentData<MagazineData>(mag).bullets_in_clip > 0)
                    {


                        MagazineData new_mag_data;

                        new_mag_data = entityManager.GetComponentData<MagazineData>(mag);

                        //REACTIVE
                        ///new_mag_data.bullets_in_clip -= 1;

                        entityManager.SetComponentData<MagazineData>(mag, new_mag_data);

                        if (entityManager.GetComponentData<MagazineData>(mag).bullets_in_clip == 2)
                            ecb.AddComponent<Disabled>(entityManager.GetComponentData<MagazineData>(mag).last_bullet_1);
                        if (entityManager.GetComponentData<MagazineData>(mag).bullets_in_clip == 1)
                            ecb.AddComponent<Disabled>(entityManager.GetComponentData<MagazineData>(mag).last_bullet_0);

                        //Debug.Log(firing_data.ValueRW.firing_CD);

                        if (new_mag_data.bullets_in_clip == 0)
                        {

                            WeaponSliderData slider_data = entityManager.GetComponentData<WeaponSliderData>(entityManager.GetComponentData<FirearmData>(entity).slider_entity);

                            var new_motor = PhysicsJoint.CreateLinearVelocityMotor(
                                   slider_data.Afromjoint,
                                   slider_data.Bfromjoint,
                                   -slider_data.motor_target_speed,
                                   slider_data.motor_max_impulse_applided
                               );

                            ///Debug.Log(slider_data.motor_entity);

                            ecb.SetComponent<PhysicsJoint>(slider_data.motor_entity, new_motor);

                        }


                        Itemsystem._shoot_firearm(entity, 1);

                        firing_data.ValueRW.firing_CD = 60f / (float)entityManager.GetComponentData<FirearmData>(entity).RPM;


                    }

                    else if (!entityManager.HasComponent<Disabled>(entityManager.GetComponentData<FirearmData>(entity).chamber_bullet))
                        ecb.AddComponent<Disabled>(entityManager.GetComponentData<FirearmData>(entity).chamber_bullet);
                    else
                    {
                        //Debug.Log("out_of_bullet");
                        ecb.RemoveComponent<AutomaticFiringData>(entity);

                    }
                 }


            }
            else
            {

                //Debug.Log(World.Time.DeltaTime);

            }

            firing_data.ValueRW.firing_CD -= World.Time.DeltaTime;

        }

    }
}