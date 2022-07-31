# Dino

An experiment with decision frequency of an RL agent

*unity env gif here*

## Short story and motivation

The idea for the Dino environment comes from my desire to experiment with formulations of reinforcement learning. Gym has practically established standards for certain design choices like frame-skipping (requesting a decision from the policy every N-th frame and repeating it between decisions) or frame-stacking (enriching the state with temporal information). The purpose of these hacks is to adapt discrete time decision making of rl to complexity of real time decision making in games. While these techniques certainly help, they are far from solving all problems and actually create some of their own. I believe that we desire a solution that would bring the way the agent interacts with the environment closer to how we interact with real-time videogames ourselves.

To illustrate these issues and exercise solving them, I was looking for a game with extremely simplistic mechanics, reward mechanism, state and action spaces. These qualities would speed up the training, cut out variables that I am not interested in and increase iteration speed. It would need to be real time, and require the agent to produce actions at a specific times, unlike many other environments, which require the agent to continuously control a physics body or a vehicle.

This is when a small chrome browser game Dino caught my attention. The game tests the player's ability to execute well timed actions, while being extremely simplistic in mechanics, reward mechanism, state and action spaces. This makes it an ideal candidate to work with.

*gif of original game*

Admittedly, at this point I've made a wrong turn and decided to approach the problem from the most realistic angle, using a real, original browser game as the environment. I had no doubt that a purposefully designed environment would be solvable by existing methods and wanted to try myself with a holy grail - drl on an environment that you have practically no control over. 

Thankfully, I could find enough information on the topic and setting up a custom gym environment was quite easy. It would capture a portion of the screen and emulate keypresses. Turns out that people are actively working in the direction of teaching agents to play real games and someone tried to make Dino deep rl and had success with it. Oh, well...

Using available resources I was able to train a reasonable agent, but ultimately had to change my approach. This problem poses many interesting challenges, like the fact that the environment can not be paused and various delays that are introduced at different stages of interaction. These things certainly play a huge role in how we interact with the world and definitely deserve further investigation, but lack of complete control over lifetime of the environment forced me to come to come to a (quite) an obvious conclusion that I need environment, which I would have full control of.

Thankfully, making Dino in Unity is a piece of cake and I hope that code ended up being pretty self-explanatory. 

## Environment description

Player is stationary, while obstacles are spawned with random delays and approach the player with constant velocity. The game is over if an obstacle collides with a player. Player can jump at any time and, if the jump is timed correctly, player will hop over a single obstacle. The opportunity window of a successful jump is pretty tight (0.25 - 0.75 seconds before collision), but humans can easily learn to perform it.

I am not going to go into details of how the Unity Engine works, however, as we are primarily interested in the time aspect, I would like to mention that the game we are trying to create is fundamentally real-time. While game engine renders individual frames and steps physics in discrete time steps, ideally, this should not affect the player's ability to execute an action at a specific time.

## DRL Agents

### Decisions intro

### Decision Period
Agents request decisions every N FixedUpdates, N being a decision period. FixedUpdate is a Unity function which is primarily used for calculations which happen once in a fixed amount of time (By default 0.02 second or 50 times per second). Decision period is an important hyperparameter, and it is particularly interesting how training process and agent's performance changes with this variable.

### Rewards

Reward function shaping is itself a complex topic and, from my experience, one of the core determining factors for the performance of the agent in created from scratch environments. At the moment, the agent receives a positive reward for successfully jumping over obstacles and negative reward for colliding with them.

I have also tried to bias the agent to jump less, by giving a small negative reward for jumping and a slightly bigger negative reward for jumping while in the air (action is ignored), as this structure is used often for similar problems. However, it resulted in agents often getting stuck in the local minima of never jumping. I am still actively looking better performing solutions, but run all experiments using the same approach for apples to apples comparison.

### Immediate action agent

#### About
This is what I would call a standard approach, and depending on the particular problem it would be modified to fit the needs.

#### Actions
Agent's decision is simple too - a discrete number that can take two values, which represent if agent should jump right now, or not.

#### Observations
In this case it is as simple as it gets - a single normalized float, representing the distance to a nearest obstacle, is enough to play the game perfectly. Decision period, velocity of the obstacle and time in general are only influencing at which specific values of the observation the agent needs to jump.

#### Considerations
The main issue is that decision period determines action period - if it is set too high, we will eventually get unlucky and a new obstacle is going to be spawned in such a way, that the entire successful jump opportunity window is going to be missed. This particular problem comes up in video games all the time - player is required not only to do split second decisions, but also issue actions at very specific times (i.e. parry the enemy attack). 

Setting decision period to be small is not an option for several reasons:

- Generally, as the decision period gets smaller, the individual agent action's effect on the expected reward gets smaller. This leads to unstable training due to advantages of actions becoming indistinguishably small (network approximation error makes it even worse). While this is not the case in the Dino environment, as the entire jump is executed regardless of decision period, this is certainly an undesired quality of the approach.

- To incorporate DRL into commercial games, we need to consider performance. We desire the opposite - the agent should request decisions as infrequently as possible, without loosing performance.

- Technically, requesting decisions every FixedUpdate would only make the inaccessible jump opportunity window size tiny, but wouldn't get rid of it completely. Agent would still be able to execute actions only when it observes the environment.


### Delayed action agent

#### About
After investigating recent papers on the topic, I came to conclusion that people are aware of these issues and came up with dozens of ways to deal with them. Their approaches vary significantly in complexity, some of them require modifications to core RL assumptions (which themselves are still actively researched), while others are less intrusive.

I am particularly interested in the latter, as I believe that at our core we are reasoning about the world in discrete dynamic size time-steps, even if the processes guiding these decisions are continuous. I think that it is possible use existing DRL algorithms to act in continuous time environments in a way that is natural for humans and, as a bonus, incorporates notion of no-op in its core, without (significant) changes to the learning algorithms themselves.

Out of everything that I found, I liked the solutions that modify decision-action system best. This approach is inspired by the STRAW paper, however I was guided by other papers and my own intuition.

#### Actions
Decision request still produces a discrete number that can take two values, which represent if agent should jump, or not. In case the agent decided to jump we request an additional positive floating point number, which represents the delay before the jump. Then a jump is scheduled to be executed at the desired time. 

#### Observations
To keep the problem Markovian, we certainly need to be able to perceive our plan.

