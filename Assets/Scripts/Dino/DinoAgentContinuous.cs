using System.Collections.Generic;
using Obstacles;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Dino
{
    public class DinoAgentContinuous : DinoAgent
    {
        private const int MAX_OBSERVABLE_OBSTACLES = 3;
        private const int MAX_OBSERVABLE_JUMPS = 3;
        private const float MAX_JUMP_DELAY = 6f;
        
        private readonly List<DelayedJump> delayedJumps = new List<DelayedJump>();

        protected override void Awake()
        {
            base.Awake();
            DelayedJump.OnJumpDelayFinished += HandleJumpDelayFinished;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            DelayedJump.OnJumpDelayFinished -= HandleJumpDelayFinished;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            int numAddedDistances = 0;

            foreach (Obstacle obstacle in obstacleGenerator.GeneratedObstacles)
            {
                float distance = obstacle.transform.position.x - transform.position.x;
                if (distance > 0 && numAddedDistances < MAX_OBSERVABLE_OBSTACLES)
                {
                    numAddedDistances++;
                    sensor.AddObservation(distance / LEVEL_WIDTH);
                }
            }
        
            for (int i = 0; i < MAX_OBSERVABLE_OBSTACLES - numAddedDistances; i++)
            {
                //sensor.AddObservation(OBSTACLE_OUT_OF_RANGE_OBSERVATION);
            }
        
            sensor.AddObservation(timeOfLastJump);
        
            for (int i = 0; i < MAX_OBSERVABLE_JUMPS; i++)
            {
                sensor.AddObservation(delayedJumps.Count > i ? delayedJumps[i].GetRemainingTime() / MAX_JUMP_DELAY : MAX_JUMP_DELAY + 1);
            }
        }
        
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                float jumpDelay = Random.Range(1f,2f);
                ScheduleJump(jumpDelay);
            }
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            //IncrementDecisionStep();
            Academy.Instance.StatsRecorder.Add("AvgDiscreteAction", actions.DiscreteActions[0], StatAggregationMethod.Average);
            Academy.Instance.StatsRecorder.Add("AvgContinuousAction", actions.ContinuousActions[0], StatAggregationMethod.Average);
            
            if (actions.DiscreteActions[0] == 1)
            {
                float jumpDelay = Mathf.Lerp(0f, MAX_JUMP_DELAY, (actions.ContinuousActions[0] + 1f) / 2f);
                Academy.Instance.StatsRecorder.Add("AvgJumpDelay", jumpDelay, StatAggregationMethod.Average);
                ScheduleJump(jumpDelay);
                
                float delayReward = 1 / (1 - (jumpDelay + 2));
                Academy.Instance.StatsRecorder.Add("AvgJumpDelay", delayReward, StatAggregationMethod.Average);
                AddReward(delayReward);
            }
        }
    
        private void ScheduleJump(float jumpDelay)
        {
            Academy.Instance.StatsRecorder.Add("AvgJumpDelay", jumpDelay, StatAggregationMethod.Average);
            for (int i = 0; i < delayedJumps.Count; i++)
            {
                if (delayedJumps[i].GetRemainingTime() > jumpDelay)
                {
                    delayedJumps.Insert(i, new DelayedJump(this, jumpDelay));
                    return;
                }
            }
            
            delayedJumps.Add(new DelayedJump(this, jumpDelay));
            Academy.Instance.StatsRecorder.Add("AvgDelayedJumps", delayedJumps.Count, StatAggregationMethod.Average);
        }

        private void HandleJumpDelayFinished(DelayedJump delayedJump)
        {
            TryJump();
            
            if (delayedJumps.Contains(delayedJump))
            {
                delayedJumps.Remove(delayedJump);
            }
        }

        public override void OnEpisodeBegin()
        {
            base.OnEpisodeBegin();
            
            foreach (DelayedJump delayedJump in delayedJumps)
            {
                delayedJump.Terminate();
            }
            
            delayedJumps.Clear();
        }
    }
}
