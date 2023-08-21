using System.Numerics;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Timeline;


public struct ZombieHipsReferencePointData : IComponentData
{
    public Entity ZombieTorsoEntity;
    public Entity ZombieHipsEntity;
}

public class ZombieHipsReferencePointDataAuthoring : MonoBehaviour
{
    public GameObject ZombieTorsoEntity;
    public GameObject ZombieHipsEntity;

    class ZombieHipsReferencePointDataBaker : Baker<ZombieHipsReferencePointDataAuthoring>
    {
        public override void Bake(ZombieHipsReferencePointDataAuthoring authoring)
        {
            AddComponent(new ZombieHipsReferencePointData
            {
                ZombieTorsoEntity = GetEntity(authoring.ZombieTorsoEntity),
                ZombieHipsEntity = GetEntity(authoring.ZombieHipsEntity)

            });
        }
    }
}