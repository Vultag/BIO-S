
using System.Numerics;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Timeline;
using Vector3 = System.Numerics.Vector3;

public struct TEST_DistanceMesurer : IComponentData
{
    public Entity pointAEntity;
    public Entity pointBEntity;
}

public class TEST_DistanceMesurerAuthoring : MonoBehaviour
{

    public GameObject pointAGB;
    public GameObject pointBGB;


    class TEST_DistanceMesurerBaker : Baker<TEST_DistanceMesurerAuthoring>
    {
        public override void Bake(TEST_DistanceMesurerAuthoring authoring)
        {
            AddComponent(new TEST_DistanceMesurer
            {
                pointBEntity = GetEntity(authoring.pointBGB),
                pointAEntity = GetEntity(authoring.pointAGB)
            });
        }
    }
}