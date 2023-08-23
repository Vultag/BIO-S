
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public partial struct MagazineData : IComponentData
{
    //public bool mag_fixed;
    public int bullets_in_clip;
    public int max_bullets_in_clip;
    //public Entity mag_joint_entity;
    public Entity last_bullet_0;
    public Entity last_bullet_1;

    public Entity disable_col_joint;
    public Entity loaded_mag_joint;

}

public class MagazineDataAuthoring : MonoBehaviour
{

    public int bullets_in_clip;
    public GameObject last_bullet_0;
    public GameObject last_bullet_1;

    public class MagazineDataBaker : Baker<MagazineDataAuthoring>
    {


        public override void Bake(MagazineDataAuthoring authoring)
        {

            AddComponent(new MagazineData
            {
                //add max_bullet_in_clip in authoring for custom clip fill
                bullets_in_clip = authoring.bullets_in_clip,
                max_bullets_in_clip = authoring.bullets_in_clip,
                last_bullet_0 = GetEntity(authoring.last_bullet_0),
                last_bullet_1 = GetEntity(authoring.last_bullet_1),
            });

        }


    }
}
