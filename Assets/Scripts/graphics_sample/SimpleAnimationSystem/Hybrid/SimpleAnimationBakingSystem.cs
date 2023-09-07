using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Hybrid.Baking;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Unity.Burst;

///[TemporaryBakingType]
//[WorldSystemFilter(WorldSystemFilterFlags.ProcessAfterLoad)]
//[BakingType]

internal struct DeformationSampleColor : IComponentData
{
    public float4 Value;
}

internal class DeformationSampleBaker : Baker<DeformationsSampleAuthoring>
{
    public override void Bake(DeformationsSampleAuthoring authoring)
    {


        var skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>(authoring);
        if (skinnedMeshRenderer == null)
            return;


        var c = authoring.Color.linear;
        var color = new float4(c.r, c.g, c.b, c.a);
        AddComponent(new DeformationSampleColor{Value = color});

        //skinnedMeshRenderer.BakeMesh(skinnedMeshRenderer.sharedMesh);

        // Only execute this if we have a valid skinning setup
        DependsOn(skinnedMeshRenderer.sharedMesh);
        var hasSkinning = skinnedMeshRenderer.bones.Length > 0 && skinnedMeshRenderer.sharedMesh.bindposes.Length > 0;
        if (hasSkinning)
        {
            // Setup reference to the root bone
            var rootTransform = skinnedMeshRenderer.rootBone ? skinnedMeshRenderer.rootBone : skinnedMeshRenderer.transform;
            var rootEntity = GetEntity(rootTransform);
            AddComponent(new RootEntity {Value = rootEntity});

            // Setup reference to the other bones
            var boneEntityArray = AddBuffer<BoneEntity>();
            boneEntityArray.ResizeUninitialized(skinnedMeshRenderer.bones.Length);

            for (int boneIndex = 0; boneIndex < skinnedMeshRenderer.bones.Length; ++boneIndex)
            {
                var bone = skinnedMeshRenderer.bones[boneIndex];
                var boneEntity = GetEntity(bone);
                boneEntityArray[boneIndex] = new BoneEntity {Value = boneEntity};
            }

            // Store the bindpose for each bone
            var bindPoseArray = AddBuffer<BindPose>();
            bindPoseArray.ResizeUninitialized(skinnedMeshRenderer.bones.Length);

            for (int boneIndex = 0; boneIndex != skinnedMeshRenderer.bones.Length; ++boneIndex)
            {
                var bindPose = skinnedMeshRenderer.sharedMesh.bindposes[boneIndex];
                bindPoseArray[boneIndex] = new BindPose { Value = bindPose };
            }
        }

    }
}

[WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
//Update toutes les frames -> tres mauvais mais je peux pas instancier les skin sans ca -> a revoir.
///[WorldSystemFilter(WorldSystemFilterFlags.Default)]

//[RequireMatchingQueriesForUpdate]

//[UpdateAfter(typeof(FixedStepSimulationSystemGroup))]

public partial struct ComputeSkinMatricesBakingSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

        var ecb = new EntityCommandBuffer(Allocator.TempJob);

        // This is only executed if we have a valid skinning setup
        foreach ( var (rootEntity,bones,entity) in SystemAPI.Query <RefRO<RootEntity>,DynamicBuffer<BoneEntity>>().WithAll<DeformationSampleColor>().WithEntityAccess().WithOptions(EntityQueryOptions.IncludeDisabledEntities|EntityQueryOptions.IncludePrefab))
        {
            // World to local is required for root space conversion of the SkinMatrices
            ecb.AddComponent<WorldToLocal>(rootEntity.ValueRO.Value);
            ecb.AddComponent<RootTag>(rootEntity.ValueRO.Value);

            // Add tags to the bones so we can find them later
            // when computing the SkinMatrices
            for (int boneIndex = 0; boneIndex < bones.Length; ++boneIndex)
            {
                var boneEntity = bones[boneIndex].Value;
                ecb.AddComponent(boneEntity, new BoneTag());
            }
        }

        foreach (var (deformColor, additionalEntities, entity) in SystemAPI.Query<RefRO<DeformationSampleColor>, DynamicBuffer<AdditionalEntitiesBakingData>>().WithEntityAccess().WithOptions(EntityQueryOptions.IncludeDisabledEntities | EntityQueryOptions.IncludePrefab))
        {
            // Override the material color of the deformation materials
            foreach (var rendererEntity in additionalEntities.AsNativeArray())
            {
                if (state.EntityManager.HasComponent<RenderMesh>(rendererEntity.Value))
                {
                    ecb.AddComponent(rendererEntity.Value, new URPMaterialPropertyBaseColor { Value = deformColor.ValueRO.Value });
                }
            }

        }


        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }


    public void OnCreate(ref SystemState state)
    {

    }
    public void OnDestroy(ref SystemState state)
    {

    }

}



class AnimatePositionBaker : Baker<AnimatePositionAuthoring>
{
    public override void Bake(AnimatePositionAuthoring authoring)
    {
        var positionAnimation = new AnimatePosition
        {
            From = authoring.FromPosition,
            To = authoring.ToPosition,
            Frequency = 1f / authoring.Phase,
            PhaseShift = authoring.Offset * authoring.Phase,
        };

        AddComponent(positionAnimation);
        AddComponent<Translation>();
    }
}

class AnimateRotationBaker : Baker<AnimateRotationAuthoring>
{
    public override void Bake(AnimateRotationAuthoring authoring)
    {
        var rotationAnimation = new AnimateRotation
        {
            From = quaternion.Euler(math.radians(authoring.FromRotation)),
            To = quaternion.Euler(math.radians(authoring.ToRotation)),
            Frequency = 1f / authoring.Phase,
            PhaseShift = authoring.Offset * authoring.Phase,
        };
        AddComponent(rotationAnimation);
        AddComponent<Rotation>();
    }
}

class AnimateScaleBaker : Baker<AnimateScaleAuthoring>
{
    public override void Bake(AnimateScaleAuthoring authoring)
    {
        var scaleAnimation = new AnimateScale
        {
            From = authoring.FromScale,
            To = authoring.ToScale,
            Frequency = 1f / authoring.Phase,
            PhaseShift = authoring.Offset * authoring.Phase,
        };

        AddComponent(scaleAnimation);
        AddComponent<NonUniformScale>();
    }
}

class AnimateBlendShapeBaker : Baker<AnimateBlendShapeAuthoring>
{
    public override void Bake(AnimateBlendShapeAuthoring authoring)
    {
        var blendshapeAnimation = new AnimateBlendShape
        {
            From = authoring.FromWeight,
            To = authoring.ToWeight,
            Frequency = 1f / authoring.Phase,
            PhaseShift = authoring.Offset * authoring.Phase,
        };

        AddComponent(blendshapeAnimation);
    }
}
