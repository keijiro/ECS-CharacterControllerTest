using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Authoring;
using UnityEngine;
using Unity.CharacterController;
using Unity.Physics;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
public class ThirdPersonCharacterAuthoring : MonoBehaviour
{
    public AuthoringKinematicCharacterProperties CharacterProperties = AuthoringKinematicCharacterProperties.GetDefault();
    public ThirdPersonCharacterComponent Character = ThirdPersonCharacterComponent.GetDefault();

    public class Baker : Baker<ThirdPersonCharacterAuthoring>
    {
        public override void Bake(ThirdPersonCharacterAuthoring authoring)
        {
            KinematicCharacterUtilities.BakeCharacter(this, authoring, authoring.CharacterProperties);

            Entity entity = GetEntity(TransformUsageFlags.Dynamic | TransformUsageFlags.WorldSpace);

            AddComponent(entity, authoring.Character);
            AddComponent(entity, new ThirdPersonCharacterControl());
        }
    }

}
