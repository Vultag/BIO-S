
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
using static UnityEngine.GraphicsBuffer;
using Unity.VisualScripting;
using Unity.Physics.Extensions;
using Unity.Rendering;
using Unity.Physics.Systems;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]

public partial class AvatarBodyTrackingSystem : SystemBase
{


    private EntityManager entityManager;

    ///Dans la version precedente ou il n y a pas de rotation verticale
    ///faut que ce soit local
    /// private float3 torso_offset_from_camera = new Vector3(0, 0.1f, -0.15f);

    private float3 hand_offset_from_camera = new Vector3(0, 0, 0);



    protected override void OnCreate()
    {
        entityManager = World.EntityManager;

    }

    protected override void OnUpdate()
    {

        Entities.WithoutBurst()
        .ForEach((ref PhysicsVelocity vel, in LocalToWorld ltw, in TransformAspect trans, in AvatarBodyTrackingData avatar_data, in PhysicsMass mass) =>
        {

            Vector3 distance;

             if (avatar_data.entity_type == 0)
                distance = ((Vector3)((entityManager.GetComponentData<LocalToWorld>(avatar_data.Tracked_Entity).Position) - ltw.Position));
            else
                distance = ((Vector3)((entityManager.GetComponentData<LocalToWorld>(avatar_data.Tracked_Entity).Position + hand_offset_from_camera) - ltw.Position));

            var target_rotation = (Quaternion)(entityManager.GetComponentData<LocalToWorld>(avatar_data.Tracked_Entity).Rotation);


            if (avatar_data.entity_type == 1)
                target_rotation *= Quaternion.Euler(-90, 90, -90);
            else if (avatar_data.entity_type == 2)
                target_rotation *= Quaternion.Euler(90, -90, 90);

            var rot_diff = (target_rotation * Quaternion.Inverse(ltw.Rotation));


            vel.Linear = distance *15f; // a preciser


            float angle;
            Vector3 axis;
            rot_diff.ToAngleAxis(out angle, out axis);


            if (float.IsInfinity(axis.x))
                return;


            if (angle > 180f)
                angle -= 360f;

            Vector3 angularDisplacement = (0.9f * Mathf.Deg2Rad * angle * 50f) * axis.normalized;

            PhysicsComponentExtensions.SetAngularVelocityWorldSpace(ref vel, mass, trans.WorldRotation, angularDisplacement);




        }).Run();
    }

}
