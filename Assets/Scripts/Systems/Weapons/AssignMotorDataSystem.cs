
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
using Unity.VisualScripting;
using Unity.Physics.Systems;
using Unity.Rendering;
using Unity.Entities.UniversalDelegates;
using Matrix4x4 = UnityEngine.Matrix4x4;
using RaycastHit = Unity.Physics.RaycastHit;
using UnityEngine.VFX;


[UpdateInGroup(typeof(LateSimulationSystemGroup))]


public partial class AssignMotorDataSystem : SystemBase
{



    private EntityManager entityManager;
    private EntityCommandBuffer ecb;


    protected override void OnCreate()
    {
        entityManager = World.EntityManager;

    }


    protected override void OnStartRunning()
    {
        _AssignMotorData();


    }

    protected override void OnUpdate()
    {


    }

    public void _AssignMotorData()
    {

        ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

        ///Get all sliders and assign full WeaponSliderData
        Entities.WithoutBurst()
        .ForEach((Entity entity, ref PhysicsJoint motor, in Parent parent) =>//, ref WeaponSliderData slider_data) =>
        {
            //Debug.Log(motor.JointType);

            if (motor.JointType == JointType.LinearVelocityMotor)
            {
                //Debug.Log(parent.Value);

                if (entityManager.HasComponent<WeaponSliderData>(parent.Value))
                {

                    WeaponSliderData new_slider_data = entityManager.GetComponentData<WeaponSliderData>(parent.Value);


                    new_slider_data.Afromjoint = motor.BodyAFromJoint.AsRigidTransform();
                    new_slider_data.Bfromjoint = motor.BodyBFromJoint.AsRigidTransform();
                    new_slider_data.motor_entity = entity;

                    //Debug.Log(entity);
                    ecb.SetComponent<WeaponSliderData>(parent.Value, new_slider_data);


                }
                else
                {
                    Debug.LogError("velocity motor not attached to gun slider, missing WeaponSliderData on slider");
                }

            }


        }).Run();

    }


}