
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
using Unity.Physics.Authoring;
using Material = Unity.Physics.Material;
using UnityEngine.Assertions;
using static UnityEngine.EventSystems.EventTrigger;
using Unity.Burst;
using Unity.XR.CoreUtils;
using System.Buffers;
using Unity.Collections;
using Unity.Rendering;
using UnityEngine.Rendering;
using System.Collections.Generic;

//[AlwaysSynchronizeSystem]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial class AvatarFingerSystem : SystemBase
{

    private EndInitializationEntityCommandBufferSystem _endInitializationEntityCommandBufferSystem;
    private EntityManager entityManager;

    protected override void OnCreate()
    {
        entityManager = World.EntityManager;

    }

    protected override void OnStartRunning()
    {


    }


    protected override void OnUpdate()
    {

        Entities
        .ForEach((ref PhysicsVelocity vel, in LocalToWorld ltw, in AvatarFingerData fingerdata, in PhysicsMass mass) =>
        {


            //var trans = SystemAPI.GetAspectRW<TransformAspect>(entity);
            //var vel = SystemAPI.GetComponent<PhysicsVelocity>(entity);


            //if (entityManager.GetComponentData<GrabDetectData>(fingerdata.hand_root).grabbing == false)
            {

                var delta_rot_target = Quaternion.identity;
                if (fingerdata.upper_finger_bone != Entity.Null)
                   delta_rot_target = (Quaternion)math.mul((Quaternion)SystemAPI.GetComponent<LocalToWorld>(fingerdata.upper_finger_bone).Rotation * SystemAPI.GetComponent<Rotation>(fingerdata.fantome_correspondant).Value, Quaternion.Inverse(ltw.Rotation)); //Quaternion.Inverse(Quaternion.LookRotation(ltw.Forward, look_up));
                else
                    delta_rot_target = (Quaternion)math.mul(SystemAPI.GetComponent<LocalToWorld>(fingerdata.fantome_correspondant).Rotation, Quaternion.Inverse(ltw.Rotation)); //Quaternion.Inverse(Quaternion.LookRotation(ltw.Forward, look_up));


                float finger_angleInDegrees;
                Vector3 finger_rotationAxis;
                delta_rot_target.ToAngleAxis(out finger_angleInDegrees, out finger_rotationAxis);

                if (float.IsInfinity(finger_rotationAxis.x))
                    return;


                if (finger_angleInDegrees > 180f)
                    finger_angleInDegrees -= 360f;


                Vector3 finger_angularDisplacement = (0.9f * Mathf.Deg2Rad * finger_angleInDegrees * 30/*100*/) * finger_rotationAxis.normalized;


                PhysicsComponentExtensions.SetAngularVelocityWorldSpace(ref vel, mass, ltw.Rotation, finger_angularDisplacement);

            }

            //SystemAPI.SetComponent<PhysicsVelocity>(entity,vel);


        }).Schedule();



    }





}
