import socket
import json
import random


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
                        runNEAT()

                    if json_data.get("expNumber") % 2 == 1:
                        print(f"Chosen DQN")
                        runDQN()

        print(f"Closing server")

def runDQN():
    print(f"Create agents")
    agentsActions = {}
    for i in range(10):
        agentsActions[i] = 0

    print(f"Making client to create agents")
    response = json.dumps({"action": "create", "number": 10})
    conn.send(response.encode('utf-8'))

    print(f"Confirmation of client creation")
    data = conn.recv(4096)
    json_data = json.loads(data.decode('utf-8'))
    print(f"data send {json_data}")

    print(f"Ask client for old state")
    response = json.dumps({"action": "SendData"})
    conn.send(response.encode('utf-8'))

    print(f"Get old state")
    data = conn.recv(4096)
    json_data = json.loads(data.decode('utf-8'))
    print(f"Agents data {json_data}")

    print(f"Get action from data")
    for key in agentsActions:
        agentsActions[key] = random.randint(0, 3)

    print(f"Send action to client")
    response = json.dumps(agentsActions)
    conn.send(response.encode('utf-8'))

    print(f"Get new state with rewards")
    data = conn.recv(4096)
    json_data = json.loads(data.decode('utf-8'))
    print(f"Agents rewards {json_data}")

    print(f"Short memory training")
    print(f"Remembering")
    print(f"Long memory training")

    pass
def runNEAT():
    print(f"Create agents")
    agentsActions = {}
    for i in range(10):
        agentsActions[i] = 0

    print(f"Making client to create agents")
    response = json.dumps({"action": "create", "number": 10})
    conn.send(response.encode('utf-8'))

    print(f"Confirmation of client creation")
    data = conn.recv(4096)
    json_data = json.loads(data.decode('utf-8'))
    print(f"data send {json_data}")

    print(f"Ask client for state")
    response = json.dumps({"action": "SendData"})
    conn.send(response.encode('utf-8'))

    print(f"Get state")
    data = conn.recv(4096)
    json_data = json.loads(data.decode('utf-8'))
    print(f"Agents data {json_data}")

    print(f"Get action from data")
    for key in agentsActions:
        agentsActions[key] = random.randint(0, 3)

    print(f"Send action to client")
    response = json.dumps(agentsActions)
    conn.send(response.encode('utf-8'))

    print(f"Get rewards")
    data = conn.recv(4096)
    json_data = json.loads(data.decode('utf-8'))
    print(f"Agents rewards {json_data}")

    print(f"Setting fitness")
    pass

if __name__ == '__main__':
    start_server()
