using System;
using System.Text;
using MLDebugTool.Scripts.Agent;
using Obstacles;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Agent
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
        
        protected float timeOfLastJump;
        protected float realTimeOfEpisodeStart;
        protected float timeOfEpisodeStart;

        private Rigidbody rb;
        private StringBuilder stringBuilder = new StringBuilder(20);

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
            Vector3 currentPosition = transform.position;
            currentPosition.y = startHeight;
            transform.position = currentPosition;
            
            rb.velocity = Vector3.zero;
            timeOfLastJump = Time.time - DURATION_OF_JUMP;
            
            obstacleGenerator.ResetObstacles();

            realTimeOfEpisodeStart = Time.realtimeSinceStartup;
            timeOfEpisodeStart = Time.time;
        }

        private void Update()
        {
            AddReward(Time.deltaTime * 0.1f);
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
            //Academy.Instance.StatsRecorder.Add("SuccessfulJumpOvers", 1f, StatAggregationMethod.Sum);
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
            float jumpObservation = Mathf.Clamp01((Time.time - timeOfLastJump) / DURATION_OF_JUMP);
            
            sensor.AddObservation(distanceObservation);
            sensor.AddObservation(jumpObservation);
            
            observationsDebugSet.Clear();
            observationsDebugSet.Add("Distance", distanceObservation.ToString("0.000"));
            observationsDebugSet.Add("Jump", jumpObservation.ToString());
            BroadcastObservationsCollected();
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            // Academy.Instance.StatsRecorder.Add("TimeTotal", Time.time, StatAggregationMethod.Sum);
            // Academy.Instance.StatsRecorder.Add("FramesTotal", Time.frameCount, StatAggregationMethod.Sum);
            // Academy.Instance.StatsRecorder.Add("Decisions", 1f, StatAggregationMethod.Sum);
            
            if (actions.DiscreteActions[0] == 1)
            {
                TryJump();
            }
            
            base.OnActionReceived(actions);
        }

        protected void TryJump()
        {
            // Academy.Instance.StatsRecorder.Add("TryJumps", 1f, StatAggregationMethod.Sum);
            if (Time.time - timeOfLastJump >= DURATION_OF_JUMP)
            {
                Jump();
            }
            else
            {
                //Academy.Instance.StatsRecorder.Add("JumpFails", 1f, StatAggregationMethod.Sum);
                AddReward(-0.1f);
            }
        }
        
        private void Jump()
        {
            rb.velocity = Vector3.zero;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            timeOfLastJump = Time.time;
            AddReward(-0.01f);
            // Academy.Instance.StatsRecorder.Add("Jumps", 1f, StatAggregationMethod.Sum);
        }
    
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag(floorTag))
            {
                timeOfLastJump = Time.time - DURATION_OF_JUMP;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(obstacleTag))
            {
                AddRealAndGameTimeStats("Deaths");
                AddReward(-1f);
                EndEpisode();
            }
        }

        protected void AddRealAndGameTimeStats(string statName, float value = 1f, StatAggregationMethod aggregationMethod = StatAggregationMethod.Sum)
        {
            AddGameTimeStat(statName, value, aggregationMethod);
            AddRealTimeStat(statName, value, aggregationMethod);
        }
        
        protected void AddGameTimeStat(string statName, float value = 1f, StatAggregationMethod aggregationMethod = StatAggregationMethod.Sum)
        {
            stringBuilder.Clear().AppendFormat("GameTime/{0}", statName);
            Academy.Instance.StatsRecorder.Add(stringBuilder.ToString(), value / (Time.time - timeOfEpisodeStart), aggregationMethod);
        }
        
        protected void AddRealTimeStat(string statName, float value = 1f, StatAggregationMethod aggregationMethod = StatAggregationMethod.Sum)
        {
            stringBuilder.Clear().AppendFormat("RealTime/{0}", statName);
            Academy.Instance.StatsRecorder.Add(stringBuilder.ToString(), value / (Time.realtimeSinceStartup - realTimeOfEpisodeStart), aggregationMethod);
        }
    }
}
