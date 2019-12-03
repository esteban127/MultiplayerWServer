using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    GameObject lastCell;
    GameObject lastCellClicked;
    Transform cameraTransform;
    float cameraSpeed = 20f;
    MovementManager movementMan;
    GameDirector gameDirector;
    
    [SerializeField] MapManager map;    
    public static bool inputEneable = true;

    bool aimingAbility = false;

    private void Start()
    {
        cameraTransform = Camera.main.transform;
        movementMan = GetComponent<MovementManager>();
        gameDirector = GetComponent<GameDirector>();
    }

    void FixedUpdate()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Debug.DrawRay(ray.origin,ray.direction * 50f, Color.red);
        int layer_mask = LayerMask.GetMask("Cells");        
        if (Physics.Raycast(ray, out hit, 50f, layer_mask))
        {
            if (aimingAbility)
            {
                gameDirector.Aiming(new Vector2 (hit.point.x,hit.point.z));
            }
            else
            {
                if (lastCell != hit.transform.gameObject)
                {
                    if (lastCell != null)
                        lastCell.GetComponent<Cell>().HighLight(false);
                    lastCell = hit.transform.gameObject;
                    lastCell.GetComponent<Cell>().HighLight(true);
                }
            }            
        }
        else
        {
            lastCell = null;
        }      

    }
    private void Update()
    {
        if (inputEneable)
        {
            if (Input.GetMouseButtonDown(0)) //cellClick
            {
                if (lastCell != null && !aimingAbility)
                {
                    lastCellClicked = lastCell;
                    lastCellClicked.GetComponent<Cell>().ClickedHighLight(true);
                }
            }
            if (Input.GetMouseButtonUp(0)) //cellReleased
            {
                if (aimingAbility)
                {
                    gameDirector.ConfirmAim();
                }
                else
                {
                    if (lastCellClicked != null)
                    {
                        lastCellClicked.GetComponent<Cell>().ClickedHighLight(false);
                        if (lastCellClicked == lastCell)
                            lastCell.GetComponent<Cell>().HighLight(true);
                        Node currentNode = map.GetNodeFromWorldPosition(lastCellClicked.transform.position);
                        if (movementMan.WalkableNode(currentNode))
                        {
                            gameDirector.MovCommand(currentNode);
                        }
                        else if (movementMan.SprintableNode(currentNode))
                        {
                            gameDirector.SprintCommand(currentNode);
                        }
                        lastCellClicked = null;
                    }
                }                         
            }
            if (Input.GetMouseButtonUp(1)) 
            {
                if (aimingAbility)
                {
                    gameDirector.CancelAim();
                    aimingAbility = false;
                }
                else
                {
                    gameDirector.DeleteLastAction();
                }
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                gameDirector.ReadyToEndTurn();
            }
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                aimingAbility = true;
            }
        }

        CameraImputs();              
        
    }

    private void CameraImputs()
    {
        if (Input.GetAxisRaw("Horizontal") != 0)
        {
            cameraTransform.position = cameraTransform.position + cameraTransform.right * Time.deltaTime * cameraSpeed * Input.GetAxisRaw("Horizontal");
        }
        if (Input.GetAxisRaw(("Vertical")) != 0)
        {
            cameraTransform.position = cameraTransform.position + new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z).normalized * Time.deltaTime * cameraSpeed * Input.GetAxisRaw("Vertical");
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            cameraTransform.RotateAround(new Vector3(9, 0, 9), new Vector3(0, 1, 0), 90);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            cameraTransform.RotateAround(new Vector3(9, 0, 9), new Vector3(0, 1, 0), -90);
        }
    }
}
