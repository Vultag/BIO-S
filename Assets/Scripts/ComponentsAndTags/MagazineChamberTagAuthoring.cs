
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public partial struct MagazineChamberTag : IComponentData
{

}

public class MagazineChamberTagAuthoring : MonoBehaviour
{


    public class MagazineChamberTagBaker : Baker<MagazineChamberTagAuthoring>
    {


        public override void Bake(MagazineChamberTagAuthoring authoring)
        {

            AddComponent(new MagazineChamberTag
            {
            });

        }


    }
}
