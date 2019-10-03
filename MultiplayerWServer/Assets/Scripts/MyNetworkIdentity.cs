using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;
public class MyNetworkIdentity : MonoBehaviour
{
    [SerializeField] string id;
    [SerializeField] bool hasControl;
    SocketIOComponent socket;

    public string ID { get { return id; } }
    public bool HasControl { get { return hasControl; } }
    public SocketIOComponent Socket { get { return socket; } set { socket = value; } }    

    public void SetID(string _id)
    {
        if (Server.id == _id)
            hasControl = true;
        id = _id;
    }
}
