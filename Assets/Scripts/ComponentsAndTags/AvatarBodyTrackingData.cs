
using System.Numerics;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Timeline;

public struct AvatarBodyTrackingData : IComponentData
{
    public Entity Tracked_Entity;
    // 0 = head; 1 = hands; 2 = torso ?
    public int entity_type;

    //sert a tester la fermeture main sans la vr
    //public float temp;
}