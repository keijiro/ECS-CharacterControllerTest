using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.CharacterController;

[Serializable]
public struct FirstPersonCharacterComponent : IComponentData
{
    public float GroundMaxSpeed;
    public float GroundedMovementSharpness;
    public float AirAcceleration;
    public float AirMaxSpeed;
    public float AirDrag;
    public float JumpSpeed;
    public float3 Gravity;
    public bool PreventAirAccelerationAgainstUngroundedHits;
    public BasicStepAndSlopeHandlingParameters StepAndSlopeHandling;

    public float MinViewAngle;
    public float MaxViewAngle;
    [HideInInspector]
    public Entity ViewEntity;
    [HideInInspector]
    public float ViewPitchDegrees;
    [HideInInspector]
    public quaternion ViewLocalRotation;

    public static FirstPersonCharacterComponent GetDefault()
    {
        return new FirstPersonCharacterComponent
        {
            GroundMaxSpeed = 10f,
            GroundedMovementSharpness = 15f,
            AirAcceleration = 50f,
            AirMaxSpeed = 10f,
            AirDrag = 0f,
            JumpSpeed = 10f,
            Gravity = math.up() * -30f,
            PreventAirAccelerationAgainstUngroundedHits = true,
            StepAndSlopeHandling = BasicStepAndSlopeHandlingParameters.GetDefault(),

            MinViewAngle = -90f,
            MaxViewAngle = 90f,
        };
    }
}

[Serializable]
public struct FirstPersonCharacterControl : IComponentData
{
    public float3 MoveVector;
    public float2 LookYawPitchDegrees;
    public bool Jump;
}

[Serializable]
public struct FirstPersonCharacterView : IComponentData
{
    public Entity CharacterEntity;
}
