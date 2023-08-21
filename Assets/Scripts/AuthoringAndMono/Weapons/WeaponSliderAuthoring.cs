
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Physics;


public partial struct WeaponSliderData : IComponentData
{

    public Entity motor_entity;
    public RigidTransform Afromjoint;
    public RigidTransform Bfromjoint;
    public float motor_target_speed;
    public float motor_max_impulse_applided;

}


public class WeaponSliderAuthoring : MonoBehaviour
{

    public float motor_target_speed;
    public float motor_max_impulse_applided;



    public class WeaponSliderBaker : Baker<WeaponSliderAuthoring>
    {


        public override void Bake(WeaponSliderAuthoring authoring)
        {
            
            AddComponent(new WeaponSliderData
            {
                motor_target_speed = authoring.motor_target_speed,
                motor_max_impulse_applided = authoring.motor_max_impulse_applided

            });
            
        }


    }






}



