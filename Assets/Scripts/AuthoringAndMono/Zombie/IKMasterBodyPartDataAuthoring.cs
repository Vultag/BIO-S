
using System.Numerics;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;
using Vector3 = UnityEngine.Vector3;


public struct IKMasterBodyPartData : IComponentData
{


    public float full_limb_hp_ratio;
    public Entity target_pos_entity;
    public Entity upper_limb_0;
    public Entity upper_limb_1;

    //get component pour chaque ou faire ca ? quel est le plus performant ?
    public float upper_limb_0_lenght;
    public float upper_limb_1_lenght;
    public float max_limbs_reach;
    public float IK_base_force;
    public Vector3 bend_orientation;


    public Vector3 upper_limb_0_pivot;
    public Vector3 upper_limb_1_pivot;

    //a preciser pour chaque a l avenir ?(oui)
    public Vector3 upper_limb_0_rot_offset;
    public Vector3 upper_limb_1_rot_offset;

    public PhysicsVelocity upper_limb_0_vel;
    public PhysicsVelocity upper_limb_1_vel;

    public PhysicsMass upper_limb_0_mass;
    public PhysicsMass upper_limb_1_mass;

    public Vector3 upper_limb_0_designated_anglevel;
    public Vector3 uupper_limb_1_designated_anglevel;


}

public class IKMasterBodyPartDataAuthoring : MonoBehaviour
{

    public GameObject target_pos_entity;
    public GameObject upper_limb_0;
    public GameObject upper_limb_1;
    public float IK_base_force;
    public Vector3 bend_orientation;

    class IKMasterBodyPartDataBaker : Baker<IKMasterBodyPartDataAuthoring>
    {
        public override void Bake(IKMasterBodyPartDataAuthoring authoring)
        {
            AddComponent(new IKMasterBodyPartData
            {

                target_pos_entity = GetEntity(authoring.target_pos_entity),
                upper_limb_0 = GetEntity(authoring.upper_limb_0),
                upper_limb_1 = GetEntity(authoring.upper_limb_1),
                full_limb_hp_ratio = 1,
                IK_base_force = authoring.IK_base_force,
                bend_orientation = authoring.bend_orientation
                

            });
        }
    }
}