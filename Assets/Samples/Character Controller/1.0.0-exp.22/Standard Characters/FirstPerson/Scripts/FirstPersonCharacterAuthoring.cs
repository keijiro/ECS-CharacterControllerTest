using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Authoring;
using UnityEngine;
using Unity.CharacterController;
using Unity.Physics;
using System.Collections.Generic;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
public class FirstPersonCharacterAuthoring : MonoBehaviour
{
    public GameObject ViewEntity;
    public AuthoringKinematicCharacterProperties CharacterProperties = AuthoringKinematicCharacterProperties.GetDefault();
    public FirstPersonCharacterComponent Character = FirstPersonCharacterComponent.GetDefault();

    public class Baker : Baker<FirstPersonCharacterAuthoring>
    {
        public override void Bake(FirstPersonCharacterAuthoring authoring)
        {
            KinematicCharacterUtilities.BakeCharacter(this, authoring, authoring.CharacterProperties);

            authoring.Character.ViewEntity = GetEntity(authoring.ViewEntity, TransformUsageFlags.Dynamic);

            Entity entity = GetEntity(TransformUsageFlags.Dynamic | TransformUsageFlags.WorldSpace);

            AddComponent(entity, authoring.Character);
            AddComponent(entity, new FirstPersonCharacterControl());
        }
    }
}
