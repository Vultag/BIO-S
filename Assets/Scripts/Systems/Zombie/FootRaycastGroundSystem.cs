using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Physics.Extensions;
using Unity.Rendering;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.Windows;
using static UnityEngine.UI.Image;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
//[UpdateInGroup(typeof(SimulationSystemGroup))] //comprendre ca
public partial class FootRaycastGroundSystem : SystemBase
{

    private EntityQuery query;

    private EntityManager entityManager;


    [BurstCompile]
    public struct RaycastJob : IJobParallelFor
    {
        [ReadOnly] public CollisionWorld world;
        [ReadOnly] public NativeArray<RaycastInput> inputs;
        public NativeArray<Unity.Physics.RaycastHit> results;

        public unsafe void Execute(int index)
        {
            Unity.Physics.RaycastHit hit;
            world.CastRay(inputs[index], out hit);
            results[index] = hit;
        }
    }

    protected override void OnCreate()
    {
        entityManager = World.EntityManager;
       
    }

    public static JobHandle ScheduleBatchRayCast(CollisionWorld world,
    NativeArray<RaycastInput> inputs, NativeArray<Unity.Physics.RaycastHit> results)
    {
        JobHandle rcj = new RaycastJob
        {
            inputs = inputs,
            results = results,
            world = world

        }.Schedule(inputs.Length, 4);
        return rcj;
    }




    protected override void OnUpdate()
    {
        query = EntityManager.CreateEntityQuery(typeof(FootBalancePushData));
        int datacount = query.CalculateEntityCount();

        PhysicsWorldSingleton phy_world = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld col_world = phy_world.PhysicsWorld.CollisionWorld;

        NativeArray<Unity.Physics.RaycastHit> raycasthit_result = new NativeArray<Unity.Physics.RaycastHit>(datacount, Allocator.TempJob);

        var rayResults = new NativeArray<Unity.Physics.RaycastHit>(datacount, Allocator.TempJob);
        var rayCommands = new NativeArray<RaycastInput>(datacount, Allocator.TempJob);

        Entities.WithBurst()
        .ForEach((Entity entity, int entityInQueryIndex,in FootBalancePushData footdata,in TransformAspect trans, in LocalToWorld ltw) =>
        {

            Matrix4x4 matrix4x4 = Matrix4x4.identity;
            matrix4x4.SetTRS(trans.WorldPosition, Quaternion.Inverse(trans.WorldRotation), new Vector3(1,1,1));

            var raycastinputs = new RaycastInput
            {
                Start = (Vector3)trans.WorldPosition + matrix4x4.inverse.MultiplyPoint3x4((Vector3)trans.WorldPosition + footdata.push_location),
                End = (Vector3)trans.WorldPosition + matrix4x4.inverse.MultiplyPoint3x4((Vector3)trans.WorldPosition + footdata.push_location) + matrix4x4.inverse.MultiplyVector(Vector3.down*0.06f),// + Vector3.down,
                Filter = new CollisionFilter
                {
                    BelongsTo = 1,
                    CollidesWith = 1,
                    GroupIndex = 0,
                }
            };

            rayCommands[entityInQueryIndex] = raycastinputs;


        }).Run();

        var handle = ScheduleBatchRayCast(col_world, rayCommands, rayResults);
        handle.Complete();

        
        Entities.WithoutBurst()
        .ForEach((Entity entity, int entityInQueryIndex, ref FootBalancePushData footdata, in TransformAspect trans) =>
        {

            if (rayResults[entityInQueryIndex].Entity != Entity.Null)
            {

                footdata.grounded = true;


                var new_color = new Unity.Rendering.URPMaterialPropertyBaseColor();
                new_color.Value = new float4(0, 1, 0, 1);

                var new_foot_grounded_data = new ZombieBalanceData();
                new_foot_grounded_data = entityManager.GetComponentData<ZombieBalanceData>(footdata.parent_chest);
                new_foot_grounded_data.foot_grounded = entityManager.GetComponentData<ZombieBalanceData>(footdata.parent_chest).foot_grounded+1;

                entityManager.SetComponentData<ZombieBalanceData>(footdata.parent_chest, new_foot_grounded_data);

                entityManager.SetComponentData<URPMaterialPropertyBaseColor>(entity, new_color);
            }
            else
            {

                footdata.grounded = false;

                var new_color = new Unity.Rendering.URPMaterialPropertyBaseColor();
                new_color.Value = new float4(1, 0, 0, 1);

                entityManager.SetComponentData<URPMaterialPropertyBaseColor>(entity, new_color);

            }



        }).Run();
        
        

        for (int i = rayCommands.Length-1; i > -1; i--)
        {

            //Debug.DrawLine(rayCommands[i].Start, rayCommands[i].End);

        }

        for (int i = rayResults.Length-1; i > 0; i--)
        {
            //Debug.Log(rayResults[i]);
            

            //if (!entityManager.HasComponent<URPMaterialPropertyBaseColor>(rayResults[i].Entity))
                //entityManager.AddComponent<URPMaterialPropertyBaseColor>(rayResults[i].Entity);


        }

        Entities.WithoutBurst()
        .ForEach((Entity entity, int entityInQueryIndex, ref PhysicsVelocity vel, in PhysicsMass mass, in FootBalancePushData footdata, in TransformAspect trans) =>
        {

            float3 new_linear_vel = vel.Linear.xyz;

            var target_direction = ((Vector3)(entityManager.GetComponentData<LocalToWorld>(footdata.target_pos_entity).Position - trans.WorldPosition));

            if (math.abs(Vector3.Distance((Vector3)(entityManager.GetComponentData<LocalToWorld>(footdata.target_pos_entity).Position), trans.WorldPosition)) > 0.3f)
                new_linear_vel += footdata.foot_speed * (float3)target_direction * SystemAPI.Time.DeltaTime;

        }).Run();




        raycasthit_result.Dispose();
        rayCommands.Dispose(); 
        rayResults.Dispose();





    }
}

