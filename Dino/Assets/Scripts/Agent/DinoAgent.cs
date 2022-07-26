using Obstacles;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Agent
{
    [RequireComponent(typeof(Rigidbody))]
    public class DinoAgent : Unity.MLAgents.Agent
    {
        private const float LEVEL_WIDTH_EXTENDED = 40f;
        protected const float LEVEL_WIDTH = 33f;
        protected const float OBSTACLE_OUT_OF_RANGE_OBSERVATION = -1;

        [SerializeField] protected ObstacleGenerator obstacleGenerator;
        [SerializeField] protected int maxDecisions;
        [SerializeField] protected float jumpForce;
        [SerializeField] protected float startHeight;
        [SerializeField] protected string obstacleTag;
        [SerializeField] protected string floorTag;

        private int currentNumDecisions;
        protected bool canJump;
        private Rigidbody rb;

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

        protected virtual void OnDestroy()
        {
            if (obstacleGenerator != null)
            {
                obstacleGenerator.OnObstacleDestroyedOutOfBounds -= HandleObstacleDestroyedOutOfBounds;
            }
        }
        
        public override void OnEpisodeBegin()
        {
            Academy.Instance.StatsRecorder.Add("EpisodeBegins", 1f, StatAggregationMethod.Sum);
            Vector3 currentPosition = transform.position;
            currentPosition.y = startHeight;
            transform.position = currentPosition;
            rb.velocity = Vector3.zero;
            canJump = true;
            currentNumDecisions = 0;
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
            Academy.Instance.StatsRecorder.Add("SuccessfulJumpOvers", 1f, StatAggregationMethod.Sum);
            AddReward(10f);
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            float observedDistance = LEVEL_WIDTH_EXTENDED;
        
            foreach (Obstacle obstacle in obstacleGenerator.GeneratedObstacles)
            {
                float distanceToObstacle = obstacle.transform.position.x - transform.position.x;
            
                if (distanceToObstacle > 0 && distanceToObstacle < observedDistance)
                {
                    observedDistance = distanceToObstacle;
                }
            }
        
            sensor.AddObservation(observedDistance / LEVEL_WIDTH);
            sensor.AddObservation(canJump);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            IncrementDecisionStep();
            
            if (actions.DiscreteActions[0] == 1)
            {
                TryJump();
            }
        }

        protected void IncrementDecisionStep()
        {
            currentNumDecisions++;
            Academy.Instance.StatsRecorder.Add("TimeTotal", Time.time, StatAggregationMethod.Sum);
            Academy.Instance.StatsRecorder.Add("FramesTotal", Time.frameCount, StatAggregationMethod.Sum);
            Academy.Instance.StatsRecorder.Add("Decisions", 1f, StatAggregationMethod.Sum);
            
            if (currentNumDecisions > maxDecisions)
            {
                Academy.Instance.StatsRecorder.Add("EpisodeInterrupts", 1f, StatAggregationMethod.Sum);
                EndEpisode();
                obstacleGenerator.ResetObstacles();
            }
        }

        private void Jump()
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            canJump = false;
            AddReward(-0.1f);
            Academy.Instance.StatsRecorder.Add("Jumps", 1f, StatAggregationMethod.Sum);
        }
    
        protected void TryJump()
        {
            Academy.Instance.StatsRecorder.Add("TryJumps", 1f, StatAggregationMethod.Sum);
            if (canJump)
            {
                Jump();
            }
            else
            {
                Academy.Instance.StatsRecorder.Add("JumpFails", 1f, StatAggregationMethod.Sum);
                AddReward(-0.5f);
            }
        }
    
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag(floorTag))
            {
                Academy.Instance.StatsRecorder.Add("Lands", 1f, StatAggregationMethod.Sum);
                canJump = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(obstacleTag))
            {
                Academy.Instance.StatsRecorder.Add("Deaths", 1f, StatAggregationMethod.Sum);
                AddReward(-10f);
                EndEpisode();
                obstacleGenerator.ResetObstacles();
            }
        }
    }
}
