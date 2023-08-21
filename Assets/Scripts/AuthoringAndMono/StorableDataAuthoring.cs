
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;



public partial struct StorableData : IComponentData
{

    public Entity storing_entity;
    //public DynamicBuffer<Entity> store_joints_entities;
    public Entity store_joint_0; 
    public Entity store_joint_1;

}

public partial struct StorableTag : IComponentData
{


}

public class StorableDataAuthoring : MonoBehaviour
{


    public class StorableDataBaker : Baker<StorableDataAuthoring>
    {


        public override void Bake(StorableDataAuthoring authoring)
        {

            AddComponent(new StorableData
            {
                //store_joints_entities = new DynamicBuffer<Entity>()
            }); 
            AddComponent(new StorableTag
            {

            });

        }


    }
}

