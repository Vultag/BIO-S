
using Unity.Entities;
using UnityEngine;
using Unity.Jobs;
using Unity.Physics;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.UI;
using Unity.Entities.UniversalDelegates;
using UnityEngine.UIElements;
using System.Numerics;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.VisualScripting;
using static Unity.Physics.Math;

//[UpdateInGroup(typeof(InitializationSystemGroup))]

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]


///change qq chose ? j avais un warning qui dit ca sert a rien
///[UpdateAfter(typeof(PhysicsSimulationGroup))]
public partial class IKBodyPartSystem : SystemBase
{

    private EntityManager entityManager;


    protected override void OnStartRunning()
    {

    }


    protected override void OnCreate()
    {
        entityManager = World.EntityManager;


    }


    protected override void OnUpdate()
    {

        Entities.WithoutBurst()
        .ForEach((ref PhysicsVelocity vel, ref IKBodyPartData ikbody, in LocalToWorld ltw, in PhysicsMass mass) =>
        {

            PhysicsComponentExtensions.SetAngularVelocityWorldSpace(ref vel, mass, ltw.Rotation, ikbody.designated_anglevel);


        }).Run();

    }

}

