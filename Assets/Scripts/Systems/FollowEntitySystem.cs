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

//[UpdateBefore(typeof(TransformSystemGroup))]
public partial class FollowEntitySystem : SystemBase
{

    private EntityManager entityManager;


    protected override void OnCreate()
    {
        entityManager = World.EntityManager;
    }

    protected override void OnUpdate()
    {


        ///set pos
        Entities
            .WithAll<LocalToWorld>()
        .ForEach((Entity entity, in FollowEntityData data) =>
        {

            var trans = SystemAPI.GetAspectRW<TransformAspect>(entity);

            trans.WorldPosition = SystemAPI.GetAspectRW<TransformAspect>(data.entity_to_follow).WorldPosition;
            trans.WorldRotation = SystemAPI.GetAspectRW<TransformAspect>(data.entity_to_follow).WorldRotation;


        }).Schedule();



    }




}