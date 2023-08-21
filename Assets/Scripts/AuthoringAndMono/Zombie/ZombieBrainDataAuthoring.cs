
using System.Numerics;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;


public partial struct ZombieBrainData : IComponentData
{
    //public Entity zombie_target;
    public Entity propeled_bodypart;
    public Entity zombie_root;
    public Entity zombie_detect;
    public float zombie_HP;
    public bool is_dead;
}
public class ZombieBrainDataAuthoring : MonoBehaviour
{

   // public GameObject zombie_target;
    public GameObject propeled_bodypart;
    public float zombie_HP;
    public GameObject zombie_root;
    public GameObject zombie_detect;

    class ZombieBrainDataBaker : Baker<ZombieBrainDataAuthoring>
    {
        public override void Bake(ZombieBrainDataAuthoring authoring)
        {
            AddComponent(new ZombieBrainData
            {
                //zombie_target = GetEntity(authoring.zombie_target),
                propeled_bodypart = GetEntity(authoring.propeled_bodypart),
                zombie_detect = GetEntity(authoring.zombie_detect),
                zombie_HP = authoring.zombie_HP,
                is_dead =false,
                zombie_root = GetEntity(authoring.zombie_root)

            });
        }
    }


}