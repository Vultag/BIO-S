
using Unity.Entities;
using UnityEngine;
using Unity.Jobs;
using Unity.Physics;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.UI;

//[AlwaysSynchronizeSystem]
//[UpdateInGroup(typeof(FixedStepSimulationSystemGroup),OrderLast = true)]
//[UpdateAfter(typeof(CharacterControllerSystem))]
public partial class ZombieHipsReferenceSystem : SystemBase
{

    private EntityManager entityManager;

    protected override void OnCreate()
    {
        entityManager = World.EntityManager;
    }


    protected override void OnUpdate()
    {

        Entities
            .WithAll<LocalToWorld, LocalToParent>()
        .ForEach((Entity entity ,in ZombieHipsReferencePointData ref_point) =>
        {

            var trans = SystemAPI.GetAspectRW<TransformAspect>(entity);

            trans.WorldPosition = SystemAPI.GetAspectRO<TransformAspect>(ref_point.ZombieTorsoEntity).WorldPosition;

            Quaternion target_rot = SystemAPI.GetAspectRO<TransformAspect>(ref_point.ZombieTorsoEntity).WorldRotation;


            float angleInDegrees;
            Vector3 rotationAxis;

            target_rot.ToAngleAxis(out angleInDegrees, out rotationAxis);

            Vector3 angularDisplacement = rotationAxis * angleInDegrees * Mathf.Deg2Rad; // 10 = vitesse

            trans.WorldRotation = quaternion.EulerXYZ(0, angularDisplacement.y, 0) ;



        }).Schedule();

    }

}