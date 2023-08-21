
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(TransformSystemGroup))]
public partial class LookAtPlayerSystem : SystemBase
{

    private EntityManager entityManager;

    protected override void OnStartRunning()
    {

    }

    protected override void OnCreate()
    {

        entityManager = World.EntityManager;

    }
    private Entity GetHeadEntity()
    {

        EntityQuery Head_query = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(PlayerHeadTag));

        NativeArray<Entity> entitynativearray = Head_query.ToEntityArray(Unity.Collections.Allocator.TempJob);

        return entitynativearray[0];

    }



    protected override void OnUpdate()
    {

        foreach (var (ltw, scale) in SystemAPI.Query<RefRW<LocalToWorld>,RefRO<NonUniformScale>>().WithAll<UITag>())
        {

            Quaternion new_rot = Quaternion.LookRotation(ltw.ValueRO.Position - entityManager.GetComponentData<LocalToWorld>(GetHeadEntity()).Position, entityManager.GetComponentData<LocalToWorld>(GetHeadEntity()).Up);

            ltw.ValueRW.Value = float4x4.TRS(ltw.ValueRO.Position, new_rot, scale.ValueRO.Value);



        }

    }
}
