from collections import deque
from classes.model import model, trainer
import random
import torch
import os

MAX_MEMORY = 100_000
BATCH_SIZE = 1000
LR = 0.001

class Agent:
    generation = 0
    def __init__(self, existingNetwork):
        self.epsilon = 0
        self.gamma = 0.9
        self.memory = deque(maxlen=MAX_MEMORY)
        self.model = model(11, 256, 4)
        self.cumulativeReward = 0
        self.randomMove = 0
        self.calculatedMove = 0

        if existingNetwork is not None:
            print("Network exist")
            self.model.load_state_dict(torch.load(existingNetwork))

        self.trainer = trainer(self.model, lr=LR, gamma=self.gamma)
        self.done = False
        self.oldState = [0, 0, 0]
        self.newState = [0, 0, 0]
        self.move = [0, 0, 0, 0]
        self.reward = 0

    #@classmethod
    def saveModel(self, folderName):
        full_path = os.path.join(folderName, f"Generation_{Agent.generation}")
        os.makedirs(f"{full_path}", exist_ok=True)
        full_path2 = os.path.join(full_path, f'DQN_Model_Generation{Agent.generation}.pth')
        torch.save(self.model.state_dict(), full_path2)

    def remember(self, state, action, reward, next_state, done):
        self.memory.append((state, action, reward, next_state, done))

    def train_long_memory(self):
        if len(self.memory) > BATCH_SIZE:
            mini_sample = random.sample(self.memory, BATCH_SIZE)  # list of tuples
        else:
            mini_sample = self.memory

        states, actions, rewards, next_states, dones = zip(*mini_sample)
        self.trainer.train_step(states, actions, rewards, next_states, dones)

    def train_short_memory(self, state, action, reward, next_state, done):
        self.trainer.train_step(state, action, reward, next_state, done)

    def get_action(self, state):
        self.epsilon = 80 - Agent.generation
        final_move = [0, 0, 0, 0]
        if random.randint(0, 200) < self.epsilon:
            move = random.randint(0, 3)
            final_move[move] = 1
            self.randomMove += 1
        else:
            state0 = torch.tensor(state, dtype=torch.float)
            prediction = self.model(state0)
            move = torch.argmax(prediction).item()
            final_move[move] = 1
            self.calculatedMove += 1

        return final_move