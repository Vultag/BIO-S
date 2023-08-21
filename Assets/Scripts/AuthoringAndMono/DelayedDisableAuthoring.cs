
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public partial struct DelayedDisableData : IComponentData
{


    public float disable_timer;

}


public class DelayedDisableAuthoring : MonoBehaviour
{
    public float disable_timer;

    class DelayedDisableAuthoringBaker : Baker<DelayedDisableAuthoring>
    {
        public override void Bake(DelayedDisableAuthoring authoring)
        {

            AddComponent(new DelayedDisableData
            {
                disable_timer = authoring.disable_timer

            });
        }
    }
}
