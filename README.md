# AI Plays Tic-Tac-Toe

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


