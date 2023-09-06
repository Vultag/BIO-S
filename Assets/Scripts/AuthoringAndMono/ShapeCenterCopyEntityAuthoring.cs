using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using CapsuleCollider = Unity.Physics.CapsuleCollider;



public struct ShapeCenterCopyEntity : IComponentData
{
    public Vector3 offset;
    public Entity self;
    public Entity target_entity;
}

public class ShapeCenterCopyEntityAuthoring : MonoBehaviour
{

    public GameObject self;
    public GameObject Target_entity;

}

public class ShapeCenterCopyEntityBaker : Baker<ShapeCenterCopyEntityAuthoring>
{

    public override void Bake(ShapeCenterCopyEntityAuthoring authoring)
    {

        AddComponent(new ShapeCenterCopyEntity
        {

            self = GetEntity(authoring.self),
            target_entity = GetEntity(authoring.Target_entity)

        });
    }
}

[RequireMatchingQueriesForUpdate]
//[UpdateInGroup(typeof(LateSimulationSystemGroup))]
//[UpdateInGroup(typeof(AfterPhysicsSystemGroup))]
public partial class ShapeCenterCopyEntitySystem : SystemBase
{


    private EntityManager entityManager;


    protected override void OnCreate()
    {
        entityManager = World.EntityManager;


    }



    protected override void OnStartRunning()
    {

    }


    protected override void OnUpdate()
    {


        foreach (var (ShapeCenterCopyEntity,collider) in SystemAPI.Query<RefRW<ShapeCenterCopyEntity>,RefRW<PhysicsCollider>>())
        {

            float3 camera = entityManager.GetComponentData<LocalToWorld>(ShapeCenterCopyEntity.ValueRO.target_entity).Position;
            Vector3 offset =  camera - entityManager.GetComponentData<LocalToWorld>(ShapeCenterCopyEntity.ValueRO.self).Position;





            unsafe
            {
                CapsuleCollider* colliderptr = (CapsuleCollider*)collider.ValueRW.ColliderPtr;

                var new_geometry = colliderptr->Geometry;
                
                new_geometry.Vertex0 = new Vector3(offset.x, new_geometry.Vertex0.y, offset.z);
                new_geometry.Vertex1 = new Vector3(offset.x, new_geometry.Vertex0.y + offset.y, offset.z);
                //Debug.Log(new_geometry.Vertex0.y);
                //Debug.Log(new_geometry.Vertex0.y + offset.y);
                //new_geometry.Vertex0 = new Vector3(new_geometry.Vertex0.x, new_geometry.Vertex0.y, new_geometry.Vertex0.z);
                //new_geometry.Vertex1 = new Vector3(new_geometry.Vertex1.x, offset.y, new_geometry.Vertex1.z);

                colliderptr->Geometry = new_geometry;
                
                /// ABANDONE 
                /// INITALEMENT PREVU POUR GERER LES MOUVEMENT DYNAMIQUE DU JOUEUR EN ADAPTANT LES PROPORTION D UNE CAPSULE A SA POSITION
                /// TROUVE UN MEILLEUR MOYEN ?
                /// NON...
                /// REPRIS
            }
        }


    }
}
