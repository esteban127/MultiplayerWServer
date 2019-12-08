using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    GameObject lastCell;
    GameObject lastCellClicked;
    GameObject characterOnMause;
    GameObject characterClicked;
    Transform cameraTransform;
    float cameraSpeed = 20f;
    GameDirector gameDirector;
    
    [SerializeField] MapManager map;    
    public static bool inputEneable = true;

    bool aimingAbility = false;
    public bool AimingAbility { set { aimingAbility = value; } }

    private void Start()
    {
        cameraTransform = Camera.main.transform;
        gameDirector = GetComponent<GameDirector>();
    }

    void FixedUpdate()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        int layer_mask = LayerMask.GetMask("Characters");
        if (Physics.Raycast(ray, out hit, 50f, layer_mask))
        {
            if (lastCell != null)
                lastCell.GetComponent<Cell>().HighLight(false);

            if (aimingAbility)
            {
                gameDirector.Aiming(new Vector2(hit.point.x, hit.point.z));
            }
            else
            {
                if (characterOnMause != hit.transform.gameObject)
                {
                    if (characterOnMause != null)
                    {
                        //characterOnMause.GetComponent<Cell>().HighLight(false);
                    }
                    characterOnMause = hit.transform.gameObject;
                    //characterOnMause.GetComponent<Cell>().HighLight(true);
                }
            }
        }
        else
        {
            characterOnMause = null;
        }
        if(characterOnMause == null)
        {
            layer_mask = LayerMask.GetMask("Cells");
            if (Physics.Raycast(ray, out hit, 50f, layer_mask))
            {
                if (aimingAbility)
                {
                    gameDirector.Aiming(new Vector2(hit.point.x, hit.point.z));
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
                if (characterOnMause != null && !aimingAbility)
                {
                    characterClicked = characterOnMause;
                    //characterClicked.GetComponent<Cell>().ClickedHighLight(true);
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
                        gameDirector.MovCommand(currentNode);                        
                        lastCellClicked = null;
                    }
                    else
                    {
                        if (characterClicked != null)
                        {
                            gameDirector.CharacterClicked(characterClicked);
                        }
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
                SelectAbility(0);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SelectAbility(1);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SelectAbility(2);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                SelectAbility(3);
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                SelectAbility(4);
            }
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                SelectAbility(5);
            }
            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                SelectAbility(6);
            }
        }

        CameraImputs();              
        
    }

    public void SelectAbility(int abilitySlot)
    {
        aimingAbility = gameDirector.SelectAbility(abilitySlot);
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
