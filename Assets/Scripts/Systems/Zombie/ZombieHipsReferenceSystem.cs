
using Unity.Entities;
using UnityEngine;
using Unity.Jobs;
using Unity.Physics;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.UI;

[AlwaysSynchronizeSystem]
public partial class ZombieHipsReferenceSystem : SystemBase
{

    private EntityManager entityManager;

    protected override void OnCreate()
    {
        entityManager = World.EntityManager;
    }


    protected override void OnUpdate()
    {

        Entities.WithoutBurst()
        .ForEach((ref TransformAspect trans, in ZombieHipsReferencePointData ref_point) =>
        {

            trans.WorldPosition = entityManager.GetComponentData<LocalToWorld>(ref_point.ZombieTorsoEntity).Position;

            Quaternion target_rot = entityManager.GetComponentData<LocalToWorld>(ref_point.ZombieTorsoEntity).Rotation;


            float angleInDegrees;
            Vector3 rotationAxis;

            target_rot.ToAngleAxis(out angleInDegrees, out rotationAxis);

            Vector3 angularDisplacement = rotationAxis * angleInDegrees * Mathf.Deg2Rad; // 10 = vitesse

            trans.WorldRotation = quaternion.EulerXYZ(0, angularDisplacement.y, 0) ;



        }).Run();

    }

}