using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[InternalBufferCapacity(14)]
public partial struct ZombieBrainElementData : IBufferElementData
{

    public Entity AllZombieEntities;

}

public class ZombieBrainElementDataAuthoring : MonoBehaviour
{

    public GameObject[] AllZombieGameObjects;

    public class ZombieBrainElementDataBaker : Baker<ZombieBrainElementDataAuthoring>
    {

        public override void Bake(ZombieBrainElementDataAuthoring authoring)
        {

            var BrainBuffer = AddBuffer<ZombieBrainElementData>();

            foreach (GameObject gb in authoring.AllZombieGameObjects)
            {
                BrainBuffer.Add(new ZombieBrainElementData{ AllZombieEntities = GetEntity(gb) });
            }

            

        }


    }
}
