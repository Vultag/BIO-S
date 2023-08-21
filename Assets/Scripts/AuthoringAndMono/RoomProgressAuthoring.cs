
using System.Numerics;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;


public partial struct RoomProgressData : IComponentData
{
    public int state;

}
public class RoomProgressAuthoring : MonoBehaviour
{

    public int state;

    class RoomProgressDataBaker : Baker<RoomProgressAuthoring>
    {
        public override void Bake(RoomProgressAuthoring authoring)
        {

            AddComponent(new RoomProgressData
            {

                state = authoring.state

            });
        }
    }
}