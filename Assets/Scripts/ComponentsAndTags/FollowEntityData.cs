
using System.Numerics;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;
using Vector3 = UnityEngine.Vector3;

public partial struct FollowEntityData : IComponentData
{

    public Entity entity_to_follow;

}