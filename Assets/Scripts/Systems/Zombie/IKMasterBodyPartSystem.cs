
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
using UnityEngine.InputSystem;
using Unity.Physics.Authoring;
using Unity.Physics.Systems;

//POUR ENABLE TRANSFORM V1
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
//[UpdateInGroup(typeof(SimulationSystemGroup))]
//[UpdateAfter(typeof(PhysicsSimulationGroup))]

public partial class IKMasterBodyPartSystem : SystemBase
{

    private EntityManager entityManager;


    protected override void OnStartRunning()
    {
        Entities.WithoutBurst()
        .ForEach((ref IKMasterBodyPartData ikmaster, in TransformAspect trans) =>
        {

            ikmaster.upper_limb_0_lenght = entityManager.GetComponentData<IKBodyPartData>(ikmaster.upper_limb_0).limb_lenght;
            ikmaster.upper_limb_1_lenght = entityManager.GetComponentData<IKBodyPartData>(ikmaster.upper_limb_1).limb_lenght;
            ikmaster.max_limbs_reach = ikmaster.upper_limb_0_lenght + ikmaster.upper_limb_1_lenght;

            ikmaster.upper_limb_0_rot_offset = (Vector3)entityManager.GetComponentData<IKBodyPartData>(ikmaster.upper_limb_0).rotation_offset;
            ikmaster.upper_limb_1_rot_offset = (Vector3)entityManager.GetComponentData<IKBodyPartData>(ikmaster.upper_limb_1).rotation_offset;

            ikmaster.upper_limb_0_vel = entityManager.GetComponentData<PhysicsVelocity>(ikmaster.upper_limb_0);
            ikmaster.upper_limb_1_vel = entityManager.GetComponentData<PhysicsVelocity>(ikmaster.upper_limb_1);

            ikmaster.upper_limb_0_mass = entityManager.GetComponentData<PhysicsMass>(ikmaster.upper_limb_0);
            ikmaster.upper_limb_1_mass = entityManager.GetComponentData<PhysicsMass>(ikmaster.upper_limb_1);


        }).Run();
    }


    protected override void OnCreate()
    {
        entityManager = World.EntityManager;



    }


    protected override void OnUpdate()
    {

        
                Entities.WithoutBurst()
        .ForEach((ref IKMasterBodyPartData ikmaster, in TransformAspect trans) =>
        {


            ikmaster.upper_limb_0_lenght = entityManager.GetComponentData<IKBodyPartData>(ikmaster.upper_limb_0).limb_lenght;
            ikmaster.upper_limb_1_lenght = entityManager.GetComponentData<IKBodyPartData>(ikmaster.upper_limb_1).limb_lenght;
            ikmaster.max_limbs_reach = ikmaster.upper_limb_0_lenght + ikmaster.upper_limb_1_lenght;

            ikmaster.upper_limb_0_rot_offset = (Vector3)entityManager.GetComponentData<IKBodyPartData>(ikmaster.upper_limb_0).rotation_offset;
            ikmaster.upper_limb_1_rot_offset = (Vector3)entityManager.GetComponentData<IKBodyPartData>(ikmaster.upper_limb_1).rotation_offset;

            ikmaster.upper_limb_0_vel = entityManager.GetComponentData<PhysicsVelocity>(ikmaster.upper_limb_0);
            ikmaster.upper_limb_1_vel = entityManager.GetComponentData<PhysicsVelocity>(ikmaster.upper_limb_1);

            ikmaster.upper_limb_0_mass = entityManager.GetComponentData<PhysicsMass>(ikmaster.upper_limb_0);
            ikmaster.upper_limb_1_mass = entityManager.GetComponentData<PhysicsMass>(ikmaster.upper_limb_1);


        }).Run();



        
        

        Entities.WithoutBurst()
        .ForEach((ref IKBodyPartData ikbody, in TransformAspect trans, in LocalToWorld ltw) =>
        {



            Matrix4x4 matrix4x4 = Matrix4x4.identity;
            matrix4x4.SetTRS(trans.LocalPosition, Quaternion.Inverse(trans.LocalRotation), new Vector3(1, 1, 1));
            ikbody.ikbody_pivot_point = trans.LocalPosition + (float3)matrix4x4.inverse.MultiplyPoint3x4(trans.LocalPosition + (float3)ikbody.pivot_offset);



        }).Run();



        Entities.WithoutBurst()
        .ForEach((ref IKMasterBodyPartData ikmaster) =>
        {

            // pos de la target !! : Vector3(1.39059997,1.5,-0.756400108)


            ikmaster.upper_limb_0_pivot = (Vector3)entityManager.GetComponentData<IKBodyPartData>(ikmaster.upper_limb_0).ikbody_pivot_point;
            ikmaster.upper_limb_1_pivot = (Vector3)entityManager.GetComponentData<IKBodyPartData>(ikmaster.upper_limb_1).ikbody_pivot_point;

            //a retirer pour plus de perf
            ikmaster.upper_limb_0_rot_offset = (Vector3)entityManager.GetComponentData<IKBodyPartData>(ikmaster.upper_limb_0).rotation_offset;
            ikmaster.upper_limb_1_rot_offset = (Vector3)entityManager.GetComponentData<IKBodyPartData>(ikmaster.upper_limb_1).rotation_offset;


            Vector3 limb_1_look_dir = (Vector3)entityManager.GetComponentData<LocalToWorld>(ikmaster.target_pos_entity).Position - ikmaster.upper_limb_1_pivot;
            Vector3 limb_0_look_dir = (Vector3)entityManager.GetComponentData<LocalToWorld>(ikmaster.target_pos_entity).Position - ikmaster.upper_limb_0_pivot;
          
            Vector3 limb_0_look_up = (Quaternion)entityManager.GetComponentData<LocalToWorld>(ikmaster.upper_limb_0).Rotation * ikmaster.bend_orientation;
            Vector3 limb_1_look_up = (Quaternion)entityManager.GetComponentData<LocalToWorld>(ikmaster.upper_limb_1).Rotation * ikmaster.bend_orientation;//Vector3.forward;
            

            //l orientation du limb1 sera toujour vers le target
            var limb_0_effecive_rot = ((Quaternion)Quaternion.LookRotation(limb_0_look_dir.normalized, limb_0_look_up) * Quaternion.Euler(ikmaster.upper_limb_0_rot_offset)) * Quaternion.Inverse(entityManager.GetComponentData<LocalToWorld>(ikmaster.upper_limb_0).Rotation);

            var limb_1_effecive_rot = ((Quaternion)quaternion.LookRotation(limb_1_look_dir.normalized, limb_1_look_up) * Quaternion.Euler(ikmaster.upper_limb_1_rot_offset)) * Quaternion.Inverse(entityManager.GetComponentData<LocalToWorld>(ikmaster.upper_limb_1).Rotation);


            limb_1_effecive_rot = quaternion.AxisAngle((Quaternion)entityManager.GetComponentData<LocalToWorld>(ikmaster.upper_limb_1).Rotation * ikmaster.bend_orientation, 2f * Mathf.Clamp(ikmaster.max_limbs_reach - Vector3.Distance((Vector3)entityManager.GetComponentData<LocalToWorld>(ikmaster.target_pos_entity).Position, ikmaster.upper_limb_1_pivot), 0f, 10f) * 1 / ikmaster.max_limbs_reach) * limb_1_effecive_rot;



            float limb_0_angleInDegrees;
            Vector3 limb_0_rotationAxis;
            limb_0_effecive_rot.ToAngleAxis(out limb_0_angleInDegrees, out limb_0_rotationAxis);

            float limb_1_angleInDegrees;
            Vector3 limb_1_rotationAxis;
            limb_1_effecive_rot.ToAngleAxis(out limb_1_angleInDegrees, out limb_1_rotationAxis);


            
            if (float.IsInfinity(limb_0_rotationAxis.x))
                return;
            

            if (limb_0_angleInDegrees > 180f)
                limb_0_angleInDegrees -= 360f;

            if (float.IsInfinity(limb_1_rotationAxis.x))
                return;


            if (limb_1_angleInDegrees > 180f)
                limb_1_angleInDegrees -= 360f;
            


            Vector3 limb_0_angularDisplacement = (0.9f* Mathf.Deg2Rad * limb_0_angleInDegrees * ikmaster.IK_base_force * ikmaster.full_limb_hp_ratio) * limb_0_rotationAxis.normalized;
            Vector3 limb_1_angularDisplacement = (0.9f * Mathf.Deg2Rad * limb_1_angleInDegrees * ikmaster.IK_base_force * ikmaster.full_limb_hp_ratio) * limb_1_rotationAxis.normalized;


            var new_body_data_0 = new IKBodyPartData();
            new_body_data_0 = entityManager.GetComponentData<IKBodyPartData>(ikmaster.upper_limb_0);
            new_body_data_0.designated_anglevel = limb_0_angularDisplacement;
            entityManager.SetComponentData<IKBodyPartData>(ikmaster.upper_limb_0, new_body_data_0);
            var new_body_data_1 = new IKBodyPartData();
            new_body_data_1 = entityManager.GetComponentData<IKBodyPartData>(ikmaster.upper_limb_1);
            new_body_data_1.designated_anglevel = limb_1_angularDisplacement;
            entityManager.SetComponentData<IKBodyPartData>(ikmaster.upper_limb_1, new_body_data_1);
            

        }).Run();

    }

}

