
using System.Numerics;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;


public partial struct RoomManagerData : IComponentData
{
    public Entity room_0_prefab;
    public Entity room_1_2_prefab;
    public Entity room_3_prefab;
    public Entity room_4_prefab;
    public Entity room_0_instance;
    public Entity room_1_2_instance;
    public Entity room_3_instance;
    public Entity room_4_instance;

}
public class RoomManagerAuthoring : MonoBehaviour
{

    public GameObject room_0_prefab;
    public GameObject room_1_2_prefab;
    public GameObject room_3_prefab;
    public GameObject room_4_prefab;

    class RoomManagerDataBaker : Baker<RoomManagerAuthoring>
    {
        public override void Bake(RoomManagerAuthoring authoring)
        {

            AddComponent(new RoomManagerData
            {

                room_0_prefab = GetEntity(authoring.room_0_prefab),
                room_1_2_prefab = GetEntity(authoring.room_1_2_prefab),
                room_3_prefab = GetEntity(authoring.room_3_prefab),
                room_4_prefab = GetEntity(authoring.room_4_prefab)

            });
        }
    }
}