using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MLDebugTool.Scripts.Agent;
using Obstacles;
using Unity.MLAgents;
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
        
        protected float timeOfLastJump;
        protected float realTimeOfEpisodeStart;
        protected float timeOfEpisodeStart;
        protected float realTimeOfLastDecision;
        protected float timeOfLastDecision;
        protected Coroutine eachRealSecondRoutine;

        private readonly Dictionary<string, float> episodeStats = new Dictionary<string, float>();
        private int numDecisions;
        private int numDecisionsTotal;
        private float gameplayTimeTotal;
        private int numEpisodesTotal;
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
            eachRealSecondRoutine = StartCoroutine(EachRealSecondRoutine());
        }

        protected override void OnDestroy()
        {
            if (obstacleGenerator != null)
            {
                obstacleGenerator.OnObstacleDestroyedOutOfBounds -= HandleObstacleDestroyedOutOfBounds;
            }
            StopCoroutine(eachRealSecondRoutine);
            
            base.OnDestroy();
        }
        
        public override void OnEpisodeBegin()
        {
            Vector3 currentPosition = transform.position;
            currentPosition.y = startHeight;
            transform.position = currentPosition;
            
            rb.velocity = Vector3.zero;
            timeOfLastJump = Time.time;
            numDecisions = 0;
            
            obstacleGenerator.ResetObstacles();

            realTimeOfEpisodeStart = realTimeOfLastDecision = Time.realtimeSinceStartup;
            timeOfEpisodeStart = timeOfLastDecision = Time.time;

            List<string> keys = episodeStats.Keys.ToList();
            foreach (string key in keys)
            {
                episodeStats[key] = 0;
            }
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
            AddEpisodeStat("SuccessfulJumpOvers");
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
            numDecisions++;
            numDecisionsTotal++;
            realTimeOfLastDecision = Time.realtimeSinceStartup;
            timeOfLastDecision = Time.time;
            
            if (actions.DiscreteActions[0] == 1)
            {
                TryJump();
            }
            
            base.OnActionReceived(actions);
        }

        protected void TryJump()
        {
            AddEpisodeStat("AttemptedJumps");
            if (Time.time - timeOfLastJump >= DURATION_OF_JUMP)
            {
                Jump();
            }
            else
            {
                AddReward(-0.1f);
                AddEpisodeStat("InvalidJumps");
            }
        }
        
        private void Jump()
        {
            rb.velocity = Vector3.zero;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            timeOfLastJump = Time.time;
            AddReward(-0.01f);
            AddEpisodeStat("ExecutedJumps");
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
                numEpisodesTotal++;
                gameplayTimeTotal += Time.time - timeOfEpisodeStart;
                SendEndEpisodeStats();
                AddReward(-1f);
                EndEpisode();
            }
        }
        
        protected void AddEpisodeStat(string statName, float value = 1f)
        {
            if (episodeStats.ContainsKey(statName))
            {
                episodeStats[statName] += value;
            }
            else
            {
                episodeStats.Add(statName, value);
            }
        }
        
        protected void SendRealTimeAndEpisodeStats(string statName, float value = 1f)
        {
            SendEpisodeStat(statName, value);
            SendRealTimeStat(statName, value);
        }

        protected void SendTotalStats()
        {
            Academy.Instance.StatsRecorder.Add("Totals/GameplaySeconds", gameplayTimeTotal, StatAggregationMethod.Sum);
            Academy.Instance.StatsRecorder.Add("Totals/Decisions", numDecisionsTotal, StatAggregationMethod.Sum);
            Academy.Instance.StatsRecorder.Add("Totals/Episodes", numEpisodesTotal, StatAggregationMethod.Sum);
        }
        
        protected void SendEpisodeStat(string statName, float value = 1f)
        {
            stringBuilder.Clear().AppendFormat("PerEpisode/{0}", statName);
            Academy.Instance.StatsRecorder.Add(stringBuilder.ToString(), value, StatAggregationMethod.Histogram);
        }
        
        protected void SendRealTimeStat(string statName, float value = 1f)
        {
            stringBuilder.Clear().AppendFormat("PerRealSecond/{0}", statName);
            Academy.Instance.StatsRecorder.Add(stringBuilder.ToString(), value / Time.realtimeSinceStartup, StatAggregationMethod.Sum);
        }

        protected IEnumerator EachRealSecondRoutine()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(1f);
                
                SendRealTimeStat("Decisions", numDecisionsTotal);
                SendRealTimeStat("GameplaySeconds", Time.time);
                SendRealTimeStat("Episodes", numEpisodesTotal);
                
                SendTotalStats();
            }
        }

        protected void SendEndEpisodeStats()
        {
            foreach (string key in episodeStats.Keys)
            {
                SendEpisodeStat(key, episodeStats[key]);
            }
            
            SendEpisodeStat("Decisions", numDecisions);
            SendEpisodeStat("GameplaySeconds", Time.time - timeOfEpisodeStart);
        }
    }
}
