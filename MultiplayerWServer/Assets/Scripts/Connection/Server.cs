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
    GameDirector currentGameDirector = null;
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
        currentGameDirector = instance;
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
            connectedPlayers = new Dictionary<string, MyNetworkIdentity>();
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
            GameObject spawneable = Instantiate(serverObj.GetObject(spawnObject.data["objectName"].ToString().Replace("\"", string.Empty)).prefav);
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
        On("newTurn", (NewTurn) =>
        {
            currentGameDirector.NewTurn();
        }
        );
        On("endTurn", (EndTurn) =>
        {
            currentGameDirector.EndOfTurn();
        }
        ); 
        On("recieveActionData", (RecievedData) =>
        {
            CharacterActionData data = JsonUtility.FromJson<CharacterActionData>(RecievedData.data.ToString());
            currentGameDirector.ReceiveActionToReplicate(data);
        }
        );
        On("startActionTurn", (actions) =>
        {
            currentGameDirector.StartReplication();
        }
        ); 
    }

    #region Emiters

    public void ReadyToEndTurn()
    {
        Emit("readyToEndTurn");
    }
    public void CancelReaedyToEndTurn()
    {
        Emit("cancelReadyToEndTurn");
    }
    public void SubmitAction(CharacterActionData data)
    {
        Emit("submitActionData", new JSONObject(JsonUtility.ToJson(data)));
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
    public void StartGame()
    {
        Emit("gameHosted");
        ChangeScene("GameScene");
    }
    public void ReplicationEnded()
    {        
        Emit("replicationEnded");        
    }
    #endregion



    #region lobbyRelated
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
    #endregion

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
#region DataClasses
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
public class CharacterActionData
{
    public string id;
    public List<MovList> mov;
    public bool sprinting;
    public string followID;
    public List<int> skillsID;
    public List<Vector2Data> skillsTarget;


    public CharacterActionData()
    {
        mov = new List<MovList>();
        skillsID = new List<int>();
        skillsTarget = new List<Vector2Data>();
    }
    public List<Vector2> GetMov()
    {
        List<Vector2> movToReturn = new List<Vector2>();
        Vector2 lastvector = new Vector2(-1,-1);
        foreach (MovList list in mov)
        {
            foreach (Vector2Data pos in list.list)
            {
                if(pos.x != lastvector.x || pos.y != lastvector.y)
                {
                    movToReturn.Add(new Vector2(pos.x, pos.y));
                    lastvector.x = pos.x;
                    lastvector.y = pos.y;
                }                
            }
        }
        return movToReturn;
    }
    public void SetMov(List<List<Vector2>> newMovs)
    {
        mov = new List<MovList>();
        MovList provitionalList;
        foreach (List<Vector2> list in newMovs)
        {
            provitionalList = new MovList();
            foreach (Vector2 pos in list)
            {
                provitionalList.list.Add(new Vector2Data((int)pos.x,(int)pos.y));
            }
            mov.Add(provitionalList);
        }
    }
    public void SetSkills(Dictionary<int, Vector2> newSkills)
    {
        foreach(int key in newSkills.Keys)
        {
            skillsID.Add(key);
            skillsTarget.Add(new Vector2Data((int)(newSkills[key].x*100), (int)(newSkills[key].y*100)));
        }
    }
    public Dictionary<int,Vector2> GetSkills()
    {
        Dictionary<int, Vector2> skillsToReturn = new Dictionary<int, Vector2>();
        for (int i = 0; i < skillsID.Count; i++)
        {
            skillsToReturn.Add(skillsID[i], new Vector2((float)skillsTarget[i].x/100, (float)skillsTarget[i].y/100));
        }            
        
        return skillsToReturn;
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
public class MovList
{
    public MovList()
    {
        list = new List<Vector2Data>();
    }
    public List<Vector2Data> list;    
}
[System.Serializable]
public class Vector2Data
{
    public Vector2Data(int _x, int _y)
    {
        x = _x;
        y = _y;
    }
    public int x;
    public int y;
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
    public ServerObject GetObject(string key)
    {
        return serverObjects[key];
    }
}
#endregion