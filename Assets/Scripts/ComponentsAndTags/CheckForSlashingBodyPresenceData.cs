using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;


public partial struct CheckForSlashingBodyPresenceData : IComponentData
{

    public Entity slashing_entity;
    public Entity slashed_entity;
    public Entity joint_entity;
    public PhysicsJoint joint;
    public int rigid_state; //0 soft, 1 rigid

}

