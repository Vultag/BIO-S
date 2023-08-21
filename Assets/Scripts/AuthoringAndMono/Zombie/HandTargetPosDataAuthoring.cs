
using Unity.Entities;
using UnityEngine;
using UnityEngine.Timeline;
using Vector3 = UnityEngine.Vector3;

public struct HandTargetPosData : IComponentData
{
    public Entity zombie_reference_pos_entity;
    public Vector3 target_pos_reference_point_offset;
}
public class HandTargetPosDataAuthoring : MonoBehaviour
{
    public GameObject zombie_reference_pos_entity;
    //public GameObject other_foot_target;
    public Vector3 target_pos_reference_point_offset;
    //public bool is_grounded;

    class HandTargetPosDataBaker : Baker<HandTargetPosDataAuthoring>
    {
        public override void Bake(HandTargetPosDataAuthoring authoring)
        {
            AddComponent(new HandTargetPosData
            {
                zombie_reference_pos_entity = GetEntity(authoring.zombie_reference_pos_entity),
                //other_foot_target = GetEntity(authoring.other_foot_target),
                target_pos_reference_point_offset = authoring.target_pos_reference_point_offset
            });
        }
    }
}