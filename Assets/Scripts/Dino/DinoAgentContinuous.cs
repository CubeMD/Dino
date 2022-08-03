using System;
using System.Collections.Generic;
using Obstacles;
using Tools;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Dino
{
    [RequireComponent(typeof(CustomDecisionRequester))]
    public class DinoAgentContinuous : DinoAgent
    {
        private const int MAX_OBSERVABLE_OBSTACLES = 3;
        private const int MAX_OBSERVABLE_JUMPS = 3;
        private const float MAX_JUMP_DELAY = 6f;

        private float decisionTime = 1;
        
        private readonly List<DelayedJump> delayedJumps = new List<DelayedJump>();

        protected override void Awake()
        {
            base.Awake();
            DelayedJump.OnJumpDelayFinished += HandleJumpDelayFinished;
        }

        private void Start()
        {
            decisionTime = GetComponent<CustomDecisionRequester>().DecisionPeriod * Time.fixedDeltaTime;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            DelayedJump.OnJumpDelayFinished -= HandleJumpDelayFinished;
        }

        private void Update()
        {
            string debug = String.Empty;
            
            foreach (DelayedJump delayedJump in delayedJumps)
            {
                debug += " " + delayedJump.GetRemainingTime();
            }
            
            //Debug.Log($"At time {Time.time} the list of times is {debug}");
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

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                float jumpDelay = Random.Range(0f,2f);
                ScheduleJump(jumpDelay);
            }
        }
        
        // public override void CollectObservations(VectorSensor sensor)
        // {
        //     // int numAddedDistances = 0;
        //     //
        //     // foreach (Obstacle obstacle in obstacleGenerator.GeneratedObstacles)
        //     // {
        //     //     float distance = obstacle.transform.position.x - transform.position.x;
        //     //     if (distance > 0 && numAddedDistances < MAX_OBSERVABLE_OBSTACLES)
        //     //     {
        //     //         numAddedDistances++;
        //     //         sensor.AddObservation(distance / LEVEL_WIDTH);
        //     //     }
        //     // }
        //     //
        //     // for (int i = 0; i < MAX_OBSERVABLE_OBSTACLES - numAddedDistances; i++)
        //     // {
        //     //     //sensor.AddObservation(OBSTACLE_OUT_OF_RANGE_OBSERVATION);
        //     // }
        //     //
        //     // sensor.AddObservation(timeOfLastJump);
        //     //
        //     // for (int i = 0; i < MAX_OBSERVABLE_JUMPS; i++)
        //     // {
        //     //     sensor.AddObservation(delayedJumps.Count > i ? delayedJumps[i].GetRemainingTime() / MAX_JUMP_DELAY : MAX_JUMP_DELAY + 1);
        //     // }
        //     sensor.AddObservation(delayedJumps.Count == 0 ? );
        //     
        //     base.CollectObservations(sensor);
        // }

        public override void OnActionReceived(ActionBuffers actions)
        {
            AddReward((Time.time - timeOfLastDecision) * 0.1f);
            RecordEpisodeStat("PerEpisode/DiscreteAction", actions.DiscreteActions[0]);
        //    Debug.Log($"Disc {actions.DiscreteActions[0]}");
            if (actions.DiscreteActions[0] == 1)
            {
                float delayPercentage = (Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f) + 1f) / 2f;
                float jumpDelay = Mathf.Lerp(0f, decisionTime, delayPercentage);
    //            Debug.Log($"Delay {jumpDelay}");
                RecordEpisodeStat("PerEpisode/JumpDelay", jumpDelay);
                ScheduleJump(jumpDelay);
            }
            OnDecision(actions);
        }
    
        private void ScheduleJump(float jumpDelay)
        {
            for (int i = 0; i < delayedJumps.Count; i++)
            {
                if (delayedJumps[i].GetRemainingTime() > jumpDelay)
                {
                    delayedJumps.Insert(i, new DelayedJump(this, jumpDelay));
                    return;
                }
            }
            
            delayedJumps.Add(new DelayedJump(this, jumpDelay));
        }

        private void HandleJumpDelayFinished(DelayedJump delayedJump)
        {
            if (delayedJumps.Contains(delayedJump))
            {
                TryJump();
                delayedJumps.Remove(delayedJump);
            }
        }
    }
}
