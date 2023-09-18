using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;
using UnityEngine;
using Unity.Entities;
using TMPro;

public class CopyTransformFromRootTag : MonoBehaviour
{


    private Entity target_entity;
    private EntityManager entityManager;
    [SerializeField]
    private GameObject camera_phy;
    [SerializeField]
    private GameObject camera_track;
    [SerializeField]
    private GameObject camera_offset;


    Vector3 vr_balance_offset;

    // This value will change at the runtime depending on target movement. Initialize with zero vector.
    private Vector3 velocity = Vector3.zero;

    //temp
    Vector3 starting_pos;

    private void Awake()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;


    }

    private Entity GetRootTrackingEntity()
    {

        EntityQuery Head_query = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(PlayerRootTag));

        NativeArray<Entity> entitynativearray = Head_query.ToEntityArray(Unity.Collections.Allocator.TempJob);

        return entitynativearray[0];

    }

    private void Start()
    {

        target_entity = GetRootTrackingEntity();

        //temp
        starting_pos = transform.position;


        //vr balance offset from camera
        vr_balance_offset = camera_track.transform.position - (Vector3)entityManager.GetComponentData<LocalToWorld>(target_entity).Position;

        //Debug.Log(vr_balance_offset);
    }

    private void LateUpdate()
    {

        // a retirer ?
        target_entity = GetRootTrackingEntity();
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        ///j ai pas compris a quoi ca sert -> track le charControl pour le VR root
        transform.position = Vector3.SmoothDamp(transform.position, (Vector3)entityManager.GetComponentData<LocalToWorld>(target_entity).Position, ref velocity, 0.3f);

    }

}
