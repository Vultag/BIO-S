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

//[AlwaysSynchronizeSystem]

//[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
//[UpdateAfter(typeof(PhysicsSimulationGroup))]

public partial class ZombieBalanceSystem : SystemBase
{

    private EntityManager entityManager;

    protected override void OnCreate()
    {
        RequireForUpdate<ZombieBalanceData>();
        entityManager = World.EntityManager;
    }

    protected override void OnStartRunning()
    {
        //?
        base.OnStartRunning();



    }

    

    protected override void OnUpdate()
    {

        float deltaTime = SystemAPI.Time.DeltaTime;

        Entities.WithoutBurst()
        .ForEach((ref PhysicsVelocity vel, ref ZombieBalanceData balancedata, ref LocalToWorld ltw, ref TransformAspect trans, in PhysicsMass mass) =>
        {

            Quaternion rot_target;

            if (balancedata.zombie_target != Entity.Null)
            {

                Vector3 target_pos = entityManager.GetComponentData<LocalToWorld>(balancedata.zombie_target).Position;

                var target_direction = new Vector3(target_pos.x, 0, target_pos.z) - new Vector3(ltw.Position.x, 0, ltw.Position.z);


                if (balancedata.foot_grounded > 0)
                {
                    vel.Linear += (float3)target_direction.normalized * 1f * balancedata.limb_hp_ratio;
                    balancedata.foot_grounded = 0;
                }

                rot_target = Quaternion.LookRotation(target_direction.normalized, Vector3.up); //Quaternion.Inverse(Quaternion.LookRotation(ltw.Forward, look_up));

            }
            else
            {
                rot_target = Quaternion.LookRotation(balancedata.starting_orientation, Vector3.up); //Quaternion.Inverse(Quaternion.LookRotation(ltw.Forward, look_up));
            }


            var chest_effecive_rot = (Quaternion)math.mul(rot_target, Quaternion.Inverse(ltw.Rotation));


            float chest_angleInDegrees;
            Vector3 chest_rotationAxis;
            chest_effecive_rot.ToAngleAxis(out chest_angleInDegrees, out chest_rotationAxis);

            if (float.IsInfinity(chest_rotationAxis.x))
                return;


            if (chest_angleInDegrees > 180f)
                chest_angleInDegrees -= 360f;

            Vector3 chest_angularDisplacement = (0.9f * Mathf.Deg2Rad * chest_angleInDegrees * 120 * balancedata.limb_hp_ratio) * chest_rotationAxis.normalized; // 10 = vitesse


            PhysicsComponentExtensions.SetAngularVelocityWorldSpace(ref vel, mass, ltw.Rotation, new Vector3(chest_angularDisplacement.x, chest_angularDisplacement.y, chest_angularDisplacement.z));


        }).Run();


    }

}
