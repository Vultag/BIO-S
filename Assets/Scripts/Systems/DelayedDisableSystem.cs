
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public partial class DelayedDisableSystem : SystemBase
{

    private EntityManager entityManager;
    private EntityCommandBuffer ecbES;

    protected override void OnStartRunning()
    {

    }

    protected override void OnCreate()
    {

        RequireForUpdate<DelayedDisableData>();
        entityManager = World.EntityManager;

    }


    protected override void OnUpdate()
    {

        foreach (var (delay_disable, entity) in SystemAPI.Query<RefRW<DelayedDisableData>>().WithEntityAccess())
        {


            //Debug.Log(entity.Index);

            delay_disable.ValueRW.disable_timer -= SystemAPI.Time.DeltaTime;

            if (delay_disable.ValueRO.disable_timer < 0)
            {

                ecbES = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);


                //ecbES.AddComponent<Disabled>(entity);
                foreach(Child child_light in SystemAPI.GetBuffer<Child>(entity))
                {
                    ecbES.AddComponent<Disabled>(child_light.Value); 
                }


                ecbES.RemoveComponent<DelayedDisableData>(entity);

            }


        }

    }
}
