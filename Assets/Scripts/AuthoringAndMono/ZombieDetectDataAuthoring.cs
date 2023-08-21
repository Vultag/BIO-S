
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;



public partial struct ZombieDetectData : IComponentData
{

    public Entity Zombie_balance;
    public Entity Zombie_L_arm;
    public Entity Zombie_R_arm;

}



public class ZombieDetectDataAuthoring : MonoBehaviour
{
    public GameObject Zombie_balance_GB;
    public GameObject Zombie_L_arm_GB;
    public GameObject Zombie_R_arm_GB;

    public class ZombieDetectDataBaker : Baker<ZombieDetectDataAuthoring>
    {


        public override void Bake(ZombieDetectDataAuthoring authoring)
        {

            AddComponent(new ZombieDetectData
            {
                Zombie_balance = GetEntity(authoring.Zombie_balance_GB),
                Zombie_L_arm = GetEntity(authoring.Zombie_L_arm_GB),
                Zombie_R_arm = GetEntity(authoring.Zombie_R_arm_GB)
            });

        }


    }
}
