using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;
using UnityEngine;
using Unity.Entities;

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
        transform.position = (Vector3)entityManager.GetComponentData<LocalToWorld>(target_entity).Position;
        //Debug.Log(transform.position);
        //Debug.Log((Vector3)entityManager.GetComponentData<LocalToWorld>(target_entity).Position);

        //Debug.Log((Vector3)entityManager.GetComponentData<LocalToWorld>(target_entity).Position);

        /*
        vr_balance_offset = camera_track.transform.position - camera_phy.transform.position;
        //vr_balance_offset = camera_track.transform.position - (Vector3)entityManager.GetComponentData<LocalToWorld>(target_entity).Position;
        //vr_balance_offset = Vector3.zero;

        //Debug.Log(vr_balance_offset);

        //deplacer le center du body pour qu il soit toujours relier au mouvemnt de camera

        //deplacer l origin sur le vr_balance mais avec un offest qui correspond a l offest de la camera, de sorte a ce qu il maintienne sa position par rapport a celle ci -> ca peux marcher ?

        //float4x4 follow_matrice = entityManager.GetComponentData<LocalToWorld>(target_entity).Value;
        Vector3 follow_position = entityManager.GetComponentData<LocalToWorld>(target_entity).Position;
        Quaternion follow_rotation = entityManager.GetComponentData<LocalToWorld>(target_entity).Rotation;


        //Vector3 follow_position = math.transform(math.inverse(follow_matrice), camera_vr.transform.position);

        //follow_position += vr_balance_offset;

        //je peux pt etre essayer de positionner ma capsule avant les calcules physiques

        //Vector3 offset = follow_position - camera_phy.transform.position;

        //Debug.Log(follow_position - camera_track.transform.position);

        //Debug.Log(follow_position - transform.InverseTransformPoint(camera_track.transform.position));

        Debug.Log(camera_offset.transform.InverseTransformPoint(camera_track.transform.position));
        Debug.Log(follow_position - transform.position);
        Debug.Log(starting_pos);



        ///transform.position = new Vector3((follow_position + vr_balance_offset - transform.InverseTransformPoint(camera_track.transform.position)).x, 0, (follow_position + vr_balance_offset - transform.InverseTransformPoint(camera_track.transform.position)).z); //follow_position - new Vector3(offset.x,0,offset.z);
        ///transform.position = new Vector3((follow_position + vr_balance_offset - camera_offset.transform.InverseTransformPoint(camera_track.transform.position)).x, 0, (follow_position + vr_balance_offset - camera_offset.transform.InverseTransformPoint(camera_track.transform.position)).z); //follow_position - new Vector3(offset.x,0,offset.z);
        transform.position = new Vector3((follow_position - camera_offset.transform.InverseTransformPoint(camera_track.transform.position)).x, transform.position.y, transform.position.z); //follow_position - new Vector3(offset.x,0,offset.z);

        ///transform.position = follow_position - transform.InverseTransformPoint(camera_vr.transform.position);
        //transform.position = new Vector3(follow_position.x,transform.position.y, follow_position.z);
        ///transform.rotation = follow_rotation;
        */

    }

}
