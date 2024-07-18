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

    //data of agents send to server
    public class agentData
    {
        public bool alive = true;
        public int reward = 0;
    }

    //contain agents
    IDictionary<int, GameObject> agents = new Dictionary<int, GameObject>();
    //contain data of agents
    IDictionary<int, agentData> agentsPositions = new Dictionary<int, agentData>();
    //contain experiment environments
    IDictionary<int, GameObject> experienceScenes = new Dictionary<int, GameObject>();
    //contains targets in experiment
    List<GameObject> targets = new List<GameObject>();

    //Environments
    public GameObject preyTest;
    public GameObject orderTest;
    public GameObject labirynthTest;

    //Agents
    public GameObject Agent;
    public GameObject Food;

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
                    sendResponseToServerSync("AgentsCreated");
                    await runAI();
                    break;
            }
        }
    }
    public void createAgentsPrey(int numberOfAgents)
    {
        experienceScenes[0] = Instantiate(preyTest);

        SpriteRenderer myRenderer = experienceScenes[0].GetComponent<SpriteRenderer>();
        Vector2 spriteSize = myRenderer.bounds.size;
        Vector3 spriteCenter = myRenderer.bounds.center;

        Vector2 agentPosition = new Vector2(0, 0);
        Vector2 targetPosition = new Vector2(0, 0);

        float randomX = 0;
        float randomY = 0;


        for (int i = 0; i < numberOfAgents; i++)
        {
            do 
            {
                randomX =  Random.Range(spriteCenter.x - (spriteSize.x - 3) / 2, spriteCenter.x + (spriteSize.x - 3) / 2);
                randomY = Random.Range(spriteCenter.y - (spriteSize.y - 3) / 2, spriteCenter.y + (spriteSize.y - 3) / 2);

                agentPosition = new Vector2(Mathf.RoundToInt(randomX), Mathf.RoundToInt(randomY));
            } 
            while(Physics.OverlapSphere(agentPosition, 2).Length != 0);

            GameObject createdAgent = null;

            createdAgent = Instantiate(Agent, agentPosition, Quaternion.identity);

            agents.Add(i, createdAgent);
            agentsPositions.Add(i, new agentData());
        }

        for (int i = 0; i < 20; i++)
        {
            do
            {
                randomX = Random.Range(spriteCenter.x - (spriteSize.x - 3) / 2, spriteCenter.x + (spriteSize.x - 3) / 2);
                randomY = Random.Range(spriteCenter.y - (spriteSize.y - 3) / 2, spriteCenter.y + (spriteSize.y - 3) / 2);

                targetPosition = new Vector2(Mathf.RoundToInt(randomX), Mathf.RoundToInt(randomY));
            }
            while (Physics.OverlapSphere(targetPosition, 2).Length != 0);

            GameObject createdTarget = null;

            createdTarget = Instantiate(Food, targetPosition, Quaternion.identity);
            targets.Add(createdTarget);
        }

        Debug.Log("Creating agents: "+ numberOfAgents);
    }
    async Task runAI()
    {
        Debug.Log("Starting NEAT");

        masageFromServer = await receiveResponseFromServerAsync();

        if ((string)masageFromServer["action"] == "SendData")
        {
            //getting data - change to method collecting data
            foreach (var agent in agentsPositions)
            {
                agent.Value.reward = Random.Range(0, 11);
            }
            Debug.Log("Getting Data");

            //send data
            await sendAgentsDataToServer();
            Debug.Log("Sending Data");

            //wait for action
            masageFromServer = await receiveResponseFromServerAsync();
            Debug.Log("Reciving Action");
            Debug.Log("The actions: " + masageFromServer);

            //aply action
            Debug.Log("Apply Action");

            //get rewards
            foreach (var agent in agentsPositions)
            {
                agent.Value.reward = Random.Range(-10, 1);
            }
            Debug.Log("Getting rewards");

            //confirm update with rewards
            await sendAgentsDataToServer();
            Debug.Log("Send revards");

            //clearAgents();
            //Debug.Log("Clearing simulation");
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
        clearAgents();
        Debug.Log("Simulation stoped");

        turnOffSimulation();
        turnOnAlgorithm();
    }

    // sending and receiving information to/from server
    async Task sendResponseToServerAsync(string message = null, string network = null, int expNumber = 0)
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
