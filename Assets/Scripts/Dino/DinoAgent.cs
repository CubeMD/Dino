using MLDebugTool.Scripts.Agent;
using Obstacles;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Dino
{
    [RequireComponent(typeof(Rigidbody))]
    public class DinoAgent : DebuggableAgent
    {
        protected const float LEVEL_WIDTH = 33f;
        protected const float DURATION_OF_JUMP = 2f;

        [SerializeField] protected ObstacleGenerator obstacleGenerator;
        [SerializeField] protected float jumpForce;
        [SerializeField] protected float startHeight;
        [SerializeField] protected string obstacleTag;
        [SerializeField] protected string floorTag;
        
        private Rigidbody rb;
        protected float timeOfLastJump;
        private bool canJump;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody>();

            if (obstacleGenerator == null)
            {
                Debug.LogError("Obstacle generator is required for proper agent functionality");
                return;
            }

            obstacleGenerator.OnObstacleDestroyedOutOfBounds += HandleObstacleDestroyedOutOfBounds;
        }

        protected override void OnDestroy()
        {
            if (obstacleGenerator != null)
            {
                obstacleGenerator.OnObstacleDestroyedOutOfBounds -= HandleObstacleDestroyedOutOfBounds;
            }

            base.OnDestroy();
        }
        
        public override void OnEpisodeBegin()
        {
            RecordEpisodeStat("PerEpisode/GameplayTime", Time.time - timeOfEpisodeStart);
            
            Vector3 currentPosition = transform.position;
            currentPosition.y = startHeight;
            transform.position = currentPosition;
            
            rb.velocity = Vector3.zero;
            timeOfLastJump = Time.time - DURATION_OF_JUMP;
            canJump = false;
            obstacleGenerator.ResetObstacles();
            
            base.OnEpisodeBegin();
        }
        
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                TryJump();
            }
        }

        private void HandleObstacleDestroyedOutOfBounds(ObstacleGenerator og)
        {
            RecordEpisodeStat("PerEpisode/SuccessfulJumpOvers");
            AddReward(1f);
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            float observedDistance = LEVEL_WIDTH;
        
            foreach (Obstacle obstacle in obstacleGenerator.GeneratedObstacles)
            {
                float distanceToObstacle = obstacle.transform.position.x - transform.position.x;
            
                if (distanceToObstacle > 0 && distanceToObstacle < observedDistance)
                {
                    observedDistance = distanceToObstacle;
                }
            }

            float distanceObservation = observedDistance / LEVEL_WIDTH;
            float jumpObservation = canJump ? 1 : Mathf.Clamp01((Time.time - timeOfLastJump) / DURATION_OF_JUMP);
            
            sensor.AddObservation(distanceObservation);
            sensor.AddObservation(jumpObservation);
            
            observationsDebugSet.Add("Distance", distanceObservation.ToString("0.000"));
            observationsDebugSet.Add("Jump", jumpObservation.ToString());
            BroadcastObservationsCollected();
            
            observationsDebugSet.Clear();
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            AddReward((Time.time - timeOfLastDecision) * 0.1f);
            
            if (actions.DiscreteActions[0] == 1)
            {
                TryJump();
            }
            
            base.OnActionReceived(actions);
        }

        protected void TryJump()
        {
            RecordEpisodeStat("PerEpisode/AttemptedJumps");
            if (canJump)
            {
                Jump();
            }
            else
            {
                AddReward(-0.1f);
                RecordEpisodeStat("PerEpisode/InvalidJumps");
            }
        }
        
        private void Jump()
        {
            rb.velocity = Vector3.zero;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            timeOfLastJump = Time.time;
            canJump = false;
            AddReward(-0.01f);
            RecordEpisodeStat("PerEpisode/ExecutedJumps");
        }
    
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag(floorTag))
            {
                canJump = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(obstacleTag))
            {
                AddReward(-1f);
                EndEpisode();
            }
        }
    }
}
