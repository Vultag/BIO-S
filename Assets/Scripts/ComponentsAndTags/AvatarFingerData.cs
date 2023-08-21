
using System.Numerics;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Timeline;


public struct AvatarFingerData : IComponentData
{
    public Entity fantome_correspondant;
    public Entity upper_finger_bone;
    public Entity hand_root;
    public Entity Skin_finger_entity;
    //public bool hand_adjustement_trigger_switch;
    //public bool physics_fingers;
}