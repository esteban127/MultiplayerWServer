using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;
using System;

public class Server : SocketIOComponent
{
    static public string id;
    Dictionary<string, MyNetworkIdentity> connectedPlayers;
    [SerializeField] GameObject PlayerPrefab;
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

        On("disconnect", (disconection) =>
        {
            GameObject objectToDestroy = connectedPlayers[id].gameObject;
            Destroy(objectToDestroy);
            connectedPlayers.Remove(id);
            Debug.Log("Juan es un gato");
        });
        On("playerDisconnected", (disconection) =>
         {
             string id = disconection.data["id"].ToString();
             GameObject objectToDestroy = connectedPlayers[id].gameObject;
             Destroy(objectToDestroy);
             connectedPlayers.Remove(id);
             Debug.Log("Juan es un gato");
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