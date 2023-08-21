
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public partial struct MagazineTag : IComponentData
{

}

public class MagazineTagAuthoring : MonoBehaviour
{


    public class MagazineTagBaker : Baker<MagazineTagAuthoring>
    {


        public override void Bake(MagazineTagAuthoring authoring)
        {

            AddComponent(new MagazineTag
            {
            });

        }


    }
}
