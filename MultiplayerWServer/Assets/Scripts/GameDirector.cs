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
public enum SkillType
{
    mov,
    prep,
    dash,
    action
}

public class GameDirector : MonoBehaviour
{
    [SerializeField] MapManager mapManager;
    MovementManager movManager;
    AimSystem aim;
    Dictionary<string,GameObject> players;
    Server sv;
    [SerializeField] GameObject character0;
    [SerializeField] GameObject character1;
    [SerializeField] LineRenderer lineRenderer;
    List<CharacterActionData> actionsToReplicate;
    List<List<Vector2>> mov;
    List<CharacterAction> actionLog;
    Vector2 currentAim;
    Node currentNode;
    int currentMoveScore;
    Character localCharacter;
    bool turn1 = false;
    public delegate void DirectorDelegate();
    static public DirectorDelegate turnStart;


    #region Local

    private void Start()
    {
        sv = Server.Instance;
        sv.SetGameDirector(this);
        movManager = GetComponent<MovementManager>();
        GeneratePlayers();
        PositionCamera();
        Turn1();
        mov = new List<List<Vector2>>();
        aim = new AimSystem(mapManager);
        actionLog = new List<CharacterAction>();
    }

    private void Turn1()
    {
        actionsToReplicate = new List<CharacterActionData>();
        turn1 = true;
        currentNode = mapManager.GetNodeFromACoord(localCharacter.Pos);
        movManager.DrawTurn1Movement(localCharacter.Team,currentNode);        
    }

    public void NewTurn()
    {
        turnStart();
        actionsToReplicate = new List<CharacterActionData>();
        currentNode = mapManager.GetNodeFromACoord(localCharacter.Pos);
        currentMoveScore = localCharacter.MovScore;
        movManager.DrawMovementWalkableRange(currentNode,currentMoveScore,localCharacter.Team,true);
        DrawMov();
        InputManager.inputEneable = true;
    }
    #region movement
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
            Node[] nodes = movManager.GetPath(currentNode, cell,localCharacter.Team,true);
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
            movManager.DrawMovementWalkableRange(currentNode, currentMoveScore,localCharacter.Team,true);
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

    #endregion

    #region actions
    public void Aiming(Vector2 aimingPos)//recive ability to know range and aiming type
    {
        currentAim = aimingPos;
        movManager.DisableMovementDraw();
        aim.PredictiveAim(localCharacter.Pos, aimingPos, 8, aimType.linear,localCharacter.Team,40);
    }
    public void ConfirmAim()
    {
        aim.RestoreFloorColors();
        movManager.ReDrawMov(false);
    }
    public void CancelAim()
    {
        aim.RestoreFloorColors();
        movManager.ReDrawMov(true);
    }

    #endregion

    public void ReadyToEndTurn()
    {
        sv.ReadyToEndTurn();
        InputManager.inputEneable = false;
    }

    public void EndOfTurn()
    {
        Debug.Log("EndTurn");
        if (turn1)
            turn1 = false;

        movManager.ResetFloorColor();
        Submit();
        actionLog = new List<CharacterAction>();
        mov = new List<List<Vector2>>();
        
    }

    private void Submit()
    {
        CharacterActionData actionData = new CharacterActionData();
        actionData.id = Server.id;
        actionData.SetMov(mov);
        sv.SubmitAction(actionData);
    }

    public void DeleteLastAction()
    {
        if (actionLog.Count > 0)
        {
            switch (actionLog[actionLog.Count - 1])
            {
                case CharacterAction.mov:
                    movManager.GetPath(mapManager.GetNodeFromACoord(mov[mov.Count - 1][0]), currentNode,localCharacter.Team,true);
                    currentMoveScore = currentMoveScore + currentNode.g;
                    currentNode = mapManager.GetNodeFromACoord(mov[mov.Count - 1][0]);
                    mov.RemoveAt(mov.Count - 1);
                    DrawMov();
                    if (!turn1)
                    {
                        movManager.ResetFloorColor();
                        movManager.DrawMovementWalkableRange(currentNode, currentMoveScore,localCharacter.Team,true);
                    }
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
            spawnPos.y -= gap < 2 ? -(gap % 2 + 1) : (gap % 2 + 1);
            newPlayer.GetComponent<Character>().Spawn(spawnPos, team);
            if (id == Server.id)
                localCharacter = newPlayer.GetComponent<Character>();
            turnStart += newPlayer.GetComponent<Character>().NewTurn;
            players.Add(id,newPlayer);
            mapManager.AddPlayer(newPlayer.GetComponent<Character>(), spawnPos);
        }
    }
    #endregion

    #region Replication

    public void ReceiveActionToReplicate(CharacterActionData data)
    {
        actionsToReplicate.Add(data);
    }

    public void StartReplication()
    {
        StartCoroutine(ReplicateActions());
        
    }
    
    IEnumerator ReplicateActions()
    {
        foreach (CharacterActionData data in actionsToReplicate)
        {



        }        
        yield return StartCoroutine(ReplicatedMoves()); 
    }

    IEnumerator ReplicatedMoves()
    {
        List<Character> movingCharacters = new List<Character>();
        List<List<Vector2>> movements = new List<List<Vector2>>();
        List<Character> followingCharacters = new List<Character>();
        List<string> followingTargetsID = new List<string>();
        List<Cell> currentCellsToMove;
        int charactersMoving = 0;
        int movCounter = 0;
        mapManager.ResetPlayersPos();
        foreach (CharacterActionData data in actionsToReplicate)
        {
            if(data.mov != null)
            {
                charactersMoving++;
                movingCharacters.Add(players[data.id].GetComponent<Character>());
                movements.Add(data.GetMov());
            }
            else
            {
                if(data.followID!= null)
                {
                    followingCharacters.Add(players[data.id].GetComponent<Character>());
                    followingTargetsID.Add(data.followID);
                }
                else
                {
                    mapManager.ActualziatePlayerPos(players[data.id].GetComponent<Character>(), players[data.id].GetComponent<Character>().Pos); 
                }
            }
        }
        while (charactersMoving > 0)
        {
            currentCellsToMove = new List<Cell>();            
            for (int i = 0; i < movingCharacters.Count; i++)
            {
                if (!movingCharacters[i].AlreadyMove)
                {
                    if (movingCharacters[i].MovScore >= 10 && movCounter < movements[i].Count && ((mapManager.GetCharacterByPos(movements[i][movCounter], false)==null || movCounter + 1 < movements[i].Count)))
                    {                       
                        movingCharacters[i].Move(movements[i][movCounter]);
                        if (!currentCellsToMove.Contains(mapManager.GetCellFromCoord(movements[i][movCounter])))
                        {
                            currentCellsToMove.Add(mapManager.GetCellFromCoord(movements[i][movCounter]));
                        }
                        else
                        {
                            MovementConflict(movingCharacters[i], movingCharacters);
                        }
                    }
                    else
                    {
                        movingCharacters[i].CellOwner = false;
                    }
                }                 
            }
            for (int i = 0; i < movingCharacters.Count; i++)
            {
                if (!movingCharacters[i].AlreadyMove)
                {
                    if (!movingCharacters[i].CellOwner)
                    {
                        MovementConflict(movingCharacters[i], movingCharacters);
                    }
                    if (movements[i].Count <= movCounter + 1 || movingCharacters[i].MovScore < 10)
                    {
                        if (movingCharacters[i].CellOwner)
                        {
                            movingCharacters[i].AlreadyMove = true;
                            mapManager.ActualziatePlayerPos(movingCharacters[i], movingCharacters[i].Pos);
                            charactersMoving--;
                        }
                    }
                }                    
            }
            for (int i = 0; i < movingCharacters.Count; i++)
            {
                if (!movingCharacters[i].AlreadyMove)
                {                    
                    if (!movingCharacters[i].CellOwner && movements[i].Count <= movCounter)
                    {
                        int scoreRange = 15;
                        Node lastNode = mapManager.GetNodeFromACoord(movingCharacters[i].LastPos);
                        Node nextNode = mapManager.GetNodeFromACoord(movingCharacters[i].Pos + (movingCharacters[i].Pos - movingCharacters[i].LastPos));
                        List<Node> neighborsNodes = new List<Node>();
                        while (neighborsNodes.Count == 0)
                        {
                            neighborsNodes = movManager.GetAllNodesUnderAScore(mapManager.GetNodeFromACoord(movingCharacters[i].Pos), scoreRange,-1,false);
                            scoreRange += 5;
                        }
                        if (neighborsNodes.Contains(lastNode))
                        {
                            movingCharacters[i].Move(lastNode.Pos);
                        }
                        else
                        {
                            if (neighborsNodes.Contains(nextNode))
                            {
                                movingCharacters[i].Move(nextNode.Pos);
                            }
                            else
                            {
                                movingCharacters[i].Move(neighborsNodes[0].Pos);
                            }
                        }
                        movingCharacters[i].AlreadyMove = true;
                        mapManager.ActualziatePlayerPos(movingCharacters[i], movingCharacters[i].Pos);
                        charactersMoving--;
                    }    
                }
            }
            /*string Error, etrategicamente colocado para encontrar rapido donde quede. Falta apligar buffs y trampas, y los follows*/
            movCounter++;
            GC.Collect();
            yield return new WaitForSeconds(0.5f);
        }         
        sv.ReplicationEnded();
    }

    private void MovementConflict(Character currentCharacter, List<Character> movingCharacters)
    {
        List<Character> conflictingCharacters = new List<Character>();
        List<Character> tiedCharacters = new List<Character>();        
        int lowestMovSpended = currentCharacter.movSpended;
        foreach(Character character in movingCharacters)
        {
            if (character.Pos == currentCharacter.Pos) 
                conflictingCharacters.Add(character);
        }
        foreach(Character character in conflictingCharacters)
        {
            character.CellOwner = false;
            if(character.movSpended< lowestMovSpended)
            {
                tiedCharacters = new List<Character>();
                tiedCharacters.Add(character);
                lowestMovSpended = character.movSpended;
            }
            else
            {
                if (character.movSpended == lowestMovSpended)
                    tiedCharacters.Add(character);
            }
        }
        if(tiedCharacters.Count == 1)
        {
            tiedCharacters[0].CellOwner = true;
        }
    }

    #endregion
}


