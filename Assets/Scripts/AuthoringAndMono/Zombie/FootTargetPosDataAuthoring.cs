

using System.Numerics;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Timeline;
using Vector3 = UnityEngine.Vector3;

public struct FootTargetPosData : IComponentData
{
    public Entity zombie_target_pos_entity;
    public Entity other_foot_target;
    public Vector3 target_pos_reference_point_offset;
    [HideInInspector]
    public Vector3 interpol_start;
    [HideInInspector]
    public Vector3 interpol_end;
    [HideInInspector]
    public float interpolate_amount;
    [HideInInspector]
    public bool foot_walking;
    public bool is_grounded;
    public Vector3 point_AB;
    public Vector3 point_BC;
}
public class FootTargetPosDataAuthoring : MonoBehaviour
{
    public GameObject zombie_target_pos_entity;
    public GameObject other_foot_target;
    public Vector3 target_pos_reference_point_offset;
    //public bool is_grounded;

    class FootTargetPosDataBaker : Baker<FootTargetPosDataAuthoring>
    {
        public override void Bake(FootTargetPosDataAuthoring authoring)
        {
            AddComponent(new FootTargetPosData
            {
                zombie_target_pos_entity = GetEntity(authoring.zombie_target_pos_entity),
                other_foot_target = GetEntity(authoring.other_foot_target),
                target_pos_reference_point_offset = authoring.target_pos_reference_point_offset,
                is_grounded = true
            });
        }
    }
}