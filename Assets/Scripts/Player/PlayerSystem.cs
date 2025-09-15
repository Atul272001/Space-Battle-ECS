using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public partial struct PlayerSystem : ISystem
{
    EntityManager entityManager;

    Entity playerEntity;
    Entity inputEntity;

    PlayerComponent playerComponent;
    InputComponent inputComponent;

    public void OnUpdate(ref SystemState state)
    {
        entityManager = state.EntityManager;
        if (!SystemAPI.HasSingleton<PlayerComponent>())
        {
            return;
        }
        playerEntity = SystemAPI.GetSingletonEntity<PlayerComponent>();
        inputEntity = SystemAPI.GetSingletonEntity<InputComponent>();

        playerComponent = entityManager.GetComponentData<PlayerComponent>(playerEntity);
        inputComponent = entityManager.GetComponentData<InputComponent>(inputEntity);

        Move(ref state);
        Shoot(ref state);
        PauseGame();
        DamageHealthDamage();
    }

    private void PauseGame()
    {
        if (inputComponent.PauseGame)
        {
            UIHandler.Instance.PauseGame();
        }
    }

    private void DamageHealthDamage()
    {
        LocalTransform playerTransform = entityManager.GetComponentData<LocalTransform>(playerEntity);
        PhysicsWorldSingleton physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        float3 up = playerTransform.Up();
        float totalHeight = 2f;
        float capsuleHalfHeight = totalHeight * 0.5f;
        float radius = 0.5f;
        float inner = math.max(0f, capsuleHalfHeight - radius);

        float3 p1 = playerTransform.Position + up * inner;
        float3 p2 = playerTransform.Position - up * inner;

        NativeList<ColliderCastHit> hits = new NativeList<ColliderCastHit>(Allocator.Temp);

        var filter = new CollisionFilter
        {
            BelongsTo = (uint)CollisionLayer.Default,
            CollidesWith = LayerMaskHelper.GetLayerMaskFromLayers(CollisionLayer.Enemy)
        };

        physicsWorld.CapsuleCastAll(p1, p2, radius, new float3(0, 0, 0), 0f, ref hits, filter);

        if(hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                Entity hitEntity = hits[i].Entity;
                if (entityManager.HasComponent<EnemyComponent>(hitEntity))
                {
                    playerComponent.Health -= 20;
                    UIHandler.Instance.UpdatePlayerHealth((float)playerComponent.Health);
                    entityManager.SetComponentData(playerEntity, playerComponent);
                    entityManager.DestroyEntity(hitEntity);

                    if (playerComponent.Health <= 0f)
                    {
                        UIHandler.Instance.GameOverPannel();
                        entityManager.DestroyEntity(playerEntity);
                    }
                }
            }
        }

        hits.Dispose();
    }

    private void Move(ref SystemState state)
    {
        LocalTransform playerTransform = entityManager.GetComponentData<LocalTransform>(playerEntity);
        playerTransform.Position += new float3(inputComponent.Movement * playerComponent.MoveSpeed * SystemAPI.Time.DeltaTime, 0);

        Vector2 dir = (Vector2)inputComponent.MousePosition - (Vector2)Camera.main.WorldToScreenPoint(playerTransform.Position);
        float angle = math.degrees(math.atan2(dir.y, dir.x));
        playerTransform.Rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        entityManager.SetComponentData(playerEntity, playerTransform);
    }

    private void Shoot(ref SystemState state)
    {
        if (inputComponent.Shoot)
        {
            for(int i = 0; i < playerComponent.NumOfBulletsToSpawn; i++)
            {
                EntityCommandBuffer ECB = new EntityCommandBuffer(Allocator.Temp);

                Entity bulletEntity = entityManager.Instantiate(playerComponent.BulletPrefab);

                ECB.AddComponent(bulletEntity, new BulletComponent
                {
                    Speed = 25f,
                    Size = 0.25f,
                    Damage = 2f
                });

                ECB.AddComponent(bulletEntity, new BulletLifeTimeComponent
                {
                    RemainingLifeTime = 1.5f
                });

                LocalTransform bulletTransform = entityManager.GetComponentData<LocalTransform>(bulletEntity);
                LocalTransform playerTransform = entityManager.GetComponentData<LocalTransform>(playerEntity);

                bulletTransform.Rotation = playerTransform.Rotation;

                float randomOffset = UnityEngine.Random.Range(-playerComponent.BulletSpread, playerComponent.BulletSpread);
                bulletTransform.Position = playerTransform.Position + (playerTransform.Right() * 1.1f) + (bulletTransform.Up() * randomOffset);

                ECB.SetComponent(bulletEntity, bulletTransform);
                ECB.Playback(entityManager);
                ECB.Dispose();
            }
        }
    }

}
