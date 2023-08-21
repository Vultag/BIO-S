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
public partial class FollowEntitySystem : SystemBase
{

    private EntityManager entityManager;


    protected override void OnCreate()
    {
        entityManager = World.EntityManager;
    }

    protected override void OnUpdate()
    {

        Entities
            .WithoutBurst()
        .ForEach((ref TransformAspect trans, in FollowEntityData data) =>
        {

            trans.WorldPosition = entityManager.GetComponentData<LocalToWorld>(data.entity_to_follow).Position;
            trans.WorldRotation = entityManager.GetComponentData<LocalToWorld>(data.entity_to_follow).Rotation;


        }).Run();
    }
}