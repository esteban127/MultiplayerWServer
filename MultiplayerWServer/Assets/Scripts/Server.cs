using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;
using System;

public class Server : SocketIOComponent
{
    string id;
    Dictionary<string, GameObject> connectedPlayers;
    [SerializeField] GameObject PlayerPrefab;
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        connectedPlayers = new Dictionary<string, GameObject>();
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
            player.name = spawn.data["id"].ToString();
            connectedPlayers.Add(spawn.data["id"].ToString(), player);

        });

        On("onRegister", (register) =>
        {
            id = register.data["id"].ToString();
        });

        On("disconnect", (disconection) =>
        {
            GameObject objectToDestroy = connectedPlayers[id];
            Destroy(objectToDestroy);
            connectedPlayers.Remove(id);
            Debug.Log("Juan es un gato");
        });
        On("playerDisconnected", (disconection) =>
         {
             string id = disconection.data["id"].ToString();
             GameObject objectToDestroy = connectedPlayers[id];
             Destroy(objectToDestroy);
             connectedPlayers.Remove(id);
             Debug.Log("Juan es un gato");
         }
        );
        
    }
}
