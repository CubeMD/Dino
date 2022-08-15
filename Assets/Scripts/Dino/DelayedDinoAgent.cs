using System;
using System.Collections.Generic;
using Tools;
using Unity.MLAgents.Actuators;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Dino
{
    [RequireComponent(typeof(CustomDecisionRequester))]
    public class DelayedDinoAgent : DinoAgent
    {
        // private const int MAX_OBSERVABLE_OBSTACLES = 3;
        // private const int MAX_OBSERVABLE_JUMPS = 3;
        // private const float MAX_JUMP_DELAY = 6f;

        private float decisionTime = 1;
        private float jumpTime;
        private bool hasDelayedJump;
        
        //private readonly List<DelayedJump> delayedJumps = new List<DelayedJump>();
        
        public void Start()
        {
            //base.Initialize();
            decisionTime = (GetComponent<CustomDecisionRequester>().DecisionPeriod - 2) * Time.fixedDeltaTime;
            // Debug.Log($"At agent init max decision time is {decisionTime}");
            //DelayedJump.OnJumpDelayFinished += HandleJumpDelayFinished;
        }

        private void FixedUpdate()
        {
            if (hasDelayedJump && Time.time >= jumpTime)
            {
                //Debug.Log($"Agent tried to jump at {Time.time}, when wanted to at {jumpTime}, error is {Time.time - jumpTime}");
                hasDelayedJump = false;
                TryJump();
            }
        }
        // private override ()
        // {
        //      //to prevent action being executed at next decision
        //     // Debug.Log($"Time max time delay is {decisionTime}");
        // }

        // protected override void OnDestroy()
        // {
        //     base.OnDestroy();
        //     DelayedJump.OnJumpDelayFinished -= HandleJumpDelayFinished;
        // }
        
        public override void OnEpisodeBegin()
        {
            base.OnEpisodeBegin();

            hasDelayedJump = false;
            // foreach (DelayedJump delayedJump in delayedJumps)
            // {
            //     delayedJump.Terminate();
            // }
            //
            // delayedJumps.Clear();
        }

        // public override void Heuristic(in ActionBuffers actionsOut)
        // {
        //     if (Input.GetKey(KeyCode.Space))
        //     {
        //         float jumpDelay = Random.Range(0f,2f);
        //         ScheduleJump(jumpDelay);
        //     }
        // }
         
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
            //Debug.Log($"Agent discrete {actions.DiscreteActions[0]}, cont {actions.ContinuousActions[0]}");
            if (actions.DiscreteActions[0] == 1)
            {
                float delayPercentage = (Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f) + 1f) / 2f; //should i do from 0 to 1 part of cont action instead?
                float jumpDelay = Mathf.Lerp(0f, decisionTime, delayPercentage);
                hasDelayedJump = true;
                jumpTime = Time.time + jumpDelay;
                // Debug.Log($"At time {Time.time} agent decided to jump in {jumpDelay} and would do it at {Time.time + jumpDelay}");
                // ScheduleJump(jumpDelay);
            }
            //Debug.Log($"At the moment of decision at {Time.time} there are {delayedJumps.Count} delayed decisions, there are {(delayedJumps.Count > 0 ? delayedJumps[0].GetRemainingTime().ToString() : "No")}");
            OnDecision(actions);
        }
    
        // private void ScheduleJump(float jumpDelay)
        // {
        //     for (int i = 0; i < delayedJumps.Count; i++)
        //     {
        //         if (delayedJumps[i].GetRemainingTime() > jumpDelay)
        //         {
        //             delayedJumps.Insert(i, new DelayedJump(this, jumpDelay));
        //             return;
        //         }
        //     }
        //     
        //     delayedJumps.Add(new DelayedJump(this, jumpDelay));
        // }

        // private void HandleJumpDelayFinished(DelayedJump delayedJump)
        // {
        //     Debug.Log($"Finished at {Time.time}, desired {delayedJump.timeOfExecution}");
        //     if (delayedJumps.Contains(delayedJump))
        //     {
        //        // Debug.Log($"The delay for the jump has been finished, desired time of execution was {delayedJump.timeOfExecution}, executed at {Time.time}");
        //         TryJump();
        //         delayedJumps.Remove(delayedJump);
        //     }
        // }
    }
}
