
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public partial struct MagazineAttachmentData : IComponentData
{
    public Entity mag_joint_entity;
    public Entity mag_entity;
    public Entity firearm_entity;

}

public class MagazineAttachmentDataAuthoring : MonoBehaviour
{

    public class MagazineAttachmentDataBaker : Baker<MagazineAttachmentDataAuthoring>
    {


        public override void Bake(MagazineAttachmentDataAuthoring authoring)
        {

            AddComponent(new MagazineAttachmentData
            {

            });

        }


    }
}
