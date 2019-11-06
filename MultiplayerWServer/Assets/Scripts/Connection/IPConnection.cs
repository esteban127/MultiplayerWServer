using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IPConnection : MonoBehaviour
{
    string ip;

    public void TryToConnect()
    {
        Server.Instance.TryConnecToAnIP(ip);
    }
    public void SetIP(string newIp)
    {
        ip = newIp;
    }
}
