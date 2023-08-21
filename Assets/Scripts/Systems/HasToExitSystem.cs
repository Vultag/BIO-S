
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


//probablement a tweak
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]

public partial class HasToExitSystem : SystemBase
{


    private EntityManager entityManager;

    private EntityCommandBuffer ecb;


    protected override void OnCreate()
    {
        entityManager = World.EntityManager;
        RequireForUpdate<HasToExitData>();

    }

    protected override void OnUpdate()
    {


        foreach (var(exit_data, entity) in SystemAPI.Query<RefRO<HasToExitData>>().WithEntityAccess())
        {

            ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            NativeList<DistanceHit> hits = new NativeList<DistanceHit>(Allocator.TempJob);

            var Filter = new CollisionFilter
            {
                BelongsTo = 3,
                CollidesWith =  exit_data.ValueRO.physics_layer,
                GroupIndex = 0,
            };

            PhysicsWorldSingleton phyworld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

            Debug.DrawLine(entityManager.GetComponentData<LocalToWorld>(exit_data.ValueRO.shape_entity).Position, entityManager.GetComponentData<LocalToWorld>(exit_data.ValueRO.shape_entity).Position + (float3)((Quaternion)entityManager.GetComponentData<LocalToWorld>(exit_data.ValueRO.shape_entity).Rotation * new float3(exit_data.ValueRO.shape_size.x, 0, 0)), Color.red);

            Debug.DrawLine(entityManager.GetComponentData<LocalToWorld>(exit_data.ValueRO.shape_entity).Position, entityManager.GetComponentData<LocalToWorld>(exit_data.ValueRO.shape_entity).Position + (float3)((Quaternion)entityManager.GetComponentData<LocalToWorld>(exit_data.ValueRO.shape_entity).Rotation * new float3(0, exit_data.ValueRO.shape_size.y, 0)), Color.red);

            Debug.DrawLine(entityManager.GetComponentData<LocalToWorld>(exit_data.ValueRO.shape_entity).Position, entityManager.GetComponentData<LocalToWorld>(exit_data.ValueRO.shape_entity).Position + (float3)((Quaternion)entityManager.GetComponentData<LocalToWorld>(exit_data.ValueRO.shape_entity).Rotation * new float3(0, 0, exit_data.ValueRO.shape_size.z)), Color.red);



            if (phyworld.OverlapBox(entityManager.GetComponentData<LocalToWorld>(exit_data.ValueRO.shape_entity).Position, entityManager.GetComponentData<LocalToWorld>(exit_data.ValueRO.shape_entity).Rotation, exit_data.ValueRO.shape_size * 0.5f, ref hits, Filter))
            {
                //Debug.Log("hit");
            }
            else
            {
                //Debug.Log("NO_hit");
                ecb.RemoveComponent<HasToExitData>(entity);
            }

        }

    }

}