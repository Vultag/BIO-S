
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;



public class TEST_join_mono : MonoBehaviour
{
    
    public GameObject body_to_joint;

}

public class TEST_join_Baker : Baker<TEST_join_mono>
{

    public override void Bake(TEST_join_mono authoring)
    {

        AddComponent(new TEST_joint_data
        {
            body_to_joint = GetEntity(authoring.body_to_joint)
        });
    }


}
