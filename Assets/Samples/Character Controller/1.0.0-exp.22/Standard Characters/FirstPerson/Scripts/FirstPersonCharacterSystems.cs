using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Unity.CharacterController;
using Unity.Burst.Intrinsics;

[UpdateInGroup(typeof(KinematicCharacterPhysicsUpdateGroup))]
[BurstCompile]
public partial struct FirstPersonCharacterPhysicsUpdateSystem : ISystem
{
    private EntityQuery _characterQuery;
    private FirstPersonCharacterUpdateContext _context;
    private KinematicCharacterUpdateContext _baseContext;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _characterQuery = KinematicCharacterUtilities.GetBaseCharacterQueryBuilder()
            .WithAll<
                FirstPersonCharacterComponent,
                FirstPersonCharacterControl>()
            .Build(ref state);

        _context = new FirstPersonCharacterUpdateContext();
        _context.OnSystemCreate(ref state);
        _baseContext = new KinematicCharacterUpdateContext();
        _baseContext.OnSystemCreate(ref state);

        state.RequireForUpdate(_characterQuery);
        state.RequireForUpdate<PhysicsWorldSingleton>();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _context.OnSystemUpdate(ref state);
        _baseContext.OnSystemUpdate(ref state, SystemAPI.Time, SystemAPI.GetSingleton<PhysicsWorldSingleton>());

        FirstPersonCharacterPhysicsUpdateJob job = new FirstPersonCharacterPhysicsUpdateJob
        {
            Context = _context,
            BaseContext = _baseContext,
        };
        job.ScheduleParallel();
    }

    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct FirstPersonCharacterPhysicsUpdateJob : IJobEntity, IJobEntityChunkBeginEnd
    {
        public FirstPersonCharacterUpdateContext Context;
        public KinematicCharacterUpdateContext BaseContext;

        void Execute(FirstPersonCharacterAspect characterAspect)
        {
            characterAspect.PhysicsUpdate(ref Context, ref BaseContext);
        }

        public bool OnChunkBegin(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            BaseContext.EnsureCreationOfTmpCollections();
            return true;
        }

        public void OnChunkEnd(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask, bool chunkWasExecuted)
        {
        }
    }
}

[UpdateInGroup(typeof(KinematicCharacterVariableUpdateGroup))]
[BurstCompile]
public partial struct FirstPersonCharacterVariableUpdateSystem : ISystem
{
    private EntityQuery _characterQuery;
    private FirstPersonCharacterUpdateContext _context;
    private KinematicCharacterUpdateContext _baseContext;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _characterQuery = KinematicCharacterUtilities.GetBaseCharacterQueryBuilder()
            .WithAll<
                FirstPersonCharacterComponent,
                FirstPersonCharacterControl>()
            .Build(ref state);

        _context = new FirstPersonCharacterUpdateContext();
        _context.OnSystemCreate(ref state);
        _baseContext = new KinematicCharacterUpdateContext();
        _baseContext.OnSystemCreate(ref state);

        state.RequireForUpdate(_characterQuery);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _context.OnSystemUpdate(ref state);
        _baseContext.OnSystemUpdate(ref state, SystemAPI.Time, SystemAPI.GetSingleton<PhysicsWorldSingleton>());

        FirstPersonCharacterVariableUpdateJob variableUpdateJob = new FirstPersonCharacterVariableUpdateJob
        {
            Context = _context,
            BaseContext = _baseContext,
        };
        variableUpdateJob.ScheduleParallel();

        FirstPersonCharacterViewJob viewJob = new FirstPersonCharacterViewJob
        {
            FirstPersonCharacterLookup = SystemAPI.GetComponentLookup<FirstPersonCharacterComponent>(true),
        };
        viewJob.ScheduleParallel();
    }

    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct FirstPersonCharacterVariableUpdateJob : IJobEntity, IJobEntityChunkBeginEnd
    {
        public FirstPersonCharacterUpdateContext Context;
        public KinematicCharacterUpdateContext BaseContext;

        void Execute(FirstPersonCharacterAspect characterAspect)
        {
            characterAspect.VariableUpdate(ref Context, ref BaseContext);
        }

        public bool OnChunkBegin(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            BaseContext.EnsureCreationOfTmpCollections();
            return true;
        }

        public void OnChunkEnd(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask, bool chunkWasExecuted)
        { }
    }

    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct FirstPersonCharacterViewJob : IJobEntity
    {
        [ReadOnly]
        public ComponentLookup<FirstPersonCharacterComponent> FirstPersonCharacterLookup;

        void Execute(ref LocalTransform localTransform, in FirstPersonCharacterView characterView)
        {
            if (FirstPersonCharacterLookup.TryGetComponent(characterView.CharacterEntity, out FirstPersonCharacterComponent character))
            {
                localTransform.Rotation = character.ViewLocalRotation;
            }
        }
    }
}
