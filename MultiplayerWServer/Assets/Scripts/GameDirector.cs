using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum CharacterAction
{
    ability0,
    ability1,
    ability2,
    ability3,
    ultimate,
    cat0,
    cat1,
    cat2,
    follow,
    mov
}
/*public enum SkillType
{
    mov,
    prep,
    dash,
    action
}*/

public class GameDirector : MonoBehaviour
{
    [SerializeField] MapManager mapManager;
    MovementManager movManager;
    FogManager fogManager;
    AimSystem aim;
    Dictionary<string,GameObject> players;
    Server sv;
    [SerializeField] GameObject character0;
    [SerializeField] GameObject character1;
    [SerializeField] LineRenderer lineRenderer;
    List<CharacterActionData> actionsToReplicate;
    List<List<Vector2>> mov;
    Dictionary<int, Vector2> abilitiesCasted;
    List<CharacterAction> actionLog;
    Vector2 currentAim;
    Node currentNode;
    int currentMoveScore;
    Character followTarget;
    Character localCharacter;
    List<Character> allieds;
    List<Character> enemies;
    Ability selectedAbility;
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
        mov = new List<List<Vector2>>();
        abilitiesCasted = new Dictionary<int, Vector2>();
        aim = new AimSystem(mapManager);
        fogManager = new FogManager(mapManager);        
        actionLog = new List<CharacterAction>();
        Turn1();
    }

    private void GeneratePlayers()
    {
        int team;
        int gap;
        GameObject newPlayer;
        Vector2 spawnPos;
        allieds = new List<Character>();
        enemies = new List<Character>();
        players = new Dictionary<string, GameObject>();
        List<string> keys = sv.ConnectedPlayers.Keys.ToList();
        keys.Sort();
        foreach (string id in keys)
        {
            team = players.Count % 2;

            newPlayer = team == 0 ? Instantiate(character0) : Instantiate(character1);
            spawnPos = mapManager.GetSpawnBaseSpawnPoint(team);
            gap = (int)players.Count / 2;
            spawnPos.y -= gap < 2 ? -(gap % 2 + 1) : (gap % 2 + 1);
            newPlayer.GetComponent<Character>().Spawn(spawnPos, team);
            if (id == Server.id)
                localCharacter = newPlayer.GetComponent<Character>();
            turnStart += newPlayer.GetComponent<Character>().NewTurn;
            newPlayer.GetComponent<Character>().ID = id;
            players.Add(id, newPlayer);
            mapManager.AddPlayer(newPlayer.GetComponent<Character>(), spawnPos);
        }
        foreach (string id in keys)
        {
            Character character = players[id].GetComponent<Character>();
            character.LocalTeam = localCharacter.Team;
            if (character.Team == localCharacter.Team)
            {
                character.SetVisible(true);
                allieds.Add(character);
            }
            else
            {
                character.SetVisible(false);
                enemies.Add(character);
            }
        }

    }

    private void Turn1()
    {
        actionsToReplicate = new List<CharacterActionData>();        
        turn1 = true;
        fogManager.CalculateVision(allieds,true);
        fogManager.CalculateVision(enemies,false);
        followTarget = null;
        currentMoveScore = localCharacter.MovScore;
        currentNode = mapManager.GetNodeFromACoord(localCharacter.Pos);
        movManager.DrawTurn1Movement(localCharacter.Team,currentNode);        
    }

    public void NewTurn()
    {
        turnStart();
        actionsToReplicate = new List<CharacterActionData>();
        currentNode = mapManager.GetNodeFromACoord(localCharacter.Pos);
        currentMoveScore = localCharacter.MovScore;
        followTarget = null;
        abilitiesCasted = new Dictionary<int, Vector2>();
        fogManager.CalculateVision(allieds, true);
        fogManager.CalculateVision(enemies, false);
        movManager.DrawMovementWalkableRange(currentNode,currentMoveScore);
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

    public void MovCommand(Node node)
    {
        if (currentMoveScore >= 10&& node != currentNode)
        {
            if (actionLog.Contains(CharacterAction.follow))
            {
                DeleteSpecificAction(CharacterAction.follow);
            }

            if (movManager.WalkableNode(node))
            {
                WalkCommand(node);
            }
            else if (movManager.SprintableNode(node))
            {
                SprintCommand(node);
            }
            else
            {
                Node[] fullPath = movManager.GetPath(currentNode, node);                
                if (fullPath!=null)
                {
                    List<Vector2> CurrentMov = new List<Vector2>();
                    Node lastNode = currentNode;
                    CurrentMov.Add(currentNode.Pos);
                    foreach (Node pathNode in fullPath)
                    {
                        if (movManager.WalkableNode(pathNode))
                        {
                            CurrentMov.Add(pathNode.Pos);
                            lastNode = pathNode;
                        }
                    }                                
                    if (turn1)
                    {
                        mov = new List<List<Vector2>>();
                        if (actionLog.Count == 0 || actionLog[0] != CharacterAction.mov)
                            actionLog.Add(CharacterAction.mov);
                    }
                    else
                    {
                        actionLog.Add(CharacterAction.mov);
                        currentNode = lastNode;
                        currentMoveScore -= lastNode.g;
                        movManager.ResetFloorColor();
                        movManager.DrawMovementWalkableRange(currentNode, currentMoveScore);
                    }
                    mov.Add(CurrentMov);
                    DrawMov();
                }
            }
        }
    }



    public void SprintCommand(Node node)
    {
        actionLog.Add(CharacterAction.mov);
    }
    private void WalkCommand(Node node)
    {
        List<Vector2> CurrentMov = new List<Vector2>();
        if (turn1)
        {
            mov = new List<List<Vector2>>();
            Node[] nodes = movManager.GetPath(currentNode, node);
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
            Node[] nodes = movManager.GetCalculatedPath(currentNode, node);
            CurrentMov.Add(currentNode.Pos);
            for (int i = 0; i < nodes.Length; i++)
            {
                CurrentMov.Add(nodes[i].Pos);
            }
            mov.Add(CurrentMov);
            currentNode = node;
            currentMoveScore -= node.g;
            movManager.ResetFloorColor();
            movManager.DrawMovementWalkableRange(currentNode, currentMoveScore);
        }

        DrawMov();
    }

    public void CharacterClicked(GameObject characterModel)
    {
        Character clikedCharacter = characterModel.GetComponentInParent<Character>();
        if (!turn1)
        {               
            if (clikedCharacter != localCharacter)
            {
                if(clikedCharacter != followTarget)
                {
                    followTarget = clikedCharacter;
                    DrawFollow();
                    actionLog.Add(CharacterAction.follow);
                    movManager.ResetFloorColor();
                }
                else
                {                    
                    MovCommand(mapManager.GetNodeFromACoord(clikedCharacter.Pos));
                    followTarget = null;
                }
            }
            else
            {
                MovCommand(mapManager.GetNodeFromACoord(localCharacter.Pos));
            }
        }
        else
        {
            MovCommand(mapManager.GetNodeFromACoord(clikedCharacter.Pos));
        }
    }

    public void DrawMov()
    {
        lineRenderer.gameObject.SetActive(true);
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
    public void DrawFollow()
    {
        lineRenderer.gameObject.SetActive(true);
        Vector3[] followPos = new Vector3[2];
        followPos[0] = new Vector3(localCharacter.Pos.x, 0, localCharacter.Pos.y);
        followPos[1] = new Vector3(followTarget.Pos.x, 0, followTarget.Pos.y);
        lineRenderer.positionCount = 2;
        lineRenderer.SetPositions(followPos.ToArray());
    }
    public void ClearLineRenderer()
    {
        lineRenderer.gameObject.SetActive(false);
    }

    #endregion

    #region actions
    public bool SelectAbility(int abilitySlot)
    {
        if (abilitiesCasted.ContainsKey(abilitySlot))
        {
            DeleteSpecificAction((CharacterAction)abilitySlot);
            abilitiesCasted.Remove(abilitySlot);
            return false;
        }
        else
        {
            selectedAbility = localCharacter.GetAbility(abilitySlot);
            if (selectedAbility.aim == aimType.selfBuff)
            {
                actionLog.Add((CharacterAction)abilitySlot);
                abilitiesCasted.Add(abilitySlot, localCharacter.Pos);
                return false;
            }
            else
            {
                return true;
            }
        }        
    }
    public void Aiming(Vector2 aimingPos)//recive ability to know range and aiming type
    {
        currentAim = aimingPos;
        movManager.DisableMovementDraw();
        aim.PredictiveAim(localCharacter.Pos, aimingPos, 8, 0.5f ,aimType.linear,localCharacter.Team,40);
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
        if (actionLog.Contains(CharacterAction.follow))
        {
            actionData.followID = followTarget.ID;
        }
        else
        {
            actionData.SetMov(mov);
        }
        actionData.SetSkills(abilitiesCasted);
        sv.SubmitAction(actionData);
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
                    if (!turn1)
                    {
                        movManager.ResetFloorColor();
                        movManager.DrawMovementWalkableRange(currentNode, currentMoveScore);
                    }
                    break;
                case CharacterAction.follow:
                    followTarget = null;                    
                    movManager.ResetFloorColor();
                    movManager.DrawMovementWalkableRange(currentNode, currentMoveScore);     
                    DrawMov();                    
                    break;
            }
            actionLog.RemoveAt(actionLog.Count - 1);
        }        
    }
    public void DeleteSpecificAction(CharacterAction action)
    {
        List<CharacterAction> newLog = new List<CharacterAction>();

        for (int i = 0; i < actionLog.Count; i++)
        {
            if(actionLog[i]!= action)
            {
                newLog.Add(actionLog[i]);
            }
            else
            {
                switch (actionLog[i])
                {
                    case CharacterAction.follow:
                        followTarget = null;                        
                        movManager.ResetFloorColor();
                        movManager.DrawMovementWalkableRange(currentNode, currentMoveScore);
                        DrawMov();                        
                        break;
                }
            }
        }
        actionLog = newLog;
    }


    #endregion

    #region Replication

    public void ReceiveActionToReplicate(CharacterActionData data)
    {
        actionsToReplicate.Add(data);
    }

    public void StartReplication()
    {
        StartCoroutine(ReplicateSkills());
        
    }
    
    IEnumerator ReplicateSkills()
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
        List<Character> followingTargets = new List<Character>();
        List<Cell> currentCellsToMove;
        int charactersMoving = 0;
        int movCounter = 0;
        mapManager.ResetPlayersPos();
        foreach (CharacterActionData data in actionsToReplicate)
        {
            if(data.mov.Count>0)
            {
                charactersMoving++;
                movingCharacters.Add(players[data.id].GetComponent<Character>());
                movements.Add(data.GetMov());
            }
            else
            {
                if(data.followID != "")
                {
                    followingCharacters.Add(players[data.id].GetComponent<Character>());
                    followingTargets.Add(players[data.followID].GetComponent<Character>());
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
            for (int i = 0; i < movingCharacters.Count; i++) // move all characters
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
            for (int i = 0; i < movingCharacters.Count; i++) // check conflicting characters & characters in their last move
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
            for (int i = 0; i < movingCharacters.Count; i++) // resolve position conflicts
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
                            neighborsNodes = movManager.GetAllEmptyNodesUnderAScore(mapManager.GetNodeFromACoord(movingCharacters[i].Pos), scoreRange);
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
                                movingCharacters[i].Move(lastNode.Pos);
                            }
                            else
                            {
                                movingCharacters[i].Move(lastNode.Pos);
                            }
                        }
                        movingCharacters[i].AlreadyMove = true;
                        mapManager.ActualziatePlayerPos(movingCharacters[i], movingCharacters[i].Pos);
                        charactersMoving--;
                    }    
                }
            }
            for (int i = 0; i < movingCharacters.Count; i++)
            {
                Cell currentCell = mapManager.GetCellFromCoord(movingCharacters[i].Pos);
                if (movingCharacters[i].CellOwner)
                {
                    //movingCharacters[i].ApplyStatus(currentCell.CheckBuffs(movingCharacters[i].Team));
                    
                }
                //then activate traps
                
            }
            movCounter++;
            fogManager.CalculateVision(allieds,true);
            fogManager.CalculateVision(enemies, false);
            if(localCharacter.Team == 0)
            {
                foreach(Character enemy in enemies)
                {
                    enemy.SetVisible(mapManager.GetCellFromCoord(enemy.Pos).Visible_0);
                }
                foreach (Character allied in allieds)
                {
                    allied.SetVisible(mapManager.GetCellFromCoord(allied.Pos).Visible_1);
                }
            }
            else
            {
                foreach (Character enemy in enemies)
                {
                    enemy.SetVisible(mapManager.GetCellFromCoord(enemy.Pos).Visible_1);
                }
                foreach (Character allied in allieds)
                {
                    allied.SetVisible(mapManager.GetCellFromCoord(allied.Pos).Visible_0);
                }
            }
            GC.Collect();
            yield return new WaitForSeconds(0.5f);
        } //Normal move


        movements = new List<List<Vector2>>();
        for (int i = 0; i < followingCharacters.Count; i++)
        {
            List<Vector2> movList = new List<Vector2>();
            Node targetNode;
            if (followingTargets[i].Team == followingCharacters[i].Team)
            {
                targetNode = mapManager.GetNodeFromACoord(followingTargets[i].Pos);
            }
            else
            {
                targetNode = mapManager.GetNodeFromACoord(followingTargets[i].LastPosSeen);
            }
            Node[] newPath = movManager.GetPath(mapManager.GetNodeFromACoord(followingCharacters[i].Pos), targetNode);
            for (int j = 0; j < newPath.Length; j++)
            {
                movList.Add(newPath[j].Pos);
            }
            movements.Add(movList);
            charactersMoving++;                

        }
        movCounter = 0;
        while (charactersMoving > 0)
        {
            currentCellsToMove = new List<Cell>();
            for (int i = 0; i < followingCharacters.Count; i++) // move all characters
            {
                if (!followingCharacters[i].AlreadyMove)
                {
                    if (followingCharacters[i].MovScore >= 10 && movCounter < movements[i].Count && ((mapManager.GetCharacterByPos(movements[i][movCounter], false) == null || movCounter + 1 < movements[i].Count)))
                    {
                        followingCharacters[i].Move(movements[i][movCounter]);
                        if (!currentCellsToMove.Contains(mapManager.GetCellFromCoord(movements[i][movCounter])))
                        {
                            currentCellsToMove.Add(mapManager.GetCellFromCoord(movements[i][movCounter]));
                        }
                        else
                        {
                            MovementConflict(followingCharacters[i], followingCharacters);
                        }
                    }
                    else
                    {
                        followingCharacters[i].CellOwner = false;
                    }
                }
            }
            fogManager.CalculateVision(allieds, true);
            fogManager.CalculateVision(enemies, false);
            if (localCharacter.Team == 0)
            {
                foreach (Character enemy in enemies)
                {
                    enemy.SetVisible(mapManager.GetCellFromCoord(enemy.Pos).Visible_0);
                }
                foreach (Character allied in allieds)
                {
                    allied.SetVisible(mapManager.GetCellFromCoord(allied.Pos).Visible_1);
                }
            }
            else
            {
                foreach (Character enemy in enemies)
                {
                    enemy.SetVisible(mapManager.GetCellFromCoord(enemy.Pos).Visible_1);
                }
                foreach (Character allied in allieds)
                {
                    allied.SetVisible(mapManager.GetCellFromCoord(allied.Pos).Visible_0);
                }
            }
            for (int i = 0; i < followingCharacters.Count; i++)
            {
                if (!followingCharacters[i].AlreadyMove)
                {

                    if (movements[i][movements[i].Count - 1] != followingTargets[i].Pos)
                    {
                        List<Vector2> movList = new List<Vector2>();
                        Node targetNode;
                        if (followingTargets[i].Team == followingCharacters[i].Team)
                        {
                            targetNode = mapManager.GetNodeFromACoord(followingTargets[i].Pos);
                        }
                        else
                        {
                            targetNode = mapManager.GetNodeFromACoord(followingTargets[i].LastPosSeen);
                        }

                        Node[] newPath = movManager.GetPath(mapManager.GetNodeFromACoord(followingCharacters[i].Pos), targetNode);
                        for (int j = 0; j < movCounter+1; j++)
                        {
                            movList.Add(followingTargets[i].Pos);
                        }
                        for (int j = 0; j < newPath.Length; j++)
                        {
                            movList.Add(newPath[i].Pos);
                        }
                        movements[i] = movList;

                    }
                }
            }

            for (int i = 0; i < followingCharacters.Count; i++) // check conflicting characters & characters in their last move
            {
                if (!followingCharacters[i].AlreadyMove)
                {
                    if (!followingCharacters[i].CellOwner)
                    {
                        MovementConflict(followingCharacters[i], followingCharacters);
                    }
                    if ( followingCharacters[i].MovScore < 10 || movements[i].Count <= movCounter + 1)
                    {
                        if (followingCharacters[i].CellOwner )
                        {
                            followingCharacters[i].AlreadyMove = true;
                            mapManager.ActualziatePlayerPos(followingCharacters[i], followingCharacters[i].Pos);
                            charactersMoving--;
                        }
                    }
                }
            }
            for (int i = 0; i < followingCharacters.Count; i++) // resolve position conflicts
            {
                if (!followingCharacters[i].AlreadyMove)
                {
                    if (!followingCharacters[i].CellOwner && movements[i].Count <= movCounter)
                    {
                        int scoreRange = 15;
                        Node lastNode = mapManager.GetNodeFromACoord(followingCharacters[i].LastPos);
                        Node nextNode = mapManager.GetNodeFromACoord(followingCharacters[i].Pos + (followingCharacters[i].Pos - followingCharacters[i].LastPos));
                        List<Node> neighborsNodes = new List<Node>();
                        while (neighborsNodes.Count == 0)
                        {
                            neighborsNodes = movManager.GetAllEmptyNodesUnderAScore(mapManager.GetNodeFromACoord(followingCharacters[i].Pos), scoreRange);
                            scoreRange += 5;
                        }
                        if (neighborsNodes.Contains(lastNode))
                        {
                            followingCharacters[i].Move(lastNode.Pos);
                        }
                        else
                        {
                            if (neighborsNodes.Contains(nextNode))
                            {
                                followingCharacters[i].Move(lastNode.Pos);
                            }
                            else
                            {
                                followingCharacters[i].Move(lastNode.Pos);
                            }
                        }
                        followingCharacters[i].AlreadyMove = true;
                        mapManager.ActualziatePlayerPos(followingCharacters[i], followingCharacters[i].Pos);
                        charactersMoving--;
                    }
                }
            }
            for (int i = 0; i < followingCharacters.Count; i++)
            {
                Cell currentCell = mapManager.GetCellFromCoord(followingCharacters[i].Pos);
                if (followingCharacters[i].CellOwner)
                {
                    //movingCharacters[i].ApplyStatus(currentCell.CheckBuffs(movingCharacters[i].Team));

                }
                //then activate traps

            }
            movCounter++;
            fogManager.CalculateVision(allieds,true);
            fogManager.CalculateVision(enemies,false);
            if (localCharacter.Team == 0)
            {
                foreach (Character enemy in enemies)
                {
                    enemy.SetVisible(mapManager.GetCellFromCoord(enemy.Pos).Visible_0);
                }
                foreach (Character allied in allieds)
                {
                    allied.SetVisible(mapManager.GetCellFromCoord(allied.Pos).Visible_1);
                }
            }
            else
            {
                foreach (Character enemy in enemies)
                {
                    enemy.SetVisible(mapManager.GetCellFromCoord(enemy.Pos).Visible_1);
                }
                foreach (Character allied in allieds)
                {
                    allied.SetVisible(mapManager.GetCellFromCoord(allied.Pos).Visible_0);
                }
            }           

            GC.Collect();
            yield return new WaitForSeconds(0.5f);
        }// Follow

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


