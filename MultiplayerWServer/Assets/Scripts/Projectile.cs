using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    Vector3 Objetive = new Vector3(-1,-1,-1);
    Vector3 DeletedPos = new Vector3(-100, 100, -100);
    Vector3 aimDirection;
    float distanceToTravel = 0;
    [SerializeField] Vector3 offset;
    [SerializeField] float speed;
    public void Spawn(Vector2 castPlace, Vector2 aimTarget)
    {
        Vector2 direction = aimTarget - castPlace;
        distanceToTravel = direction.magnitude;
        Debug.Log(distanceToTravel);
        float angle = Vector2.Angle(new Vector2(1, 0), direction);
        if (direction.y > 0)
        {
            angle *= -1;
        }
        aimDirection = new Vector3(direction.x, 0, direction.y);
        transform.position = new Vector3(castPlace.x, 1, castPlace.y) + aimDirection.normalized * offset.magnitude;
        transform.eulerAngles = new Vector3(transform.localEulerAngles.x, angle, transform.localEulerAngles.z);
        Objetive = new Vector3(aimTarget.x, 1, aimTarget.y);
    }
    public void DeleteProjectile()
    {
        transform.position = DeletedPos;
        distanceToTravel = 0;
        Objetive = new Vector3(-1, -1, -1);
    }
    private void Start()
    {
        DeleteProjectile();
    }

    private void Update()
    {
        if(Objetive!= new Vector3(-1, -1, -1))
        {
            if (distanceToTravel > 0)
            {
                transform.position += (aimDirection.normalized * Time.deltaTime * speed);
                distanceToTravel -= (aimDirection.normalized * Time.deltaTime * speed).magnitude;
            }
            else
            {
                DeleteProjectile();
            }            
        }
    }

}
