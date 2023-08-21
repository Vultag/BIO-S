using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
public class Test_copy_tag_mono : MonoBehaviour
{

}

public class Test_copy_tag_Baker : Baker<Test_copy_tag_mono>
{

    public override void Bake(Test_copy_tag_mono authoring)
    {

        AddComponent(new Test_XR_input_tag
        {

        });
    }


}
