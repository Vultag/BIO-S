using Unity.Entities;
using UnityEngine;
using Unity.Jobs;
using Unity.Physics;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine.UI;
using Unity.Physics.Extensions;
using System.Numerics;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
using Unity.VisualScripting;
using Unity.Physics.Systems;
using Unity.Rendering;
using Unity.Entities.UniversalDelegates;
using Matrix4x4 = UnityEngine.Matrix4x4;

///[AlwaysSynchronizeSystem]

//POUR ENABLE TRANSFORM V1
///[UpdateInGroup(typeof(AfterPhysicsSystemGroup))]
///[UpdateInGroup(typeof(AfterPhysicsSystemGroup))]

//[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]

public partial class FootTargetPosSystem : SystemBase
{


    private EntityQuery query;

    private EntityManager entityManager;

    private NativeList<Unity.Physics.RaycastHit> rayResults;
    private NativeList<RaycastInput> rayCommands;
    private NativeList<Unity.Physics.RaycastHit> raycasthit_result;



    protected override void OnCreate()
    {
        entityManager = World.EntityManager;

      
        raycasthit_result = new NativeList<Unity.Physics.RaycastHit>(Allocator.Persistent);
        rayResults = new NativeList<Unity.Physics.RaycastHit>(Allocator.Persistent);
        rayCommands = new NativeList<RaycastInput>(Allocator.Persistent);


    }

    public struct RaycastJob : IJobParallelFor
    {
        [ReadOnly] public CollisionWorld world;
        [ReadOnly] public NativeArray<RaycastInput> inputs;
        public NativeArray<Unity.Physics.RaycastHit> results;

        public unsafe void Execute(int index)
        {
            Unity.Physics.RaycastHit hit;
            world.CastRay(inputs[index], out hit);
            results[index] = hit;
        }
    }

    public static JobHandle ScheduleBatchRayCast(CollisionWorld world,
    NativeArray<RaycastInput> inputs, NativeArray<Unity.Physics.RaycastHit> results, JobHandle jobhandle)
    {
        JobHandle rcj = new RaycastJob
        {
            inputs = inputs,
            results = results,
            world = world

        }.Schedule(inputs.Length, 4, jobhandle);
        return rcj;
    }
    protected override void OnUpdate()
    {


        PhysicsWorldSingleton phy_world = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        query = EntityManager.CreateEntityQuery(typeof(FootTargetPosData));
        int datacount = query.CalculateEntityCount();


        raycasthit_result.Clear();
        rayCommands.Clear();
        rayResults.Clear();

        raycasthit_result.Resize(datacount, NativeArrayOptions.UninitializedMemory);
        rayResults.Resize(datacount, NativeArrayOptions.UninitializedMemory);
        rayCommands.Resize(datacount,NativeArrayOptions.UninitializedMemory); //(datacount);

        var raycasthit_result_local = raycasthit_result.AsArray();
        var rayResults_local = rayResults.AsArray();
        var rayCommands_local = rayCommands.AsArray();

        this.Dependency = Entities
            .WithAll<LocalToWorld>()
        .ForEach((Entity entity, int entityInQueryIndex, ref FootTargetPosData targetdata) =>
        {

            var ltw = SystemAPI.GetComponent<LocalToWorld>(entity);

        //if (targetdata.foot_walking == false)
        {

            float3 target_reference_pos = SystemAPI.GetComponent<LocalToWorld>(targetdata.zombie_target_pos_entity).Position + (float3)((Quaternion)SystemAPI.GetComponent<LocalToWorld>(targetdata.zombie_target_pos_entity).Rotation * (float3)targetdata.target_pos_reference_point_offset);//entityManager.GetComponentData<Translation>(targetdata.zombie_target_pos_entity).Value + (float3)targetdata.target_pos_reference_point_offset;

            //Debug.DrawLine(entityManager.GetComponentData<LocalToWorld>(targetdata.zombie_target_pos_entity).Position, target_reference_pos, Color.cyan);

                var raycastinputs = new RaycastInput
                {
                    Start = ltw.Position, //le +f est pour que de base, les pieds visent sous le so
                    End = target_reference_pos,// + target_reference_pos,//(Vector3)target_reference_pos,
                    Filter = new CollisionFilter
                    {
                        BelongsTo = 1,
                        CollidesWith = 1,
                        GroupIndex = 0,
                    }
                };


                rayCommands_local[entityInQueryIndex] = raycastinputs;
            }

            SystemAPI.SetComponent<LocalToWorld>(entity, ltw);

        }).Schedule(this.Dependency);


        var handle = ScheduleBatchRayCast(phy_world.CollisionWorld, rayCommands_local, rayResults_local, this.Dependency);


        /*
        for (int i = 0; i < rayCommands.Length; i++)
        {

            //Debug.DrawLine(rayCommands[i].Start, rayCommands[i].End,Color.red);

        }
        */

        this.Dependency = Entities
            .WithReadOnly(rayCommands_local)
            .WithAll<FootTargetPosData>()
        .ForEach((Entity entity, int entityInQueryIndex, ref TransformAspect trans) =>
        {
            var targetdata = SystemAPI.GetComponent<FootTargetPosData>(entity);

            if (SystemAPI.GetComponent<FootTargetPosData>(targetdata.other_foot_target).is_grounded)
            {


                if (targetdata.foot_walking == false)
                {
                    float trigger_walk_distance = 0.2f; //A METTRE EN PUBLIC

                    ///var pos_diff = rayCommands[entityInQueryIndex].End.xz - rayCommands[entityInQueryIndex].Start.xz;
                    var pos_diff = rayCommands_local[entityInQueryIndex].End - rayCommands_local[entityInQueryIndex].Start;

                    var pythagore = math.abs(math.sqrt((math.pow(pos_diff.x, 2) + (math.pow(pos_diff.y, 2)))));

                    if (pythagore > trigger_walk_distance)
                    {
                        targetdata.is_grounded = false;
                        targetdata.interpol_start = trans.WorldPosition;

                        targetdata.interpol_end = new float3(rayCommands_local[entityInQueryIndex].End.x, rayCommands_local[entityInQueryIndex].End.y, rayCommands_local[entityInQueryIndex].End.z);

                        targetdata.foot_walking = true;
                    }
                }
                else
                {


                    targetdata.interpolate_amount += SystemAPI.Time.DeltaTime * 1.5f;

                    if (Vector3.Distance(trans.WorldPosition, targetdata.interpol_end) > 0.1f) // 0.1 initialement a 0 mais a tendence a se block 
                    {

                        var torso_physics = SystemAPI.GetComponent<PhysicsVelocity>(SystemAPI.GetComponent<ZombieHipsReferencePointData>(targetdata.zombie_target_pos_entity).ZombieTorsoEntity); //GetComponentLookup<PhysicsVelocity>(true);
                        //var torso_physic_data = torso_physics[entityManager.GetComponentData<ZombieHipsReferencePointData>(targetdata.zombie_target_pos_entity).ZombieTorsoEntity];

                        Vector3 horizontal_torso_vel_direction = (new Vector3(torso_physics.Linear.x, 0, torso_physics.Linear.z));

                        targetdata.interpol_end = new float3(rayCommands_local[entityInQueryIndex].End.x, rayCommands_local[entityInQueryIndex].End.y, rayCommands_local[entityInQueryIndex].End.z);
                        targetdata.interpol_end += horizontal_torso_vel_direction * 0.3f;


                        var point_AB = Vector3.Lerp(targetdata.interpol_start, Vector3.Lerp(targetdata.interpol_start, targetdata.interpol_end, 0.5f) + Vector3.up, targetdata.interpolate_amount);
                        var point_BC = Vector3.Lerp(Vector3.Lerp(targetdata.interpol_start, targetdata.interpol_end, 0.5f) + Vector3.up, targetdata.interpol_end, targetdata.interpolate_amount);
                        trans.WorldPosition = Vector3.Lerp(point_AB, point_BC, targetdata.interpolate_amount);
                    }
                    else
                    {
                        targetdata.is_grounded = true;
                        trans.WorldPosition = targetdata.interpol_end;
                        targetdata.foot_walking = false;
                        targetdata.interpolate_amount = 0;
                    }

                    //if "marche" activee -> les pieds doivent prendre de l avance
                }


                SystemAPI.SetComponent<FootTargetPosData>(entity, targetdata);

            }



        }).Schedule(handle);//.Run();
        
        
    }
}
