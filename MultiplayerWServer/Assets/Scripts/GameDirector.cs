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
    mov,
    sprint
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
    [SerializeField] ActionBarDisplay actionBar;
    MovementManager movManager;
    TrapManager trapManager;
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
    int actionPoints;
    Character followTarget;
    Character localCharacter;
    List<Character> allieds;
    List<Character> enemies;
    Ability selectedAbility;
    int selectedAbilitySlot;
    bool turn1 = false;
    public delegate void DirectorDelegate();
    static public DirectorDelegate turnStart;
    bool ready = false;
    bool submited = false;

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
        trapManager = new TrapManager();
        turnStart += trapManager.NewTurn;
        aim = new AimSystem(mapManager,trapManager);
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
        SetActionBar();

    }

    private void SetActionBar()
    {
        for (int i = 0; i < 8; i++)
        {
            Ability ability = localCharacter.GetAbility(i);            
            actionBar.SetSprite(i, ability.icon);
            actionBar.SetCooldown(i, localCharacter.GetCurrentCooldown(i));
            if (localCharacter.TryToGetAbility(i, actionPoints) == null)
                actionBar.DisableOnPos(i);
        }
    }
    private void ActualziateActionBar()
    {
        for (int i = 0; i < 8; i++)
        {
            actionBar.SetCooldown(i, localCharacter.GetCurrentCooldown(i));
            if (localCharacter.TryToGetAbility(i, actionPoints) == null)
                actionBar.DisableOnPos(i);
        }
    }

    private void Turn1()
    {
        actionBar.DisableAll();
        actionsToReplicate = new List<CharacterActionData>();        
        turn1 = true;
        fogManager.CalculateVision(allieds,true);
        fogManager.CalculateVision(enemies,false);
        followTarget = null;
        actionPoints = 1;
        currentMoveScore = localCharacter.MovScore;
        currentNode = mapManager.GetNodeFromACoord(localCharacter.Pos);
        movManager.DrawTurn1Movement(localCharacter.Team,currentNode);        
    }

    public void NewTurn()
    {
        turnStart();
        CheckBuffSpawneds();
        fogManager.CalculateVision(allieds, true);
        fogManager.CalculateVision(enemies, false);        
        actionsToReplicate = new List<CharacterActionData>();
        followTarget = null;
        abilitiesCasted = new Dictionary<int, Vector2>();
        ready = false;
        submited = false;
        if (localCharacter.Alive)
        {
            actionPoints = 3;
            actionBar.EneableAll();
            ActualziateActionBar();
            currentNode = mapManager.GetNodeFromACoord(localCharacter.Pos);            
            currentMoveScore = localCharacter.MovScore;
            movManager.DrawMovement(currentNode, currentMoveScore, actionPoints, localCharacter.SprintScore);
            DrawMov();
            InputManager.inputEneable = true;            
        }
        else
        {
            actionBar.DisableAll();
            ReadyToEndTurn();
        }
    }

    private void CheckBuffSpawneds( )
    {
        Cell currentCell;
        for (int i = 0; i < allieds.Count; i++)
        {
            currentCell = mapManager.GetCellFromCoord(allieds[i].Pos);
            PickeableBuff cellBuff = currentCell.PickBuff(allieds[i].Team);
            if (cellBuff != PickeableBuff.None)
            {
                Status statusToApply = new Status();
                switch (cellBuff)
                {
                    case PickeableBuff.Energize:
                        statusToApply.type = StatusType.energized;
                        statusToApply.duration = 2;
                        break;
                    case PickeableBuff.Heal:
                        statusToApply.type = StatusType.healing;
                        statusToApply.duration = 2;
                        break;
                    case PickeableBuff.Haste:
                        statusToApply.type = StatusType.hasted;
                        statusToApply.duration = 2;
                        break;
                    case PickeableBuff.semiHeal:
                        statusToApply.type = StatusType.healing;
                        statusToApply.duration = 0;
                        break;
                    case PickeableBuff.Might:
                        statusToApply.type = StatusType.might;
                        statusToApply.duration = 2;
                        break;
                }
                allieds[i].ApplyStatus(statusToApply);
            }
        }
        for (int i = 0; i < enemies.Count; i++)
        {
            currentCell = mapManager.GetCellFromCoord(enemies[i].Pos);
            PickeableBuff cellBuff = currentCell.PickBuff(enemies[i].Team);
            if (cellBuff != PickeableBuff.None)
            {
                Status statusToApply = new Status();
                switch (cellBuff)
                {
                    case PickeableBuff.Energize:
                        statusToApply.type = StatusType.energized;
                        statusToApply.duration = 2;
                        break;
                    case PickeableBuff.Heal:
                        statusToApply.type = StatusType.healing;
                        statusToApply.duration = 2;
                        break;
                    case PickeableBuff.Haste:
                        statusToApply.type = StatusType.hasted;
                        statusToApply.duration = 2;
                        break;
                    case PickeableBuff.semiHeal:
                        statusToApply.type = StatusType.healing;
                        statusToApply.duration = 0;
                        break;
                    case PickeableBuff.Might:
                        statusToApply.type = StatusType.might;
                        statusToApply.duration = 2;
                        break;
                }
                enemies[i].ApplyStatus(statusToApply);
            }
        }

    }
    private void PositionCamera()
    {
        if (localCharacter.Team == 0)
        {
            Camera.main.transform.RotateAround(new Vector3(9, 0, 9), new Vector3(0, 1, 0), 180);
        }
    }

    #region movement

    public void MovCommand(Node node)
    {
        if (actionPoints>0 &&(currentMoveScore >= 10|| actionPoints>=2) && node != currentNode)
        {
            if (actionLog.Contains(CharacterAction.follow))
            {
                DeleteSpecificAction(CharacterAction.follow);
            }

            if (movManager.WalkableNode(node))
            {
                WalkCommand(node);
            }
            else if (movManager.SprintableNode(node) && actionPoints >= 2)
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
                        if (actionPoints >= 2)
                        {
                            if (movManager.SprintableNode(pathNode))
                            {
                                CurrentMov.Add(pathNode.Pos);
                                lastNode = pathNode;
                            }
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
                        if (actionPoints >= 2)
                        {
                            ModifiActionPoints(-2);
                            actionLog.Add(CharacterAction.sprint);
                            currentMoveScore += localCharacter.SprintScore;
                        }
                        else
                        {
                            actionLog.Add(CharacterAction.mov);
                        }
                        currentNode = lastNode;
                        currentMoveScore -= lastNode.g;
                        movManager.ResetFloorColor();
                        movManager.DrawMovement(currentNode, currentMoveScore,actionPoints, localCharacter.SprintScore);
                    }
                    mov.Add(CurrentMov);
                    DrawMov();
                }
            }
        }
    }



    public void SprintCommand(Node node)
    {
        ModifiActionPoints(-2);
        List<Vector2> CurrentMov = new List<Vector2>();                
        actionLog.Add(CharacterAction.sprint);
        Node[] nodes = movManager.GetCalculatedPath(currentNode, node);
        CurrentMov.Add(currentNode.Pos);
        for (int i = 0; i < nodes.Length; i++)
        {
            CurrentMov.Add(nodes[i].Pos);
        }
        mov.Add(CurrentMov);
        currentNode = node;
        currentMoveScore -= node.g;
        currentMoveScore += localCharacter.SprintScore;
        movManager.ResetFloorColor();
        movManager.DrawMovement(currentNode, currentMoveScore, actionPoints, localCharacter.SprintScore);


        DrawMov();
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
            movManager.DrawMovement(currentNode, currentMoveScore, actionPoints, localCharacter.SprintScore);
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
        if (turn1)
            return false;
        
        Ability givenAbility = localCharacter.GetAbility(abilitySlot);

        if (abilitiesCasted.ContainsKey(abilitySlot))
        {
            CancelAbility(abilitySlot, true);
            return false;
        }
        else
        {
            givenAbility = localCharacter.TryToGetAbility(abilitySlot, actionPoints);
            if (givenAbility == null)
            {
                return false;
            }
            actionBar.Select(abilitySlot);
            selectedAbility = givenAbility;
            selectedAbilitySlot = abilitySlot;
            if (selectedAbility.aim == AimType.selfBuff)
            {
                actionLog.Add((CharacterAction)abilitySlot);
                ModifiActionPoints(-selectedAbility.cost);
                abilitiesCasted.Add(abilitySlot, localCharacter.Pos);
                return false;
            }
            else
            {
                return true;
            }
        }
        
    }
    void CancelAbility(int abilitySlot, bool delete)
    {
        if(delete)
            DeleteSpecificAction((CharacterAction)abilitySlot);

        Debug.Log("CancelAbility");
        actionBar.Unselect(abilitySlot);
        ModifiActionPoints(localCharacter.GetAbility(abilitySlot).cost);
        movManager.DrawMovement(currentNode, currentMoveScore, actionPoints, localCharacter.SprintScore);
        abilitiesCasted.Remove(abilitySlot);
    }
    public void ModifiActionPoints(int ammount)
    {
        actionPoints += ammount;
        ActualziateActionBar();
    }
    public void Aiming(Vector2 aimingPos)
    {
        currentAim = aimingPos;
        movManager.DisableMovementDraw();
        aim.PredictiveAim(localCharacter.Pos, aimingPos,localCharacter,selectedAbility);
    }
    public void ConfirmAim()
    {
        if(selectedAbility.aim == AimType.cell)
        {
            if(aim.PredictiveAim(localCharacter.Pos, currentAim, localCharacter, selectedAbility)[0][0].impactPos == new Vector2(-1, -1))
            {
                CancelAim();
            }
            else
            {
                actionLog.Add((CharacterAction)selectedAbilitySlot);
                ModifiActionPoints( -selectedAbility.cost);
                aim.RestoreFloorColors();
                abilitiesCasted.Add(selectedAbilitySlot, currentAim);
                movManager.DrawMovement(currentNode, currentMoveScore, actionPoints, localCharacter.SprintScore);
            }
        }
        else
        {

            actionLog.Add((CharacterAction)selectedAbilitySlot);
            ModifiActionPoints(-selectedAbility.cost);
            aim.RestoreFloorColors();
            abilitiesCasted.Add(selectedAbilitySlot, currentAim);
            movManager.DrawMovement(currentNode, currentMoveScore, actionPoints, localCharacter.SprintScore);
        }        
    }
    public void CancelAim()
    {
        actionBar.Unselect(selectedAbilitySlot);
        aim.RestoreFloorColors();
        movManager.DrawMovement(currentNode, currentMoveScore, actionPoints, localCharacter.SprintScore);
    }

    #endregion

    public void ReadyToEndTurn()
    {
        if (!submited)
        {
            if (ready)
            {
                sv.CancelReaedyToEndTurn();
                InputManager.inputEneable = true;
            }
            else
            {
                sv.ReadyToEndTurn();
                InputManager.inputEneable = false;
            }
        }            
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

        actionBar.DisableAll();
        submited = true;
        CharacterActionData actionData = new CharacterActionData();
        actionData.id = Server.id;
        if (actionPoints > 0)
        {
            if (actionLog.Contains(CharacterAction.follow))
            {
                actionData.followID = followTarget.ID;
            }
            else
            {
                actionData.SetMov(mov);
            }
        }       
        if (actionPoints >= 2|| actionLog.Contains(CharacterAction.sprint))
            actionData.sprinting = true;
        actionData.SetSkills(abilitiesCasted);
        sv.SubmitAction(actionData);
    }

    public void DeleteLastAction()
    {
        Debug.Log("Delete last");
        if (actionLog.Count > 0)
        {
            switch (actionLog[actionLog.Count - 1])
            {
                case CharacterAction.sprint:
                case CharacterAction.mov:
                    if(actionLog[actionLog.Count - 1] == CharacterAction.sprint)
                    {
                        ModifiActionPoints( 2);
                        currentMoveScore -= localCharacter.SprintScore;
                    }
                    movManager.GetPath(mapManager.GetNodeFromACoord(mov[mov.Count - 1][0]), currentNode);
                    currentMoveScore = currentMoveScore + currentNode.g;
                    currentNode = mapManager.GetNodeFromACoord(mov[mov.Count - 1][0]);
                    mov.RemoveAt(mov.Count - 1);
                    DrawMov();
                    if (!turn1)
                    {
                        movManager.ResetFloorColor();
                        movManager.DrawMovement(currentNode, currentMoveScore, actionPoints, localCharacter.SprintScore);
                    }
                    break;
                case CharacterAction.follow:
                    followTarget = null;                    
                    movManager.ResetFloorColor();
                    movManager.DrawMovement(currentNode, currentMoveScore, actionPoints, localCharacter.SprintScore);
                    DrawMov();                    
                    break;
                case CharacterAction.ability0:
                case CharacterAction.ability1:
                case CharacterAction.ability2:
                case CharacterAction.ability3:
                case CharacterAction.ultimate:
                case CharacterAction.cat0:
                case CharacterAction.cat1:
                case CharacterAction.cat2:
                    CancelAbility((int)actionLog[actionLog.Count - 1],false);
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
                        movManager.DrawMovement(currentNode, currentMoveScore, actionPoints, localCharacter.SprintScore);
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
        StartCoroutine(ReplicateAbilities());
        
    }
    
    IEnumerator ReplicateAbilities()
    {
        List<AbilityCastedInfo> prepAbilities = new List<AbilityCastedInfo>();
        List<AbilityCastedInfo> dashAbilities = new List<AbilityCastedInfo>();       
        List<AbilityCastedInfo> fireAbilities = new List<AbilityCastedInfo>();
        Dictionary<int, Vector2> currentSkills;

        foreach (CharacterActionData data in actionsToReplicate)
        {
            Character character = players[data.id].GetComponent<Character>();            
            if (character.Alive)
            {
                currentSkills = data.GetSkills();
                foreach (int abilityKey in currentSkills.Keys)
                {
                    AbilityCastedInfo info = new AbilityCastedInfo();
                    info.ability = character.GetAbility(abilityKey);                    
                    info.caster = character;
                    info.target = currentSkills[abilityKey];
                    switch (info.ability.type)
                    {
                        case AbilityType.preparation:
                            prepAbilities.Add(info);
                            break;
                        case AbilityType.dash:
                            dashAbilities.Add(info);
                            break;
                        case AbilityType.fire:
                            fireAbilities.Add(info);
                            break;
                    }
                }

            }
        }
        foreach(AbilityCastedInfo abilityCasted in prepAbilities) // prep
        {
            Ability currentAbility = abilityCasted.ability;
            Vector2 target = abilityCasted.target;
            Character caster = abilityCasted.caster;
            Debug.Log(caster.name + " Cast:" + currentAbility.abilityName + "!!!");
            if (currentAbility.aim == AimType.selfBuff)
            {
                caster.AddEnergy(currentAbility.energyProduced);
                foreach(Status status in currentAbility.statusToApply)
                {
                    caster.ApplyStatus(new Status(status.type,status.duration));
                }
            }
            else
            {
                List<HitInfo>[] hits = aim.CheckImpact(caster.Pos, target, caster, currentAbility);
                ReplicateImpacts(hits, currentAbility,caster);
            }
            //play animations&stuff
            caster.SetOnCooldown(currentAbility);      
            yield return new WaitForSeconds(0.5f);
        }
        foreach (AbilityCastedInfo abilityCasted in dashAbilities) // dash
        {
            Ability currentAbility = abilityCasted.ability;
            Vector2 target = abilityCasted.target;
            Character caster = abilityCasted.caster;
            List<HitInfo>[] hits = aim.CheckImpact(caster.Pos, target, caster, currentAbility);
            Debug.Log(caster.name + " Cast:" + currentAbility.abilityName + "!!!");
            Cell targetCell = mapManager.GetCellFromCoord(hits[0][0].impactPos);
            if(mapManager.GetCharacterByPos(targetCell.Pos,false) == null)
            {
                caster.Move(targetCell.Pos);
                mapManager.ActualziatePlayerPos(caster, targetCell.Pos);
            }
            else
            {
                int scoreRange = 15;
                List<Node> neighborsNodes = new List<Node>();
                while (neighborsNodes.Count == 0)
                {
                    neighborsNodes = movManager.GetAllEmptyNodesUnderAScore(mapManager.GetNodeFromACoord(caster.Pos), scoreRange);
                    scoreRange += 5;
                }
                caster.Move(neighborsNodes[0].Pos);
                mapManager.ActualziatePlayerPos(caster, neighborsNodes[0].Pos);
            }
            ReplicateImpacts(hits, currentAbility,caster);
            caster.SetOnCooldown(currentAbility);
            //play animations&stuff
        }
        foreach (AbilityCastedInfo abilityCasted in fireAbilities) // fire
        {
            Ability currentAbility = abilityCasted.ability;
            Vector2 target = abilityCasted.target;
            Character caster = abilityCasted.caster;
            Debug.Log(caster.name + " Cast:" + currentAbility.abilityName + "!!!");
            List<HitInfo>[] hits = aim.CheckImpact(caster.Pos, target, caster, currentAbility);            
            //play animations&stuff
            yield return new WaitForSeconds(0.5f);
            caster.SetOnCooldown(currentAbility);
            ReplicateImpacts(hits, currentAbility, caster);
        }


        for (int i = 0; i < allieds.Count; i++)
        {
            if (!allieds[i].CheckItsAlive())
            {
                allieds[i].AlreadyMove = true;
                mapManager.ActualziatePlayerPos(allieds[i], new Vector2(-1, -1));
            }
        }
        for (int i = 0; i < enemies.Count; i++)
        {
            if (!enemies[i].CheckItsAlive())
            {
                enemies[i].AlreadyMove = true;
                mapManager.ActualziatePlayerPos(enemies[i], new Vector2(-1, -1));
            }
        }
        GC.Collect();
        yield return StartCoroutine(ReplicatedMoves()); 
    }
    void ReplicateImpacts(List<HitInfo>[] hits, Ability currentAbility, Character caster)
    {
        bool energyGiven = false;        
        foreach (HitInfo hit in hits[0])
        {
            if (hit.target != null)
            {
                if(!energyGiven || currentAbility.tags.Contains(AbilityTags.energyPerEnemyHit))
                {
                    caster.AddEnergy(currentAbility.energyProduced);
                    Debug.Log(caster.name + " Produce " + currentAbility.energyProduced + " Energy!");
                }
                foreach (Status status in currentAbility.statusToApply)
                {
                    hit.target.ApplyStatus(new Status(status.type, status.duration));
                }
                hit.target.ReceiveDamage((int)(currentAbility.damage0*caster.DamageMultiply()), hit.cover);
            }
        }
        foreach (HitInfo hit in hits[1])
        {
            if (hit.target != null)
            {
                if (!energyGiven || currentAbility.tags.Contains(AbilityTags.energyPerEnemyHit))
                {
                    caster.AddEnergy(currentAbility.energyProduced);
                }
                foreach (Status status in currentAbility.statusToApply)
                {
                    hit.target.ApplyStatus(new Status(status.type, status.duration));
                }
                hit.target.ReceiveDamage((int)(currentAbility.damage1 * caster.DamageMultiply()), hit.cover);
            }
        }
        foreach (HitInfo hit in hits[2])
        {
            if (hit.target != null)
            {
                if (!energyGiven || currentAbility.tags.Contains(AbilityTags.energyPerEnemyHit))
                {
                    caster.AddEnergy(currentAbility.energyProduced);
                }
                foreach (Status status in currentAbility.statusToApply)
                {
                    hit.target.ApplyStatus(new Status(status.type, status.duration));
                }
                hit.target.ReceiveDamage((int)(currentAbility.damage2 * caster.DamageMultiply()), hit.cover);
            }
        }
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
            Character character = players[data.id].GetComponent<Character>();
            if (character.Alive)
            {
                if (data.sprinting)
                {
                    character.MovScore += character.SprintScore;
                }
                if (data.mov.Count > 0)
                {
                    charactersMoving++;
                    movingCharacters.Add(character);
                    movements.Add(data.GetMov());
                }
                else
                {
                    if (data.followID != "")
                    {
                        followingCharacters.Add(character);
                        followingTargets.Add(players[data.followID].GetComponent<Character>());
                    }
                    else
                    {
                        mapManager.ActualziatePlayerPos(players[data.id].GetComponent<Character>(), players[data.id].GetComponent<Character>().Pos);
                    }
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
                    PickeableBuff cellBuff = currentCell.PickBuff(movingCharacters[i].Team);
                    if (cellBuff != PickeableBuff.None)
                    {
                        Status statusToApply = new Status();
                        switch (cellBuff)
                        {
                            case PickeableBuff.Energize:
                                statusToApply.type = StatusType.energized;
                                statusToApply.duration = 2;
                                break;
                            case PickeableBuff.Heal:
                                statusToApply.type = StatusType.healing;
                                statusToApply.duration = 2;
                                break;
                            case PickeableBuff.Haste:
                                statusToApply.type = StatusType.hasted;
                                statusToApply.duration = 2;
                                break;
                            case PickeableBuff.semiHeal:
                                statusToApply.type = StatusType.healing;
                                statusToApply.duration = 0;
                                break;
                            case PickeableBuff.Might:
                                statusToApply.type = StatusType.might;
                                statusToApply.duration = 2;
                                break;
                        }
                        movingCharacters[i].ApplyStatus(statusToApply);
                    }

                }
                List<Trap> cellTraps = currentCell.CheckTraps(movingCharacters[i].Team);
                foreach(Trap trap in cellTraps)
                {
                    movingCharacters[i].ReceiveDamage(trap.ability.damage0,false);
                    foreach(Status status in trap.ability.statusToApply)
                    {
                        movingCharacters[i].ApplyStatus(new Status(status.type, status.duration));
                    }
                    if (trap.ability.tags.Contains(AbilityTags.energyOnActivate))
                    {
                        trap.caster.AddEnergy(trap.ability.energyProduced);
                    }
                    if (trap.ability.tags.Contains(AbilityTags.destroyOnContact))
                    {
                        trapManager.RemoveTrap(trap);
                    }
                }                
            }

            for (int i = 0; i < followingCharacters.Count; i++)
            {
                if (!followingCharacters[i].CheckItsAlive())
                {
                    followingCharacters[i].AlreadyMove = true;
                    charactersMoving--;
                    mapManager.ActualziatePlayerPos(followingCharacters[i], new Vector2(-1, -1));
                }
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
                    PickeableBuff cellBuff = currentCell.PickBuff(followingCharacters[i].Team);
                    if (cellBuff != PickeableBuff.None)
                    {
                        Status statusToApply = new Status();
                        switch (cellBuff)
                        {
                            case PickeableBuff.Energize:
                                statusToApply.type = StatusType.energized;
                                statusToApply.duration = 2;
                                break;
                            case PickeableBuff.Heal:
                                statusToApply.type = StatusType.healing;
                                statusToApply.duration = 2;
                                break;
                            case PickeableBuff.Haste:
                                statusToApply.type = StatusType.hasted;
                                statusToApply.duration = 2;
                                break;
                            case PickeableBuff.semiHeal:
                                statusToApply.type = StatusType.healing;
                                statusToApply.duration = 0;
                                break;
                            case PickeableBuff.Might:
                                statusToApply.type = StatusType.might;
                                statusToApply.duration = 2;
                                break;
                        }
                        followingCharacters[i].ApplyStatus(statusToApply);
                    }

                }
                List<Trap> cellTraps = currentCell.CheckTraps(movingCharacters[i].Team);
                foreach (Trap trap in cellTraps)
                {
                    movingCharacters[i].ReceiveDamage(trap.ability.damage0, false);
                    foreach (Status status in trap.ability.statusToApply)
                    {
                        movingCharacters[i].ApplyStatus(new Status(status.type, status.duration));
                    }
                    if (trap.ability.tags.Contains(AbilityTags.energyOnActivate))
                    {
                        trap.caster.AddEnergy(trap.ability.energyProduced);
                    }
                    if (trap.ability.tags.Contains(AbilityTags.destroyOnContact))
                    {
                        trapManager.RemoveTrap(trap);
                    }
                }
            }
            for (int i = 0; i < followingCharacters.Count; i++)
            {
                if (!followingCharacters[i].CheckItsAlive())
                {
                    followingCharacters[i].AlreadyMove = true;
                    charactersMoving--;
                    mapManager.ActualziatePlayerPos(followingCharacters[i], new Vector2(-1, -1));
                }
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

public class AbilityCastedInfo
{
    public Character caster;
    public Vector2 target;
    public Ability ability;
}
