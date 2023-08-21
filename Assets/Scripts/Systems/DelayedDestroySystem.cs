
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public partial class DelayedDestroySystem : SystemBase
{

    private EntityManager entityManager;
    private EntityCommandBuffer ecbES;
    private EntityCommandBuffer ecbBS;

    protected override void OnStartRunning()
    {

    }

    protected override void OnCreate()
    {

        RequireForUpdate<DelayedDestroyData>();
        entityManager = World.EntityManager;

    }


    protected override void OnUpdate()
    {

        foreach (var (delay_destroy,entity) in SystemAPI.Query<RefRW<DelayedDestroyData>>().WithEntityAccess())
        {


            //Debug.Log(entity.Index);

            delay_destroy.ValueRW.destroy_timer -= SystemAPI.Time.DeltaTime;

            if (delay_destroy.ValueRO.destroy_timer < 0)
            {

                ecbES = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
                ecbBS = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);



                if (delay_destroy.ValueRO.zombie_brain_entity != Entity.Null)
                {
                    foreach (ZombieBrainElementData body_part in entityManager.GetBuffer<ZombieBrainElementData>(delay_destroy.ValueRO.zombie_brain_entity))
                    {



                        PhysicsCollider new_col = new PhysicsCollider();



                        ecbES.SetComponent<PhysicsCollider>(body_part.AllZombieEntities, new_col);



                    }
                }

                ecbES.DestroyEntity(entity);

            }


        }

    }
}
