
using System.Numerics;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Timeline;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public struct FantomeFingerData : IComponentData
{
    //  0 = right hand / 1 = left hand
    public int hand_ID;
    public Entity hand_root;
    public Vector3 open_hand_rot;
    public Vector3 close_hand_rot;
    public Vector3 hold_USP_0_hand_rot; // gachette leve
    public Vector3 hold_USP_1_hand_rot; // gachette appuye
}