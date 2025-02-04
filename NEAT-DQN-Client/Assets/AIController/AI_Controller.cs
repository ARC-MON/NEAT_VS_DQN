using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using TMPro;
using UnityEngine;
using Newtonsoft.Json;
using System.Text;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using Task = System.Threading.Tasks.Task;
using UnityEditor.VersionControl;
using Random = UnityEngine.Random;
using System.Threading.Tasks;
using System.Linq;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class AI_Controller : MonoBehaviour
{
    //Connect pannel UI
    [SerializeField]
    private TMP_InputField _address;
    [SerializeField]
    private TMP_InputField _port;

    //Algorithm pannel UI
    [SerializeField]
    private TMP_Dropdown _dropdown;
    [SerializeField]
    private TMP_InputField _file;

    //UI panels
    public GameObject connectPanel;
    public GameObject algorithmPanel;
    public GameObject simulationPanel;

    //Server connection variables
    private TcpClient client;
    private NetworkStream stream;

    //variables for comunication
    private string jsonData;
    private byte[] data;
    private byte[] buffer = new byte[4096];
    private int bytesRead;
    private JObject masageFromServer;

    //simulation only variables
    bool doSimulation = true;
    List<int> keysToRemove = new List<int>();
    bool afterAction = true;
    public int numberOfMoves = 0;

    //timers
    [SerializeField]
    private Timer _Timer;

    //scene creation
    Vector2 sceneSize;
    Vector3 sceneScale;
    float spacingX;
    float spacingY;
    SpriteRenderer sceneSprite;
    float xPosition;
    float yPosition;

    //data of agents send to server
    public class agentData
    {
        public bool alive = true;
        public string reason = "";
        public int reward = 0;
        public int x = 0;
        public int y = 0;

        public int moveNumber = 0;
        public int lifeTime = 0;
        public List<int> who = new List<int>();
        public List<int> distance = new List<int>();
    }

    //contain agents
    IDictionary<int, GameObject> agents = new Dictionary<int, GameObject>();
    //contain data of agents
    IDictionary<int, agentData> agentsPositions = new Dictionary<int, agentData>();
    //contain experiment environments
    IDictionary<int, GameObject> experienceScenes = new Dictionary<int, GameObject>();
    //contains targets in experiment
    List<GameObject> targets = new List<GameObject>();
    //enemies
    IDictionary<int, List<GameObject>> enemiesDictionary = new Dictionary<int, List<GameObject>>();

    //Environments
    public GameObject preyTest;
    public GameObject orderTest;
    public GameObject labirynthTest;

    //Agents
    public GameObject Agent;
    public GameObject Food;
    public GameObject Enemy;

    private void Start()
    {
        _address.text = "127.0.0.1";
        _port.text = "12345";
    }

    // connect to python server
    public void connectToServer()
    {
        if (_address.text == "" || _port.text == "")
        {
            if (_address.text == "")
                Debug.Log("IP address missing");
            if (_port.text == "")
                Debug.Log("Port missing");
        }
        else
        {
            try
            {
                client = new TcpClient(_address.text, int.Parse(_port.text));
                stream = client.GetStream();

                Debug.Log("Connected to python server");

                turnOffConnect();
                turnOnAlgorithm();
            }
            catch (Exception e)
            {
                Debug.Log("Failed to connect to server: " + e.Message);
            }
        }
    }

    // disconnect from python server
    public void disconnectFromServer()
    {
        if (client != null)
            client.Close();
        if (stream != null)
            stream.Close();

        Debug.Log("Disconnected from server");

        turnOffAlgorithm();
        turnOnConnect();
    }

    async public void startSimulation()
    {
        if(_file.text == "")
            sendResponseToServerSync("Start", null, _dropdown.value);
        else
            sendResponseToServerSync("Start", _file.text, _dropdown.value);

        Debug.Log("Simulation starting...");

        turnOffAlgorithm();
        turnOnSimulation();

        masageFromServer = receiveResponseFromServerSync();
        if ((string)masageFromServer["action"] == "create")
        {

            switch (_dropdown.value)
            {
                case 0:
                    createAgentsPrey((int)masageFromServer["number"]);
                    sendResponseToServerSync("AgentsCreated");
                    await runAI();
                    break;
                case 1:
                    createAgentsPrey((int)masageFromServer["number"]);
                    sendResponseToServerSync("AgentsCreated");;
                    await runAI();
                    break;
            }
        }
    }
    public void createAgentsPrey(int numberOfAgents)
    {
        sceneSprite = preyTest.GetComponent<SpriteRenderer>();
        sceneSize = sceneSprite.sprite.bounds.size;
        sceneScale = preyTest.transform.localScale;

        spacingX = sceneSize.x * sceneScale.x;
        spacingY = sceneSize.y * sceneScale.y;

        for (int i = 0; i < numberOfAgents; i++)
        {
            xPosition = (i % 5) * (spacingX+10);
            yPosition = -(i / 5) * (spacingY+10);
            experienceScenes[i] = Instantiate(preyTest, new Vector3(xPosition, yPosition, 0), Quaternion.identity);

            sceneSprite = experienceScenes[i].GetComponent<SpriteRenderer>();

            Vector2 spriteSize = sceneSprite.bounds.size;
            Vector3 spriteCenter = sceneSprite.bounds.center;

            Vector2 agentPosition = new Vector2(0, 0);
            Vector2 targetPosition = new Vector2(0, 0);
            Vector2 enemyPosition = new Vector2(0, 0);

            float randomX = 0;
            float randomY = 0;

            makeFood(60, experienceScenes[i]);

            do
            {
                randomX = Random.Range(spriteCenter.x - (spriteSize.x - 3) / 2, spriteCenter.x + (spriteSize.x - 3) / 2);
                randomY = Random.Range(spriteCenter.y - (spriteSize.y - 3) / 2, spriteCenter.y + (spriteSize.y - 3) / 2);

                agentPosition = new Vector2(Mathf.RoundToInt(randomX), Mathf.RoundToInt(randomY));
            }
            while (Physics.OverlapSphere(agentPosition, 2).Length != 0);

            GameObject createdAgent = null;

            createdAgent = Instantiate(Agent, agentPosition, Quaternion.identity, experienceScenes[i].transform);
            createdAgent.transform.localScale = new Vector2(0.022f,0.05f);

            agents.Add(i, createdAgent);
            agentsPositions.Add(i, new agentData());
            enemiesDictionary[i] = new List<GameObject>();

            for (int j = 0; j < 4; j++)
            {
                do
                {
                    randomX = Random.Range(spriteCenter.x - (spriteSize.x - 3) / 2, spriteCenter.x + (spriteSize.x - 3) / 2);
                    randomY = Random.Range(spriteCenter.y - (spriteSize.y - 3) / 2, spriteCenter.y + (spriteSize.y - 3) / 2);

                    enemyPosition = new Vector2(Mathf.RoundToInt(randomX), Mathf.RoundToInt(randomY));
                }
                while (Physics.OverlapSphere(enemyPosition, 2).Length != 0);

                GameObject createdEnemy = null;
                createdEnemy = Instantiate(Enemy, enemyPosition, Quaternion.identity, experienceScenes[i].transform);
                createdEnemy.transform.localScale = new Vector2(0.01f, 0.02f);

                enemiesDictionary[i].Add(createdEnemy);
            }
        }
    }

    public void makeFood(int numberOfFood, GameObject scene)
    {
        Vector2 targetPosition = new Vector2(0, 0);

        sceneSprite = scene.GetComponent<SpriteRenderer>();

        Vector2 spriteSize = sceneSprite.bounds.size;
        Vector3 spriteCenter = sceneSprite.bounds.center;

        float randomX = 0;
        float randomY = 0;

        for (int j = 0; j < numberOfFood; j++)
        {
            do
            {
                randomX = Random.Range(spriteCenter.x - (spriteSize.x - 3) / 2, spriteCenter.x + (spriteSize.x - 3) / 2);
                randomY = Random.Range(spriteCenter.y - (spriteSize.y - 3) / 2, spriteCenter.y + (spriteSize.y - 3) / 2);

                targetPosition = new Vector2(Mathf.RoundToInt(randomX), Mathf.RoundToInt(randomY));
            }
            while (Physics.OverlapSphere(targetPosition, 2).Length != 0);

            GameObject createdTarget = null;

            createdTarget = Instantiate(Food, targetPosition, Quaternion.identity, scene.transform);
            createdTarget.transform.localScale = new Vector2(0.01f, 0.02f);

            targets.Add(createdTarget);
        }

    }

    async Task runAI()
    {
        Debug.Log("Starting NEAT");
        numberOfMoves = 0;
        _Timer.on();

        while (doSimulation)
        {
            masageFromServer = await receiveResponseFromServerAsync();
            Debug.Log("Waiting for order");

            if ((string)masageFromServer["action"] == "SendData")
            {
                //getting data - change to method collecting data
                Debug.Log("Getting Data");


                foreach (var agent in agents)
                {
                    (agentsPositions[agent.Key].x, agentsPositions[agent.Key].y, agentsPositions[agent.Key].who, agentsPositions[agent.Key].distance, agentsPositions[agent.Key].lifeTime) = agent.Value.GetComponent<AI_PreyAgent>().getState();
                }

                //send data
                await sendAgentsDataToServer();
                Debug.Log("Sending Data");

                //wait for action
                masageFromServer = await receiveResponseFromServerAsync();
                Debug.Log($"{masageFromServer}");
                Debug.Log("Reciving Action");
                Debug.Log("The actions: " + masageFromServer);

                //aply action
                Debug.Log("Apply Action");
                foreach (var agent in agents)
                {
                    if (!(masageFromServer[agent.Key.ToString()] == null))
                    {
                        afterAction = agent.Value.GetComponent<AI_PreyAgent>().move((int)masageFromServer[agent.Key.ToString()]);
                        //enemy move
                        foreach (var enemy in enemiesDictionary[agent.Key])
                        {
                            afterAction = enemy.GetComponent<AI_Predatoragent>().makeMove(agent.Value);
                        }
                        afterAction = agent.Value.GetComponent<AI_PreyAgent>().detect();
                        (agentsPositions[agent.Key].x, agentsPositions[agent.Key].y, agentsPositions[agent.Key].who, agentsPositions[agent.Key].distance, agentsPositions[agent.Key].lifeTime) = agent.Value.GetComponent<AI_PreyAgent>().getState();
                    }
                }

                numberOfMoves++;

                foreach (var agent in agents)
                {
                    agentsPositions[agent.Key].moveNumber = numberOfMoves;
                }

                //get rewards
                foreach (var agent in agentsPositions)
                {
                    agent.Value.reward = agents[agent.Key].GetComponent<AI_PreyAgent>().reward;
                    agents[agent.Key].GetComponent<AI_PreyAgent>().reward = 0;

                    if (agents[agent.Key].GetComponent<AI_PreyAgent>().hp <= 0)
                    {
                        agent.Value.alive = false;
                        agent.Value.reason = "food";
                        agent.Value.reward = -10;
                    }
                    else if (agents[agent.Key].GetComponent<AI_PreyAgent>().captured == true)
                    {
                        agent.Value.alive = false;
                        agent.Value.reason = "enemy";
                        agent.Value.reward = -20;
                    }
                    else
                        agent.Value.alive = true;
                }
                Debug.Log("Getting rewards");

                //confirm update with rewards
                await sendAgentsDataToServer();
                Debug.Log("Send revards");

                foreach (var agent in agentsPositions)
                {
                    agent.Value.reward = 0;
                }

                //deliting dead agents

                keysToRemove.Clear();

                foreach (KeyValuePair<int, GameObject> pair in agents)
                {
                    if (agentsPositions[pair.Key].alive == false)
                    {
                        keysToRemove.Add(pair.Key);
                    }
                }

                foreach (int key in keysToRemove)
                {
                    Destroy(agents[key]);
                    agents.Remove(key);
                    agentsPositions.Remove(key);
                }

                Debug.Log("Agents deleted");

                //looking for next generation
                masageFromServer = await receiveResponseFromServerAsync();
                Debug.Log("If next generation");

                if ((string)masageFromServer["action"] == "new_gen")
                    break;

                await sendResponseToServerAsync("We can continue",null,0, numberOfMoves, (int)_Timer._currentTotalTime);

                //clearAgents();
                //Debug.Log("Clearing simulation");
            }
        }

        if (doSimulation)
        {
            Debug.Log("New generation");
            await sendResponseToServerAsync("We can do next generation", null, 0, numberOfMoves, (int)_Timer._currentTotalTime);


            _Timer.off();
            _Timer.nextGeneration();

            makeNextGeneration();
        }
        else 
        {

            _Timer.off();
            _Timer.nextGeneration();

            masageFromServer = await receiveResponseFromServerAsync();
            sendResponseToServerSync("stop");
            clearAgents();
            _Timer.restartTimers();
        }
    }
    async void makeNextGeneration()
    {
        masageFromServer = receiveResponseFromServerSync();

        if((string)masageFromServer["action"] == "create")
        {
            clearAgents();
            createAgentsPrey((int)masageFromServer["number"]);
            sendResponseToServerSync("AgentsCreated");

            await runAI();
        }
    }
    public void clearAgents()
    {
        agentsPositions.Clear();

        foreach (var item in targets)
        {
            Destroy(item);
        }
        targets.Clear();

        foreach (var item in agents)
        {
            Destroy(item.Value);
        }
        agents.Clear();

        foreach (var item in experienceScenes)
        {
            Destroy(item.Value);
        }
        experienceScenes.Clear();

        foreach (var list in enemiesDictionary)
        {
            foreach (var enemy in list.Value)
            {
                Destroy(enemy);
            }
            list.Value.Clear();
        }
        enemiesDictionary.Clear();
    }

    async Task sendAgentsDataToServer()
    {
        jsonData = JsonConvert.SerializeObject((agentsPositions));
        if (stream != null && stream.CanWrite)
        {
            data = Encoding.UTF8.GetBytes(jsonData);
            await stream.WriteAsync(data, 0, data.Length);
        }
    }

    public void stopSimulation()
    {
        doSimulation = false;
        Debug.Log("Simulation stoped");

        turnOffSimulation();
        turnOnAlgorithm();
    }

    // sending and receiving information to/from server
    async Task sendResponseToServerAsync(string message = null, string network = null, int expNumber = 0, int time = 0, int totalTime = 0)
    {
        var response = new
        {
            message = message,
            network = network,
            expNumber = expNumber,
            time = time,
            totalTime = totalTime
        };

        jsonData = JsonConvert.SerializeObject(response);

        if (stream != null && stream.CanWrite)
        {
            data = Encoding.UTF8.GetBytes(jsonData);
            await stream.WriteAsync(data, 0, data.Length);
        }
    }
    void sendResponseToServerSync(string message = null, string network = null, int expNumber = 0)
    {
        var response = new
        {
            message = message,
            network = network,
            expNumber = expNumber
        };

        jsonData = JsonConvert.SerializeObject(response);
        if (stream != null && stream.CanWrite)
        {
            data = Encoding.UTF8.GetBytes(jsonData);
            stream.Write(data, 0, data.Length);
        }
    }
    async Task<JObject> receiveResponseFromServerAsync()
    {
        bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
        if (bytesRead > 0)
        {
            jsonData = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            return JObject.Parse(jsonData);
        }
        else { return null; }
    }
    JObject receiveResponseFromServerSync()
    {
        bytesRead = stream.Read(buffer, 0, buffer.Length);

        if (bytesRead > 0)
        {
            jsonData = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            return JObject.Parse(jsonData);
        }
        else { return null; }
    }

    //UI controller
    public void turnOnConnect()
    {
        connectPanel.SetActive(true);
    }

    public void turnOffConnect()
    {
        connectPanel.SetActive(false);
    }

    public void turnOnAlgorithm()
    {
        algorithmPanel.SetActive(true);
    }

    public void turnOffAlgorithm()
    {
        algorithmPanel.SetActive(false);
    }

    public void turnOnSimulation()
    {
        simulationPanel.SetActive(true);
    }

    public void turnOffSimulation()
    {
        simulationPanel.SetActive(false);
    }
}
