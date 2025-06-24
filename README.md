# AI Plays Tic-Tac-Toe

|  X  |     |     |
| --- | --- | --- |
|     |  O  |     |
|     |     |  X  |

This project demonstrates the use of model free [reinforcement learning](https://en.wikipedia.org/wiki/Reinforcement_learning) where an agent with no prior knowledge
of the game, plays against itself, learning, until it achieves super-human skill.  I implemented with tic-tac-toe as a simple game so as to place a focus on RL and not so much on game mechanics.

## Reinforcement learning (RL)
RL works by an agent making an action, and is rewarded for achieving a goal.  The agent randomly
takes actions, according to the rules of the game, and gains experience, observing what rewards
it gets from taking what actions.  It is capable of learning immediate and delayed reward by allocating
the reward to the history of actions taken to reach such a state.  It cannot learn if it exclusively
uses its experience because it starts out with none.  

## Model free
[Model free learning](https://en.wikipedia.org/wiki/Model-free_(reinforcement_learning)) differs in that it uses Monte Carlo estimation to choose the best move
and creates an optimal policy through trial and error.  It does not require any prior knowledge of environment dynamics.

## Exploration/exploitation tradeoff
It must balance the amount of time it spends exploring unknowing moves, and exploiting the optimal policy because more time spent
exploring to find better solutions is time lost spent exploiting the best moves.  This is called the classic exploration/exploitation tradeoff.  
In this implementation we use an exploration coefficient to manage the tradeoff and begin with a higher value and reduce it as it learns the optimal policy.

## Implementatiom 

1) The Q values represent the the expected reward R for taking an action A given a state S.
2) The agent moves through states by taking an action, and the environment returns a reward for the action.
3) The agent updates its Q values by the rewards received at each state via the following formula:

```
    Q(s, a) = Q(s, a) + α [r + γ * maxQ(s', a') - Q(s, a)]
```

where:
-  $s$ is the current state.
-  $a$ is the action taken by the agent.
-  $s'$ is the next state the agent moves to.
-  $a'$ is the best next action in state $s'$.
-  $α$ (Alpha) is the learning rate determining how much new information affects the old Q-values.
-  $r$ is the reward received for taking action A in state S.
-  $γ$ (Gamma) is the discount factor which balances immediate rewards with future rewards.

Notice how after each move, it effectively pushes a portion of the incremental amount of future reward backwards in time.  This is effectively learning which move is better to make next time.  Over time the Q table should converge to an optimal strategy due to the law of large numbers.

With tic-tac-toe the board is simple and thus the Q table has a finite number of possible states.



