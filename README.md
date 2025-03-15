# NEAT VS DQN
![DQNvsNEAT](https://github.com/user-attachments/assets/1d0edc07-018b-45c4-b000-34097f7d69b7)
# Description
The project is a simulation of agents learning to survive in an environment by collecting food and avoiding predators. Base consists of a client created on the Unity game engine and a server written in Python.<br/><br/>
The project offers the possibility of using two solutions:
- NEAT (NeuroEvolution of Augmenting Topologies) - A genetic algorithm that, in addition to weights, also modifies the topology of the neural network, starting from small and expanding them to large ones, allowing for finding a more optimized solution to the problem
- DQN (Deep Q-Learning) - A reinforcement learning algorithm that uses a deep neural network that learns from the states and rewards received from the agent to approximate the Q-value.<br/><br/>

During the experiment, data on the state of the agents is collected on the client side and sent via sockets to the Python server, which stores their neural networks and modifies them. 
After processing the data, the movement to be performed by the agent is determined and sent back to the Unity client.
Agents have limited health that decreases with each step they take and is fully regenerated if they eat food. When all agents are dead, a new generation is created.<br/><br/>
The agent reward system is as follows:
- Eating food +20 points
- Hitting a wall -5 points
- Starving to death -10 points
- Dying to an enemy -20 points

# Usage
To run the project, it is necessary to first turn on the server in python and then connect to it using the Unity client. 
After connecting, the program gives you the option to choose an algorithm as well as the option to use an already trained network located somewhere on your computer.
The program automatically saves neural networks every 50 generations or the latest generation when the program is stopped.<br/><br/> During the program's operation, information is also collected about:
- Highest, lowest and average generation ratings
- CPU and memory usage
- The largest, smallest and average number of moves in a generation
- The agent's movement type
- Cause of death of agent
- Decisions made by agents (direction of movement)
