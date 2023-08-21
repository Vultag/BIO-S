
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Timeline;

public struct ZombieBalanceData : IComponentData
{

    public Entity zombie_target;
    public float limb_hp_ratio;
    public float force;
    public int foot_grounded;
    public UnityEngine.Vector3 pivot_location;
    public Entity target_pos_entity;
    public UnityEngine.Vector3 starting_orientation;
}

public class ZombieBalanceDataAuthoring : MonoBehaviour
{
    public GameObject zombie_target;
    public float force;
    //public int foot_grounded;
    public UnityEngine.Vector3 pivot_location;
    public GameObject target_pos_entity;




}

class ZombieBalanceDataBaker : Baker<ZombieBalanceDataAuthoring>
{
    public override void Bake(ZombieBalanceDataAuthoring authoring)
    {
        AddComponent(new ZombieBalanceData
        {
            ///zombie_target = GetEntity(authoring.zombie_target),
            starting_orientation = UnityEngine.Quaternion.Euler(0, UnityEngine.Random.Range(0.0f, 360.0f), 0) * Vector3.forward,
            force = authoring.force,
            limb_hp_ratio = 1,
            pivot_location = authoring.pivot_location,
            target_pos_entity = GetEntity(authoring.target_pos_entity)

        });
    }
}
