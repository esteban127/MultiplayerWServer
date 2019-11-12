using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;
using UnityEngine.UI;
using System;
using System.Linq;
using UnityEngine.SceneManagement;

public class Server : SocketIOComponent
{
    static public string id;    
    Dictionary<string, MyNetworkIdentity> connectedPlayers;
    public Dictionary<string, MyNetworkIdentity>  ConnectedPlayers { get { return connectedPlayers; } }
    [SerializeField] Dictionary<string, ServerObject> SpawnedObjects;
    [SerializeField] Lobby hostingLobby;
    [SerializeField] GameObject connectingW;
    [SerializeField] GameObject failedToConnectW;
    GameDirector currentGameDirectror = null;
    ServerObjectManager serverObj;
    bool isHost = false;
    public bool IsHost { get { return isHost; } }
    string hostIP;
    string playerName = "Pedro";
    bool serverConnection = false;
    string currentScene = "HostScene";
    public string CurrentScene { get { return currentScene; } }
    [SerializeField] string port = "8080";
    public delegate void SaveDelegate();
    public static event SaveDelegate BeforeClosing;
    static private Server instance = null;
    static public Server Instance { get { return instance; } }

    public void SetGameDirector(GameDirector instance)
    {
        currentGameDirectror = instance;
    }

    protected override void Awake()
    {
        base.Awake();
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;            
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        serverObj = new ServerObjectManager();
        connectedPlayers = new Dictionary<string, MyNetworkIdentity>();
        Hook();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();        
    }
    private void Hook()
    {
        On("open", (connect) =>
        {
            Debug.Log("connected");
        });

        On("playerJoin", (join) =>
        {
            GameObject playerLabel = hostingLobby.AddPlayer(join.data["name"].ToString().Replace("\"", string.Empty));
            if (isHost)
            {
                if(join.data["id"].ToString().Replace("\"", string.Empty) == id)
                {
                    playerLabel.GetComponentInChildren<Button>().onClick.AddListener(() => CloseLobby());
                }
                else
                {
                    playerLabel.GetComponentInChildren<Button>().onClick.AddListener(() => KickPlayer(join.data["id"].ToString().Replace("\"", string.Empty)));
                }
            }
            else
            {
                if (join.data["id"].ToString().Replace("\"", string.Empty) == id)
                {
                    playerLabel.GetComponentInChildren<Button>().onClick.AddListener(() => LeaveLobby());
                }
                else
                {
                    playerLabel.GetComponentInChildren<Button>().gameObject.SetActive(false);
                }
            }
            
            MyNetworkIdentity playerID = playerLabel.GetComponent<MyNetworkIdentity>();
            playerID.SetID(join.data["id"].ToString().Replace("\"", string.Empty));
            playerID.Socket = this;            
            connectedPlayers.Add(join.data["id"].ToString().Replace("\"", string.Empty), playerID);
        });
        On("kicked", (kicked) =>
        {
            LeaveLobby();
        });
        On("onRegister", (register) =>
        {
            string a = register.data["id"].ToString().Replace("\"", string.Empty);            
            Debug.Log(a);
            id = register.data["id"].ToString().Replace("\"",string.Empty);
            SimpleMessage nameData = new SimpleMessage();
            nameData.message = playerName;
            Emit("setName", new JSONObject(JsonUtility.ToJson(nameData)));            
            serverConnection = true;
            Emit("joinLobby");
        });
        /*
        On("updateIn", (updateTransform) =>
        {
            float x = updateTransform.data["position"]["x"].f;
            float y = updateTransform.data["position"]["y"].f;
            float z = updateTransform.data["position"]["z"].f;
            connectedPlayers[updateTransform.data["id"].ToString()].gameObject.transform.position = new Vector3(x, y, z);
            Debug.Log(updateTransform.data["id"].ToString());
        });*/

        On("spawnObj", (spawnObject) => {
            GameObject spawneable = Instantiate(serverObj.getObject(spawnObject.data["objectName"].ToString().Replace("\"", string.Empty)).prefav);
            float x = spawnObject.data["position"]["x"].f;
            float y = spawnObject.data["position"]["y"].f;
            float z = spawnObject.data["position"]["z"].f;
            spawneable.transform.position = new Vector3(x, y, z);
            SpawnedObjects.Add(spawnObject.data["id"].ToString().Replace("\"", string.Empty), new ServerObject());
        });
        On("unspawnObj", (unspawnObject) => {
            string objID = unspawnObject.data["id"].ToString().Replace("\"", string.Empty);
            GameObject objectToDestroy = SpawnedObjects[objID].prefav;
            Destroy(objectToDestroy);
            SpawnedObjects.Remove(objID);
        });
        On("updateObj", (updateObj) => {
            string objID = updateObj.data["id"].ToString().Replace("\"", string.Empty);
            GameObject objectToDestroy = SpawnedObjects[objID].prefav;
            Destroy(objectToDestroy);
            SpawnedObjects.Remove(objID);
        });

        On("disconnect", (disconection) =>
        {
            serverConnection = false;
            GameObject objectToDestroy = connectedPlayers[id].gameObject;
            Destroy(objectToDestroy);
            connectedPlayers.Remove(id);
        });
        On("playerDisconnected", (disconection) =>
         {
             string playerID = disconection.data["id"].ToString().Replace("\"", string.Empty);
             GameObject objectToDestroy = connectedPlayers[playerID].gameObject;
             Destroy(objectToDestroy);
             connectedPlayers.Remove(playerID);
         }
        );
        On("startGame",(start) =>
         {
             ChangeScene("GameScene");
         }
        );
    }

    public void ReadyToEndTurn()
    {

    }

    public void JoinLobby()
    {        
        hostingLobby.Join(isHost);        
    }
    public void LeaveLobby()
    {
        hostingLobby.Leave();
        isHost = false;
        Close();
    }
    public void KickPlayer(string kickId)
    {
        SimpleMessage idData = new SimpleMessage();
        idData.message = kickId;
        Emit("kick", new JSONObject(JsonUtility.ToJson(idData)));
    }
    public void CloseLobby()
    {
        Emit("closeLobby");
    }
    public void SetName(string name)
    {
        playerName = name;
    }

    public void HostLocal()
    {
        hostIP = "127.0.0.1";
        SetSocket("ws://127.0.0.1:" + port + "/socket.io/?EIO=4&transport=websocket");
        Connect();
        isHost = true;
        JoinLobby(); 
    }  

    public void TryConnecToAnIP(string ip)
    {
        hostIP = ip;
        SetSocket("ws://" + hostIP + ":" + port + "/socket.io/?EIO=4&transport=websocket");
        Connect();
        StartCoroutine(ConnectingToAnIP());        
    }

    IEnumerator ConnectingToAnIP()
    {
        connectingW.SetActive(true);
        for (int i = 0; i < 50; i++)
        {
            yield return new WaitForSeconds(.1f);
            if (serverConnection)
            {
                connectingW.SetActive(false);
                JoinLobby();
                yield break;
            }
        }
        connectingW.SetActive(false);
        failedToConnectW.SetActive(true);
    }

    public void AbortConnection()
    {
        StopCoroutine(ConnectingToAnIP());
        connectingW.SetActive(false);
        failedToConnectW.SetActive(true);
    }

    public void StartGame()
    {
        Emit("gameHosted");
        ChangeScene("GameScene");
    }
    public void ChangeScene(string sceneName)
    {
        if (BeforeClosing != null)
        {
            BeforeClosing();
            CleanDelegate();
        }
        currentScene = sceneName;
        SceneManager.LoadScene("LoadingScreen", LoadSceneMode.Single);
    }

    public void QuitApplication()
    {
        if (BeforeClosing != null)
        {
            BeforeClosing();
            CleanDelegate();
        }
        Application.Quit();
    }

    private void CleanDelegate()
    {
        Delegate[] functions = BeforeClosing.GetInvocationList();
        for (int i = 0; i < functions.Length; i++)
        {
            BeforeClosing -= (SaveDelegate)functions[i];
        }
    }

}

[System.Serializable]
class PlayerData
{
    public string id;
    public Vector3Data position;
    public PlayerData()
    {
        position = new Vector3Data();
    }
}
[System.Serializable]
class SimpleMessage
{
    public string message;   
}
[System.Serializable]
class Vector3Data
{
    public float x;
    public float y;
    public float z;
}
[System.Serializable]
class MovData
{
    public List<List<Vector2Data>> movs;
}
[System.Serializable]
class Vector2Data
{
    public float x;
    public float y;
}

[System.Serializable]
class ServerObject
{
    public GameObject prefav;
    public string ID;
}

class ServerObjectManager : MonoBehaviour
{
    [SerializeField] Dictionary<string, ServerObject> serverObjects;
    public ServerObjectManager()
    {
        serverObjects = new Dictionary<string, ServerObject>();
    }
    public ServerObject getObject(string key)
    {
        return serverObjects[key];
    }
}