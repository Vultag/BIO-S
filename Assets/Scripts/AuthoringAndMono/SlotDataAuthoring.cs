
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;



public partial struct SlotData : IComponentData
{
    // 0 katana // 1 USP // 3 Mag // same as itemdata
    public int slot_type_id;
    public Entity Storing_entity;
    public Vector3 shape_size;
    public bool has_to_exit;

}
public partial struct SlotTag : IComponentData
{


}


public class SlotDataAuthoring : MonoBehaviour
{
    // 0 katana // 1 USP // 3 Mag // same as itemdata
    public int slot_type_id;
    public GameObject Storing_GB;
    public Vector3 shape_size;

    public class SlotDataBaker : Baker<SlotDataAuthoring>
    {


        public override void Bake(SlotDataAuthoring authoring)
        {

            AddComponent(new SlotData
            {
                slot_type_id = authoring.slot_type_id,
                Storing_entity = GetEntity(authoring.Storing_GB),
                shape_size = authoring.shape_size,
                has_to_exit = false
            }); 
            AddComponent(new SlotTag
            {

            });

        }


    }
}
