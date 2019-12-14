using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapModel : MonoBehaviour
{
    [SerializeField] bool drawLine = false;
    [SerializeField] Transform lineOrigin = null;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] Vector3 deledtePos = new Vector3(-100, 100 - 100);

    private void Start()
    {
        transform.parent = null;
        DisableTrap();        
    }

    public void SetTrap(Vector2 castPlace, Vector2 aimTarget)
    {
        Vector2 direction = aimTarget - castPlace;
        float angle = Vector2.Angle(new Vector2(1, 0), direction);
        if (direction.y > 0)
        {
            angle *=-1;
        }
        Debug.Log(angle);
        transform.position = new Vector3(castPlace.x, 1.4f, castPlace.y);
        transform.eulerAngles = new Vector3(transform.localRotation.x, angle ,transform.localRotation.z);
        Vector3[] aiming = new Vector3[2];
        aiming[0] = lineOrigin.position;
        aiming[1] = new Vector3(aimTarget.x, 0, aimTarget.y);
        lineRenderer.positionCount = 2;
        lineRenderer.SetPositions(aiming);
    }
    public void DisableTrap()
    {
        if (drawLine)
        {
            lineRenderer.positionCount = 0;
        }
        transform.position = deledtePos;
    }
}
