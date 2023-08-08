using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlignToSurface : MonoBehaviour
{
    RaycastHit hit;
    Vector3 theRay;

    public LayerMask terainMask;

    void FixedUpdate()
    {
        Align();
    }
  
    private void Align()
    {
        theRay = -transform.up;

        if (Physics.Raycast(transform.position,
            theRay, out hit, 20, terainMask))
        {

            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.parent.rotation;
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime / 0.15f);
        }
    }
}
