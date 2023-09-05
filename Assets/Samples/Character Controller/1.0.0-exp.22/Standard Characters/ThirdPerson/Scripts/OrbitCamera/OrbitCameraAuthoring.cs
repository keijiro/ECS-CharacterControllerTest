using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
public class OrbitCameraAuthoring : MonoBehaviour
{
    public List<GameObject> IgnoredEntities = new List<GameObject>();
    public OrbitCamera OrbitCamera = OrbitCamera.GetDefault();

    public class Baker : Baker<OrbitCameraAuthoring>
    {
        public override void Bake(OrbitCameraAuthoring authoring)
        {
            authoring.OrbitCamera.CurrentDistanceFromMovement = authoring.OrbitCamera.TargetDistance;
            authoring.OrbitCamera.CurrentDistanceFromObstruction = authoring.OrbitCamera.TargetDistance;
            authoring.OrbitCamera.PlanarForward = -math.forward();

            Entity entity = GetEntity(TransformUsageFlags.Dynamic | TransformUsageFlags.WorldSpace);
            
            AddComponent(entity, authoring.OrbitCamera);
            AddComponent(entity, new OrbitCameraControl());
            DynamicBuffer<OrbitCameraIgnoredEntityBufferElement> ignoredEntitiesBuffer = AddBuffer<OrbitCameraIgnoredEntityBufferElement>(entity);

            for (int i = 0; i < authoring.IgnoredEntities.Count; i++)
            {
                ignoredEntitiesBuffer.Add(new OrbitCameraIgnoredEntityBufferElement
                {
                    Entity = GetEntity(authoring.IgnoredEntities[i], TransformUsageFlags.None),
                });
            }
        }
    }
}