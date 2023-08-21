
using System.Numerics;
using Unity.Collections;
using Unity.Entities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;
using Vector3 = UnityEngine.Vector3;



public struct IKBodyPartData : IComponentData
{

    //public int limb_order;
    public float limb_lenght;
    //public float upper_limb_lenght;
    //public float lower_limb_lenght;
    public Entity IK_master_entity;
    public Vector3 pivot_offset;
    public Vector3 rotation_offset;
    [HideInInspector]public Vector3 ikbody_pivot_point;
    public Vector3 designated_anglevel;
    //public Vector3 upper_limb_pivot_offset;
    //public Vector3 lower_limb_pivot_offset;
    //public Vector3 upper_limb_rotation_offset;
    //public Vector3 lower_limb_rotation_offset;

}

public class IKBodyPartDataAuthoring : MonoBehaviour
{

    public float limb_lenght;
    public GameObject IK_master_entity;
    public Vector3 pivot_offset;
    public Vector3 rotation_offset;

    class IKBodyPartDataBaker : Baker<IKBodyPartDataAuthoring>
    {
        public override void Bake(IKBodyPartDataAuthoring authoring)
        {
            AddComponent(new IKBodyPartData
            {

                limb_lenght = authoring.limb_lenght,
                IK_master_entity = GetEntity(authoring.IK_master_entity),
                pivot_offset = authoring.pivot_offset,
                rotation_offset = authoring.rotation_offset

            });
        }
    }
}
