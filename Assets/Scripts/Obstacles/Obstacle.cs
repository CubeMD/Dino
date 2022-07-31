using System;
using UnityEngine;

namespace Obstacles
{
    public class Obstacle : MonoBehaviour
    {
        public event Action<Obstacle> OnObstacleOutOfBounds;

        [SerializeField] private string obstacleDestroyerTag;
        [SerializeField] private float velocity;
    
        private void FixedUpdate()
        {
            transform.position += Vector3.left * velocity * Time.fixedDeltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(obstacleDestroyerTag))
            {
                OnObstacleOutOfBounds?.Invoke(this);
            }
        }
    }
}
