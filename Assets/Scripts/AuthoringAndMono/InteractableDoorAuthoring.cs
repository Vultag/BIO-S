
using System.Numerics;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;


public partial struct InteractableDoorData : IComponentData
{
    public float starting_y;
}
public class InteractableDoorAuthoring : MonoBehaviour
{

    public float starting_y;

    class InteractableDoorBaker : Baker<InteractableDoorAuthoring>
    {
        public override void Bake(InteractableDoorAuthoring authoring)
        {

            AddComponent(new InteractableDoorData
            {

                starting_y = authoring.starting_y

            });
        }
    }
}