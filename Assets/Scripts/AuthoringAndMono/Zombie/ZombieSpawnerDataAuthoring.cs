using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Entities;
using UnityEngine;


public partial struct ZombieSpawnerData : IComponentData
{

    public Entity zombie_prefab;
    public float timer;
    public int zombie_quantity;
    public float respawn_timer;
    public Entity player_ref_entity;
    public bool active;

}
public class ZombieSpawnerDataAuthoring : MonoBehaviour
{

    public GameObject zombie_prefab;
    public float respawn_timer;
    public int zombie_quantity;
    public GameObject player_ref_entity;
    public bool active;


    class ZombieSpawnerDataBaker : Baker<ZombieSpawnerDataAuthoring>
    {
        public override void Bake(ZombieSpawnerDataAuthoring authoring)
        {


            AddComponent(new ZombieSpawnerData
            {

                zombie_prefab = GetEntity(authoring.zombie_prefab),
                respawn_timer = authoring.respawn_timer,
                zombie_quantity = authoring.zombie_quantity,
                timer = authoring.respawn_timer,
                player_ref_entity = GetEntity(authoring.player_ref_entity),
                active = authoring.active

            });
        }
    }
}
