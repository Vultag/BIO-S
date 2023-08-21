
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
public partial class DoorSystem : SystemBase
{

    private EntityManager entityManager;

    private EntityCommandBuffer ecb;

    protected override void OnCreate()
    {
        entityManager = World.EntityManager;
        RequireForUpdate<DoorData>();
    }

    protected override void OnUpdate()
    {
        foreach (var (vel,door_data,ltw,entity) in SystemAPI.Query<RefRW<PhysicsVelocity>,RefRO<DoorData>,RefRO<LocalToWorld>>().WithEntityAccess())
        {


            if (door_data.ValueRO.starting_world_y + door_data.ValueRO.target_local_y > ltw.ValueRO.Position.y)
            {
                //Debug.Log("move");
                vel.ValueRW.Linear = new Vector3(0,door_data.ValueRO.open_speed,0);
            }
            else
            {

                vel.ValueRW.Linear = Vector3.zero;

                ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

                ecb.RemoveComponent<DoorData>(entity);

            }

        }

    }
}