using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDirector : MonoBehaviour
{
    [SerializeField] MapManager mapManager;
    [SerializeField] MovementManager mov;
    Dictionary<string,GameObject> players;
    Server sv;
    [SerializeField] GameObject character0;
    [SerializeField] GameObject character1;
    Character localCharacter;
    private void Start()
    {
        sv = Server.Instance;
        GeneratePlayers();
        PositionCamera();
        Turn1();
    }

    private void Turn1()
    {
        mov.DrawTurn1Movement(localCharacter.Team);
    }

    private void PositionCamera()
    {
        if (localCharacter.Team == 0)
        {
            Camera.main.transform.RotateAround(new Vector3(9, 0, 9), new Vector3(0, 1, 0), 180);
        }
    }

    private void GeneratePlayers()
    {
        int team;
        int gap;
        GameObject newPlayer;
        Vector2 spawnPos;
        players = new Dictionary<string, GameObject>();
        foreach (string id in sv.ConnectedPlayers.Keys)
        {
            team = players.Count % 2;   
            
            newPlayer = team == 0? Instantiate(character0) : Instantiate(character1);           
            spawnPos = mapManager.GetSpawnBaseSpawnPoint(team);
            gap = (int)players.Count / 2;
            spawnPos.y -= gap <= 2 ? -(gap % 2 + 1) : gap % 2 + 1;
            newPlayer.GetComponent<Character>().Spawn(spawnPos, team);
            if (id == Server.id)
                localCharacter = newPlayer.GetComponent<Character>();
            players.Add(id,newPlayer);
        }
    }
}
