import socket
import json
import psutil
import neat
import os
from datetime import datetime
import pickle
import copy
from classes.agent import Agent
from classes.ploter import cpuPloter, fitPloter, timePloter

cpu_data = []
memory_data = []
avg_cpu_data = []
avg_memory_data = []

maxFit = []
minFit = []
avgFit = []

maxTime = []
minTime = []
avgTime = []

fitestNEATAgent = None
bestNEATGeneration = 0
existingNetwork = None

def start_server():
    global conn, existingNetwork
    HOST = 'localhost'
    PORT = 12345

    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.bind((HOST, PORT))
        s.listen()

        print("Waiting for client connection...")
        conn, addr = s.accept()

        with conn:
            print(f"Connected with client by {addr}")

            while True:
                data = conn.recv(4096)
                if not data:
                    break

                json_data = json.loads(data.decode('utf-8'))
                print(f"Data send to server {json_data}")

                if json_data.get("message") == "Start":

                    if json_data.get("network") is not None:
                        existingNetwork = json_data.get("network")
                        print(f"Chosen network {existingNetwork}")

                    if json_data.get("expNumber") % 2 == 0:
                        print(f"Chosen NEAT")

                        local_dir = os.path.dirname(__file__)
                        config_file = os.path.join(local_dir, 'config/config.txt')

                        runNEAT(config_file)

                    if json_data.get("expNumber") % 2 == 1:
                        print(f"Chosen DQN")
                        try:
                            runDQN()
                        except StopIteration as e:
                            print(e)


        print(f"Closing server")
def monitor_resources(interval=0.1):
    while True:
        cpu = psutil.cpu_percent(interval=interval, percpu=True)
        memory = psutil.virtual_memory().percent
        yield cpu, memory
def runDQN():
    global folderName

    print(f"Create agents")
    agentsActions = {}
    DQNagents = {}

    folderName = "networks/DQN/Prey"
    now = datetime.now()
    formatted_time = now.strftime('%Y-%m-%d_%H-%M-%S')
    folderName = f"{folderName}_{formatted_time}"

    os.makedirs(f"{folderName}", exist_ok=True)

    for i in range(1):
        DQNagents[i] = Agent(existingNetwork)

    Agent.generation = 0

    cpu_data.clear()
    memory_data.clear()
    avg_cpu_data.clear()
    avg_memory_data.clear()

    while True:

        resource_monitor = monitor_resources()
        agentTimes = []

        Agent.generation += 1
        print(Agent.generation)

        for key in DQNagents:
            DQNagents[key].done = False
            agentsActions[key] = 0

        print(f"Making client to create agents")
        response = json.dumps({"action": "create", "number": 1})
        conn.send(response.encode('utf-8'))

        print(f"Confirmation of client creation")
        data = conn.recv(4096)
        json_data = json.loads(data.decode('utf-8'))
        print(f"data send {json_data}")

        if json_data.get("message") == 'stop':
            DQNagents[0].saveModel(folderName)
            saveDQN(folderName, Agent.generation)
            raise StopIteration("Finished algorithm on demand")

        while True:

            print(f"Ask client for old state")
            response = json.dumps({"action": "SendData"})
            conn.send(response.encode('utf-8'))

            print(f"Get old state")
            data = conn.recv(4096)
            json_data = json.loads(data.decode('utf-8'))
            print(f"Agents data {json_data}")

            if json_data.get("message") == 'stop':
                DQNagents[0].saveModel(folderName)
                saveDQN(folderName, Agent.generation)
                raise StopIteration("Finished algorithm on demand")

            print(f"Get action from data")
            for key in agentsActions:
                DQNagents[key].oldState = [0, 1, 0] #popraw na pobierane z unity
                DQNagents[key].move = DQNagents[key].get_action(DQNagents[key].oldState)
                agentsActions[key] = DQNagents[key].move.index(max(DQNagents[key].move))

            print(f"Send action to client")
            response = json.dumps(agentsActions)
            conn.send(response.encode('utf-8'))

            print(f"Get new state with rewards")
            data = conn.recv(4096)
            json_data = json.loads(data.decode('utf-8'))
            print(f"Agents rewards {json_data}")

            if json_data.get("message") == 'stop':
                DQNagents[0].saveModel(folderName)
                saveDQN(folderName, Agent.generation)
                raise StopIteration("Finished algorithm on demand")

            for key in agentsActions:
                DQNagents[key].newState = [0, 0, 1] #popraw na pobierane z unity
                DQNagents[key].done = not (json_data['0']['alive'])
                DQNagents[key].reward += 10

            print(f"Short memory training")
            for key in agentsActions:
                DQNagents[key].train_short_memory(DQNagents[key].oldState, DQNagents[key].move, DQNagents[key].reward, DQNagents[key].newState, DQNagents[key].done)
                DQNagents[key].remember(DQNagents[key].oldState, DQNagents[key].move, DQNagents[key].reward, DQNagents[key].newState, DQNagents[key].done)

            print(f"Remembering")

            for key in DQNagents:
                if DQNagents[key].done == True:
                    agentsActions.pop(key)
                    agentTimes.append()

            print(f"Testing if all agents are dead")

            if all(instance.done == True for instance in DQNagents.values()):
                response = json.dumps({"action": "new_gen"})
                conn.send(response.encode('utf-8'))
                print(f"Waiting for ability to continue new_ge")
                data = conn.recv(4096)
                json_data = json.loads(data.decode('utf-8'))
                print(f"Returned info: {json_data}")

                if json_data.get("message") == 'stop':
                    DQNagents[0].saveModel(folderName)
                    saveDQN(folderName, Agent.generation)
                    raise StopIteration("Finished algorithm on demand")

                break
            else:
                response = json.dumps({"action": "continue"})
                conn.send(response.encode('utf-8'))
                print(f"Waiting for ability to continue continue")
                data = conn.recv(4096)
                json_data = json.loads(data.decode('utf-8'))
                print(f"Returned info: {json_data}")

                if json_data.get("message") == 'stop':
                    DQNagents[0].saveModel(folderName)
                    saveDQN(folderName, Agent.generation)
                    raise StopIteration("Finished algorithm on demand")

        print(f"Long memory training")
        for key in DQNagents:
            DQNagents[key].train_long_memory()

        agentsActions.clear()

        if (Agent.generation) % 50 == 0:
            DQNagents[0].saveModel(folderName)

        cpu, memory = next(resource_monitor)
        cpu_data.append(sum(cpu) / len(cpu))
        memory_data.append(memory)
        avg_cpu_data.append((sum(cpu_data) / len(cpu_data)))
        avg_memory_data.append((sum(memory_data) / len(memory_data)))

        cpuPloter(cpu_data, avg_cpu_data, memory_data, avg_memory_data, "", "DQN")
        fitPloter(maxFit, minFit, avgFit, "", "DQN") # dokończyć
        timePloter(maxTime, minTime, avgTime, "", "DQN") # dokończyć
def runNEAT(config_file):
    global population, folderName

    folderName = "networks/NEAT/Prey"
    now = datetime.now()
    formatted_time = now.strftime('%Y-%m-%d_%H-%M-%S')
    folderName = f"{folderName}_{formatted_time}"

    os.makedirs(f"{folderName}", exist_ok=True)

    config = neat.config.Config(
        neat.DefaultGenome,
        neat.DefaultReproduction,
        neat.DefaultSpeciesSet,
        neat.DefaultStagnation,
        config_file
    )

    population = neat.Population(config)

    print(f"WTF {existingNetwork}")

    if existingNetwork is not None:
        with open(existingNetwork, 'rb') as f:
            trained_genome = pickle.load(f)

        for genome_id in population.population:
            population.population[genome_id].nodes = copy.deepcopy(trained_genome.nodes)
            population.population[genome_id].connections = copy.deepcopy(trained_genome.connections)

    cpu_data.clear()
    memory_data.clear()
    avg_cpu_data.clear()
    avg_memory_data.clear()

    maxFit.clear()
    minFit.clear()
    avgFit.clear()

    maxTime.clear()
    minTime.clear()
    avgTime.clear()

    try:
       population.run(fitnessNEAT)
    except StopIteration as e:
        print(e)
        saveNEAT(folderName, bestNEATGeneration)
def fitnessNEAT(genomes, config):
    global fitestNEATAgent, bestNEATGeneration

    resource_monitor = monitor_resources()

    print(f"Create agents")

    agentsActions = {}
    a_genomes = {}
    nets = {}
    agentTimes = []

    bestTime = None
    worstTime = None
    meanTime = 0

    for i, (genome_id, genome) in enumerate(genomes):
        agentsActions[i] = 0
        genome.fitness = 0
        a_genomes[i] = genome
        nets[i] = neat.nn.FeedForwardNetwork.create(genome, config)

    print(f"Making client to create agents")
    response = json.dumps({"action": "create", "number": 10})
    conn.send(response.encode('utf-8'))

    print(f"Confirmation of client creation")
    data = conn.recv(4096)
    json_data = json.loads(data.decode('utf-8'))
    print(f"data send {json_data}")

    if json_data.get("message") == 'stop':
        raise StopIteration("Finished algorithm on demand")

    while True:
        print(f"Ask client for state")
        response = json.dumps({"action": "SendData"})
        conn.send(response.encode('utf-8'))

        print(f"Get state")
        data = conn.recv(4096)
        json_data = json.loads(data.decode('utf-8'))
        print(f"Agents data {json_data}")

        if json_data.get("message") == 'stop':
            raise StopIteration("Finished algorithm on demand")

        print(f"Get action from data")
        for key in agentsActions:
            output_value = nets[key].activate((0, 1, 0))
            agentsActions[key] = output_value.index(max(output_value))

        print(f"Send action to client")
        response = json.dumps(agentsActions)
        conn.send(response.encode('utf-8'))

        print(f"Get rewards")
        data = conn.recv(4096)
        json_data = json.loads(data.decode('utf-8'))
        print(f"Agents rewards {json_data}")

        if json_data.get("message") == 'stop':
            raise StopIteration("Finished algorithm on demand")

        print(f"Setting fitness")
        for key in json_data:
            a_genomes[int(key)].fitness += json_data[key]['reward']

        print(f"killing off the agents")
        for key in json_data:
            if not json_data[key]['alive']:
                agentsActions.pop(int(key))
                a_genomes.pop(int(key))
                nets.pop(int(key))

                agentTimes.append(json_data[key]['lifeTime'])

        print(f"Testing if all agents are dead")

        if len(agentsActions) <= 0:
            response = json.dumps({"action": "new_gen"})
            conn.send(response.encode('utf-8'))
            print(f"Waiting for ability to continue")
            data = conn.recv(4096)
            json_data = json.loads(data.decode('utf-8'))
            print(f"Returned info: {json_data}")

            if json_data.get("message") == 'stop':
                raise StopIteration("Finished algorithm on demand")

            break
        else:
            response = json.dumps({"action": "continue"})
            conn.send(response.encode('utf-8'))
            print(f"Waiting for ability to continue")
            data = conn.recv(4096)
            json_data = json.loads(data.decode('utf-8'))

            print(f"I got times {json_data['time']} {json_data['totalTime']}")

            if json_data.get("message") == 'stop':
                raise StopIteration("Finished algorithm on demand")

            print(f"Returned info: {json_data}")

    bestNEATGeneration = (population.generation + 1)

    least = None
    most = None
    avarage = 0

    for i, (genome_id, genome) in enumerate(genomes):
        avarage += genome.fitness
        if least is None or genome.fitness < least:
            least = genome.fitness
        if most is None or genome.fitness > most:
            fitestNEATAgent = genome
            most = genome.fitness

    for i in nets:
        agentTimes.append(json_data['time'])

    meanTime = sum(agentTimes) / len(agentTimes)
    bestTime = max(agentTimes)
    worstTime = min(agentTimes)

    maxFit.append(most)
    minFit.append(least)
    avgFit.append(avarage/len(genomes))

    maxTime.append(bestTime)
    minTime.append(worstTime)
    avgTime.append(meanTime)

    cpu, memory = next(resource_monitor)
    cpu_data.append(sum(cpu) / len(cpu))
    memory_data.append(memory)
    avg_cpu_data.append((sum(cpu_data) / len(cpu_data)))
    avg_memory_data.append((sum(memory_data) / len(memory_data)))

    cpuPloter(cpu_data, avg_cpu_data, memory_data, avg_memory_data, "", "NEAT")
    fitPloter(maxFit, minFit, avgFit, "", "NEAT")
    timePloter(maxTime, minTime, avgTime, "", "NEAT")

    if (population.generation+1) % 50 == 0 and fitestNEATAgent is not None:
        saveNEAT(folderName, bestNEATGeneration)
def saveNEAT(folderName, generationNumber):

    full_path = os.path.join(folderName, f"Generation_{generationNumber}")
    os.makedirs(f"{full_path}", exist_ok=True)
    full_path2 = os.path.join(full_path, f'NEAT_Model_Generation{generationNumber}.pkl')
    with open(full_path2, 'wb') as f:
        pickle.dump(fitestNEATAgent, f)

    cpuPloter(cpu_data, avg_cpu_data, memory_data, avg_memory_data, full_path, "NEAT")
    fitPloter(maxFit, minFit, avgFit, full_path, "NEAT")
    timePloter(maxTime, minTime, avgTime, full_path, "NEAT")
def saveDQN(folderName, generations):
    full_path = os.path.join(folderName, f"Generation_{generations}")
    os.makedirs(f"{full_path}", exist_ok=True)

    cpuPloter(cpu_data, avg_cpu_data, memory_data, avg_memory_data, full_path, "DQN")
    fitPloter(maxFit, minFit, avgFit, full_path, "DQN")
    timePloter(maxTime, minTime, avgTime, full_path, "DQN")





if __name__ == '__main__':
    start_server()
