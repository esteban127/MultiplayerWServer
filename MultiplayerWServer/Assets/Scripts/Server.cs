using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;
using System;
using System.Text;

public class Server : SocketIOComponent
{
    static public string id;
    Dictionary<string, MyNetworkIdentity> connectedPlayers;
    [SerializeField] GameObject PlayerPrefab;
    [SerializeField] Dictionary<string,ServerObject> SpawnedObjects;
    [SerializeField] ServerObjectManager serverObj;
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        connectedPlayers = new Dictionary<string, MyNetworkIdentity>();
        Hook();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Close();
            Debug.Log(id);
        }
    }

    private void Hook()
    {
        On("open", (connect) =>
        {
            Debug.Log("Me cunecte");
        });

        On("spawn", (spawn) =>
        {
            Debug.Log("Oli");
            GameObject player = Instantiate(PlayerPrefab);
            player.GetComponent<MyNetworkIdentity>().SetID(spawn.data["id"].ToString());
            player.GetComponent<MyNetworkIdentity>().Socket = this;
            player.name = spawn.data["id"].ToString();
            connectedPlayers.Add(spawn.data["id"].ToString(), player.GetComponent<MyNetworkIdentity>());
        });

        On("onRegister", (register) =>
        {
            id = register.data["id"].ToString();
        });
        On("updateIn", (updateTransform) =>
        {
            float x = updateTransform.data["position"]["x"].f;
            float y = updateTransform.data["position"]["y"].f;
            float z = updateTransform.data["position"]["z"].f;
            connectedPlayers[updateTransform.data["id"].ToString()].gameObject.transform.position = new Vector3(x, y, z);
            Debug.Log(updateTransform.data["id"].ToString());
        });

        On("spawnObj", (spawnObject)=>{
            GameObject spawneable = Instantiate(serverObj.getObject(spawnObject.data["objectName"].ToString()).prefav);
            float x = spawnObject.data["position"]["x"].f;
            float y = spawnObject.data["position"]["y"].f;
            float z = spawnObject.data["position"]["z"].f;
            spawneable.transform.position = new Vector3(x, y, z);
            SpawnedObjects.Add(spawnObject.data["id"].ToString(), new ServerObject());
        });
        On("unspawnObj", (unspawnObject) => {
            string objID = unspawnObject.data["id"].ToString();
            GameObject objectToDestroy = SpawnedObjects[objID].prefav;
            Destroy(objectToDestroy);
            SpawnedObjects.Remove(objID);
        });
        On("updateObj", (updateObj) => {
            string objID = updateObj.data["id"].ToString();
            GameObject objectToDestroy = SpawnedObjects[objID].prefav;
            Destroy(objectToDestroy);
            SpawnedObjects.Remove(objID);
        });

        On("disconnect", (disconection) =>
        {
            GameObject objectToDestroy = connectedPlayers[id].gameObject;
            Destroy(objectToDestroy);
            connectedPlayers.Remove(id);
        });
        On("playerDisconnected", (disconection) =>
         {
             string playerID = disconection.data["id"].ToString();
             GameObject objectToDestroy = connectedPlayers[playerID].gameObject;
             Destroy(objectToDestroy);
             connectedPlayers.Remove(playerID);
         }
        );        
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
class Vector3Data
{
    public float x;
    public float y;
    public float z;
}

[System.Serializable]
class ServerObject
{
    public GameObject prefav;
    public string ID;
}

class ServerObjectManager : MonoBehaviour
{
    GameObject Data;
    [SerializeField]
    Dictionary<string, ServerObject> serverObjects;
    public ServerObject getObject(string key)
    {
        return serverObjects[key];
    }
}