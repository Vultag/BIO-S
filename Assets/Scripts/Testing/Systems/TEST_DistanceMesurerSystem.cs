
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public partial class TEST_DistanceMesurerSystem : SystemBase
{

    private EntityManager entityManager;



    protected override void OnCreate()
    {

        RequireForUpdate<TEST_DistanceMesurer>();
        entityManager = World.EntityManager;

    }
    protected override void OnStartRunning()
    {
        foreach (var distance_point in SystemAPI.Query<TEST_DistanceMesurer>())
        {
            Debug.Log(Vector3.Distance(entityManager.GetComponentData<LocalToWorld>(distance_point.pointAEntity).Position, entityManager.GetComponentData<LocalToWorld>(distance_point.pointBEntity).Position));

        }

    }


    protected override void OnUpdate()
    {


    }
}
