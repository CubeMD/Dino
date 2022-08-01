using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Obstacles
{
    public class ObstacleGenerator : MonoBehaviour
    {
        public event Action<ObstacleGenerator> OnObstacleDestroyedOutOfBounds;
        
        [SerializeField] private Obstacle obstacle;
        [SerializeField] private Vector2 spawnTimeMinMax;
        [SerializeField] private Transform spawnPosition;

        private float RandomSpawnDelay => Random.Range(spawnTimeMinMax.x, spawnTimeMinMax.y);
    
        private readonly List<Obstacle> generatedObstacles = new List<Obstacle>();
        public List<Obstacle> GeneratedObstacles => generatedObstacles;
        
        private Coroutine spawningRoutine;

        private void OnDestroy()
        {
            StopSpawningRoutine();
        }

        private IEnumerator SpawningRoutine()
        {
            while (true)
            {
                Obstacle spawned = Instantiate(obstacle.gameObject, spawnPosition.position, Quaternion.identity, transform).GetComponent<Obstacle>();
                spawned.OnObstacleOutOfBounds += HandleObstacleOutOfBounds;
                generatedObstacles.Add(spawned);
                yield return new WaitForSeconds(RandomSpawnDelay);
            }
        }

        private void HandleObstacleOutOfBounds(Obstacle ob)
        {
            generatedObstacles.Remove(ob);
            DestroyObstacle(ob);
            OnObstacleDestroyedOutOfBounds?.Invoke(this);
        }

        public void ResetObstacles()
        {
            foreach (Obstacle ob in generatedObstacles)
            {
                DestroyObstacle(ob);
            }
            generatedObstacles.Clear();

            StopSpawningRoutine();
            spawningRoutine = StartCoroutine(SpawningRoutine());
        }

        private void StopSpawningRoutine()
        {
            if (spawningRoutine != null)
            {
                StopCoroutine(spawningRoutine);
            }
        }

        private void DestroyObstacle(Obstacle obs)
        {
            if (obs == null)
            {
                return;
            }
            
            obs.OnObstacleOutOfBounds -= HandleObstacleOutOfBounds;
            Destroy(obs.gameObject);
        }
    }
}
