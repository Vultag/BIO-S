using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;


[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]

public partial class ZombieSpawnerSystem : SystemBase
{

    private EntityManager entityManager;
    private EntityCommandBuffer ecbBS;
    //private EntityCommandBuffer ecbES;

    protected override void OnStartRunning()
    {

    }


    protected override void OnCreate()
    {
        entityManager = World.EntityManager;

        RequireForUpdate<ZombieSpawnerData>();

    }


    protected override void OnUpdate()
    {

        foreach ( var (spawner,ltw) in SystemAPI.Query<RefRW<ZombieSpawnerData>,RefRO<LocalToWorld>>())
        {

            if (spawner.ValueRO.active == true)
            {
                spawner.ValueRW.respawn_timer -= SystemAPI.Time.DeltaTime;


                if (spawner.ValueRO.respawn_timer < 0)
                {
                    if (spawner.ValueRO.zombie_quantity > 0)
                    {
                    ecbBS = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
                   


                    var instance = new NativeArray<Entity>(1, Allocator.Temp);

                    //sync point -> chiant
                    entityManager.Instantiate(spawner.ValueRO.zombie_prefab, instance);

                    //pour stocker l offset des physics bodies qui sont instancie
                    List<float3> PhyBodiesOffset = new();


                    foreach (var linked in entityManager.GetBuffer<LinkedEntityGroup>(instance[0]))
                    {

                        var entity = linked.Value;

                        if (entityManager.HasComponent<PhysicsVelocity>(entity))
                        {
                            PhyBodiesOffset.Add(entityManager.GetComponentData<LocalToWorld>(entity).Position);

                        }


                    }


                    //je deplace le root du prefab
                    entityManager.SetComponentData<Translation>(instance[0], new Translation { Value = ltw.ValueRO.Position });


                    //je replace les bodies en fonction
                    int index = 0;
                    foreach (var linked in entityManager.GetBuffer<LinkedEntityGroup>(instance[0]))
                    {

                        var entity = linked.Value;

                        if (entityManager.HasComponent<PhysicsVelocity>(entity))
                        {

                            Translation trans_body = new();

                            trans_body.Value = ltw.ValueRO.Position + PhyBodiesOffset[index];


                            entityManager.SetComponentData<Translation>(entity, trans_body);

                            index++;

                        }


                    }


                    foreach (var balance_data in SystemAPI.Query<RefRW<ZombieBalanceData>>())
                    {

                        balance_data.ValueRW.zombie_target = spawner.ValueRO.player_ref_entity;


                    }


                    foreach (var hand_follow in SystemAPI.Query<RefRW<FollowEntityData>>().WithAll<IKMasterBodyPartData>())
                    {

                        hand_follow.ValueRW.entity_to_follow = spawner.ValueRO.player_ref_entity;

                    }

                    spawner.ValueRW.respawn_timer = spawner.ValueRO.timer;
                    spawner.ValueRW.zombie_quantity -= 1;

                }
                    else
                    {
                        spawner.ValueRW.active = false;
                    }
                }
            }

        }

    }
}
