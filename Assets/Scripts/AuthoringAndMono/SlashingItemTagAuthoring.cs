
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public partial struct SlashingItemTag : IComponentData
{

}

public class SlashingItemTagAuthoring : MonoBehaviour
{


    public class SlashingItemTagBaker : Baker<SlashingItemTagAuthoring>
    {


        public override void Bake(SlashingItemTagAuthoring authoring)
        {

            AddComponent(new SlashingItemTag { 
            });

        }


    }
}
