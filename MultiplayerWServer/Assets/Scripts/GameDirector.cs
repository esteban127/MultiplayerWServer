using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum CharacterAction
{
    ability1,
    ability2,
    ability3,
    ability4,
    ability5,
    cat1,
    cat2,
    cat3,
    mov
}

public class GameDirector : MonoBehaviour
{
    [SerializeField] MapManager mapManager;
    MovementManager movManager;
    Dictionary<string,GameObject> players;
    Server sv;
    [SerializeField] GameObject character0;
    [SerializeField] GameObject character1;
    [SerializeField] LineRenderer lineRenderer;
    List<List<Vector2>> mov;
    List<CharacterAction> actionLog;
    Node currentNode;
    int currentMoveScore;
    Character localCharacter;
    bool turn1 = false;

    #region Local

    private void Start()
    {
        sv = Server.Instance;
        movManager = GetComponent<MovementManager>();
        GeneratePlayers();
        PositionCamera();
        Turn1();
        mov = new List<List<Vector2>>();
        actionLog = new List<CharacterAction>();
    }

    private void Turn1()
    {
        turn1 = true;
        currentNode = mapManager.GetNodeFromACoord(localCharacter.Pos);
        movManager.DrawTurn1Movement(localCharacter.Team);        
    }

    private void NewTurn()
    {
        localCharacter.ResetTurnValues();
        currentNode = mapManager.GetNodeFromACoord(localCharacter.Pos);
        currentMoveScore = localCharacter.MovScore;
        movManager.DrawMovementWalkableRange(currentNode,currentMoveScore);
    }

    private void PositionCamera()
    {
        if (localCharacter.Team == 0)
        {
            Camera.main.transform.RotateAround(new Vector3(9, 0, 9), new Vector3(0, 1, 0), 180);
        }
    }
    public void MovCommand(Node cell)
    {
        List<Vector2> CurrentMov = new List<Vector2>();
        if (turn1)
        {
            mov = new List<List<Vector2>>();
            Node[] nodes = movManager.GetPath(currentNode, cell);
            CurrentMov.Add(currentNode.Pos);
            for (int i = 0; i < nodes.Length; i++)
            {
                CurrentMov.Add(nodes[i].Pos);
            }
            mov.Add(CurrentMov);
            if(actionLog.Count == 0 || actionLog[0] != CharacterAction.mov)
                actionLog.Add(CharacterAction.mov);
        }
        else
        {
            actionLog.Add(CharacterAction.mov);
            Node[] nodes = movManager.GetCalculatedPath(currentNode, cell);
            CurrentMov.Add(currentNode.Pos);
            for (int i = 0; i < nodes.Length; i++)
            {
                CurrentMov.Add(nodes[i].Pos);
            }
            mov.Add(CurrentMov);
            currentNode = cell;
            currentMoveScore -= cell.g;
            movManager.ResetFloorColor();
            movManager.DrawMovementWalkableRange(currentNode, currentMoveScore);
        }

        DrawMov();

        //normalMov

    }

    public void DrawMov()
    {              
        List<Vector3> positions = new List<Vector3>();      

        foreach (List<Vector2> lists in mov)
        {
            foreach(Vector2 vectors in lists)
            {
                positions.Add(new Vector3(vectors.x, 0, vectors.y));
            }
        }
        lineRenderer.positionCount = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());
        
    }

    public void SprintCommand(Node cell)
    {
        actionLog.Add(CharacterAction.mov);
    }

    void EndOfTurn()
    {
        if (turn1)
            turn1 = false;
        mov = new List<List<Vector2>>();
    }

    public void DeleteLastAction()
    {
        if (actionLog.Count > 0)
        {
            switch (actionLog[actionLog.Count - 1])
            {
                case CharacterAction.mov:
                    movManager.GetPath(mapManager.GetNodeFromACoord(mov[mov.Count - 1][0]), currentNode);
                    currentMoveScore = currentMoveScore + currentNode.g;
                    currentNode = mapManager.GetNodeFromACoord(mov[mov.Count - 1][0]);
                    mov.RemoveAt(mov.Count - 1);
                    DrawMov();
                    movManager.ResetFloorColor();
                    movManager.DrawMovementWalkableRange(currentNode, currentMoveScore);
                    break;
            }
        }        
    }

    private void GeneratePlayers()
    {
        int team;
        int gap;
        GameObject newPlayer;
        Vector2 spawnPos;
        players = new Dictionary<string, GameObject>();
        List<string> keys = sv.ConnectedPlayers.Keys.ToList();
        keys.Sort();
        foreach (string id in keys)
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
#endregion

    #region Replication

    public void Replicate(List<CharacterActionData> actions)
    {
        foreach(CharacterActionData data in actions)
        {
            if (data.mov.Count > 0)
                StartCoroutine(ReplicatedMove(data.mov, data.ID));
        }
    }

    IEnumerator ReplicatedMove(List<List<Vector2>> path, string id)
    {
        int nodeCounter = 0;
        Character movingPlayer = players[id].GetComponent<Character>();        
        List<Node> nodes = new List<Node>();
        foreach(List<Vector2> list in path)
        {
            foreach (Vector2 pos in list)
            {
                nodes.Add(mapManager.GetNodeFromACoord(pos));
            }
        }
        while(movingPlayer.MovScore> 10 && nodeCounter< nodes.Count)
        {
            //animationStuff
            yield return new WaitForSeconds(0.5f);
            movingPlayer.Move(nodes[nodeCounter].Pos);
            mapManager.getCellFromNode(nodes[nodeCounter]).CheckTrap(movingPlayer.Team); //shouldApplyTraps            
            nodeCounter++;            
        }
    }

    #endregion
}

public class CharacterActionData
{
    string id;
    public string ID { get { return id; } }
    public List<List<Vector2>> mov;
    public List<string> prepSkills;
    public List<string> dashSkills;
    public List<string> acitonSkills;
    public CharacterActionData(string id)
    {
        mov = new List<List<Vector2>>();
        prepSkills = new List<string>();
        dashSkills = new List<string>();
        acitonSkills = new List<string>();
    }
}
