using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics.Systems;
using Unity.CharacterController;
using UnityEngine.SocialPlatforms;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class FirstPersonPlayerInputsSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<FixedTickSystem.Singleton>();
        RequireForUpdate(SystemAPI.QueryBuilder().WithAll<FirstPersonPlayer, FirstPersonPlayerInputs>().Build());
    }
    
    protected override void OnUpdate()
    {
        uint fixedTick = SystemAPI.GetSingleton<FixedTickSystem.Singleton>().Tick;

        foreach (var (playerInputs, player) in SystemAPI.Query<RefRW<FirstPersonPlayerInputs>, FirstPersonPlayer>())
        {
            playerInputs.ValueRW.MoveInput =  new float2();
            playerInputs.ValueRW.MoveInput.y += Input.GetKey(KeyCode.W) ? 1f : 0f;
            playerInputs.ValueRW.MoveInput.y += Input.GetKey(KeyCode.S) ? -1f : 0f;
            playerInputs.ValueRW.MoveInput.x += Input.GetKey(KeyCode.D) ? 1f : 0f;
            playerInputs.ValueRW.MoveInput.x += Input.GetKey(KeyCode.A) ? -1f : 0f;
            
            playerInputs.ValueRW.LookInput = new float2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            
            // For button presses that need to be queried during fixed update, use the "FixedInputEvent" helper struct.
            // This is part of a strategy for proper handling of button press events that are consumed during the fixed update group
            if (Input.GetKeyDown(KeyCode.Space))
            {
                playerInputs.ValueRW.JumpPressed.Set(fixedTick);
            }
        }
    }
}

/// <summary>
/// Apply inputs that need to be read at a variable rate
/// </summary>
[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
[UpdateBefore(typeof(FixedStepSimulationSystemGroup))]
[BurstCompile]
public partial struct FirstPersonPlayerVariableStepControlSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<FirstPersonPlayer, FirstPersonPlayerInputs>().Build());
    }
    
    public void OnDestroy(ref SystemState state)
    { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (playerInputs, player) in SystemAPI.Query<FirstPersonPlayerInputs, FirstPersonPlayer>().WithAll<Simulate>())
        {
            if (SystemAPI.HasComponent<FirstPersonCharacterControl>(player.ControlledCharacter))
            {
                FirstPersonCharacterControl characterControl = SystemAPI.GetComponent<FirstPersonCharacterControl>(player.ControlledCharacter);
                
                // Look
                characterControl.LookYawPitchDegrees = playerInputs.LookInput * player.MouseSensitivity;
            
                SystemAPI.SetComponent(player.ControlledCharacter, characterControl);
            }
        }
    }
}

/// <summary>
/// Apply inputs that need to be read at a fixed rate.
/// It is necessary to handle this as part of the fixed step group, in case your framerate is lower than the fixed step rate.
/// </summary>
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup), OrderFirst = true)]
[BurstCompile]
public partial struct FirstPersonPlayerFixedStepControlSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<FixedTickSystem.Singleton>();
        state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<FirstPersonPlayer, FirstPersonPlayerInputs>().Build());
    }

    public void OnDestroy(ref SystemState state)
    { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        uint fixedTick = SystemAPI.GetSingleton<FixedTickSystem.Singleton>().Tick;
        
        foreach (var (playerInputs, player) in SystemAPI.Query<RefRW<FirstPersonPlayerInputs>, FirstPersonPlayer>().WithAll<Simulate>())
        {
            if (SystemAPI.HasComponent<FirstPersonCharacterControl>(player.ControlledCharacter))
            {
                FirstPersonCharacterControl characterControl = SystemAPI.GetComponent<FirstPersonCharacterControl>(player.ControlledCharacter);
                
                quaternion characterRotation = SystemAPI.GetComponent<LocalTransform>(player.ControlledCharacter).Rotation;

                // Move
                float3 characterForward = MathUtilities.GetForwardFromRotation(characterRotation);
                float3 characterRight = MathUtilities.GetRightFromRotation(characterRotation);
                characterControl.MoveVector = (playerInputs.ValueRW.MoveInput.y * characterForward) + (playerInputs.ValueRW.MoveInput.x * characterRight);
                characterControl.MoveVector = MathUtilities.ClampToMaxLength(characterControl.MoveVector, 1f);

                // Jump
                // We use the "FixedInputEvent" helper struct here to detect if the event needs to be processed.
                // This is part of a strategy for proper handling of button press events that are consumed during the fixed update group.
                characterControl.Jump = playerInputs.ValueRW.JumpPressed.IsSet(fixedTick);
            
                SystemAPI.SetComponent(player.ControlledCharacter, characterControl);
            }
        }
    }
}