using UnityEngine;
using System.Collections;

public class Laser : MonoBehaviour
{
    public LineRenderer laserLineRenderer;
    public float laserWidth = 0.1f;
    public float laserMaxLength = 5f;

    void Start()
    {
    }

    void Update()
    {
        ShootLaserFromTargetPosition(transform.position, transform.forward, laserMaxLength);
    }

    void ShootLaserFromTargetPosition(Vector3 targetPosition, Vector3 direction, float length)
    {
        laserLineRenderer.SetPosition(0, targetPosition);

        RaycastHit raycastHit;

        if (Physics.Raycast(transform.position, transform.forward, out raycastHit))
        {
            if (raycastHit.collider)
            {
                laserLineRenderer.SetPosition(1, raycastHit.point);
            }
        }

        laserLineRenderer.SetPosition(1, transform.forward * 1000);
    }
}