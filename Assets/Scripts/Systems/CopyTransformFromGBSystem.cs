
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

//[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial class ItemSystemCopyTransformFromGBSystem : SystemBase
{ 


    private EntityManager entityManager;

    //static MonoBehaviour copy_mono;

    static GameObject right_hand_gb;
    static GameObject left_hand_gb;
    static GameObject camera_gb;
    private LocalToWorld right_hand_gb_transform;

    protected override void OnCreate()
    {
        entityManager = World.EntityManager;

    }

    protected override void OnStartRunning()
    {

        right_hand_gb = GameObject.FindGameObjectWithTag("right_hand");
        left_hand_gb = GameObject.FindGameObjectWithTag("left_hand");
        camera_gb = GameObject.FindGameObjectWithTag("main_camera_target");


        Entities.WithoutBurst()
        .ForEach((ref CopyTransformFromGB copy, in LocalToWorld ltw) =>
        {

            copy.initial_rot = ltw.Rotation;

            //Debug.LogWarning(copy.initial_rot);


        }).Run();

    }



    protected override void OnUpdate()
    {

        
        Entities.WithoutBurst()
        .ForEach((ref TransformAspect trans, in CopyTransformFromGB copy) =>
        {

            if (copy.is_head)
            {
                trans.WorldPosition = camera_gb.transform.position;
                trans.WorldRotation = camera_gb.transform.rotation;
            }
            else
            {
                if (copy.is_right_hand)
                {
                    trans.WorldPosition = right_hand_gb.transform.position;
                    trans.WorldRotation = right_hand_gb.transform.rotation * Quaternion.Euler(0, -90, 0); //ICI
                }
                else
                {
                    trans.WorldPosition = left_hand_gb.transform.position;
                    trans.WorldRotation =  left_hand_gb.transform.rotation * Quaternion.Euler(0,90,0); //ICI
                }
            }


        }).Run();
        


    }

}