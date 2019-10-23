using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public float distance = 50f;
    GameObject lastCell;
    GameObject lastCellClicked;
    Transform cameraTransform;
    float cameraSpeed = 20f;

    private void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    void FixedUpdate()
    {       
            
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        int layer_mask = LayerMask.GetMask("Cells");
        if (Physics.Raycast(ray, out hit, distance, layer_mask))
        {
            if (lastCell != hit.transform.gameObject)
            {
                if (lastCell != null)
                    lastCell.GetComponent<Cell>().HighLight(false);
                lastCell = hit.transform.gameObject;
                lastCell.GetComponent<Cell>().HighLight(true);
            }
            if (Input.GetMouseButtonDown(0)) //cellClick
            {
                lastCellClicked = hit.transform.gameObject;
                lastCellClicked.GetComponent<Cell>().ClickedHighLight(true);
            }
        }        

    }
    private void Update()
    {
        
        if (Input.GetMouseButtonUp(0)) //cellReleased
        {
            if (lastCellClicked != null)
            {
                lastCellClicked.GetComponent<Cell>().ClickedHighLight(false);
                if (lastCellClicked == lastCell)
                    lastCell.GetComponent<Cell>().HighLight(true);

                lastCellClicked = null;
            }            
        }
        if(Input.GetAxisRaw("Horizontal") != 0)
        {
            cameraTransform.position = cameraTransform.position + cameraTransform.right * Time.deltaTime * cameraSpeed *Input.GetAxisRaw("Horizontal");
        }
        if (Input.GetAxisRaw(("Vertical")) != 0)
        {           
            cameraTransform.position = cameraTransform.position +new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z).normalized * Time.deltaTime * cameraSpeed* Input.GetAxisRaw("Vertical");
        }        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            cameraTransform.RotateAround(new Vector3(9, 0, -9), new Vector3(0, 1, 0), 90);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            cameraTransform.RotateAround(new Vector3(9, 0, -9), new Vector3(0, 1, 0), -90);
        }
    }
}
