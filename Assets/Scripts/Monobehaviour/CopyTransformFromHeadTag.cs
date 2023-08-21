using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Transforms;
using UnityEngine;
using Unity.Entities;

public class CopyTransformFromHeadTag : MonoBehaviour
{


    private Entity target_entity;
    private EntityManager entityManager;

    private void Awake()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;


    }

    private Entity GetCameraTrackingEntity()
    {

        EntityQuery Head_query = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(PlayerHeadTag));

        NativeArray<Entity> entitynativearray = Head_query.ToEntityArray(Unity.Collections.Allocator.TempJob);

        return entitynativearray[0];

    }

    private void Start()
    {

        target_entity = GetCameraTrackingEntity();
        
    }

    private void LateUpdate()
    {

        //pas opti mais marche pas dans le build sinon
        target_entity = GetCameraTrackingEntity();


        Vector3 follow_position = entityManager.GetComponentData<LocalToWorld>(target_entity).Position;
        Quaternion follow_rotation = entityManager.GetComponentData<LocalToWorld>(target_entity).Rotation;

        //Debug.LogError(follow_position);

        transform.position = follow_position;
        transform.rotation = follow_rotation;


    }

}
