using UnityEngine;
using Unity.Entities;

[DisallowMultipleComponent]
public class FirstPersonPlayerAuthoring : MonoBehaviour
{
    public GameObject ControlledCharacter;
    public float MouseSensitivity = 1f;

    public class Baker : Baker<FirstPersonPlayerAuthoring>
    {
        public override void Bake(FirstPersonPlayerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new FirstPersonPlayer
            {
                ControlledCharacter = GetEntity(authoring.ControlledCharacter, TransformUsageFlags.Dynamic),
                MouseSensitivity = authoring.MouseSensitivity,
            });
            AddComponent(entity, new FirstPersonPlayerInputs());
        }
    }
}