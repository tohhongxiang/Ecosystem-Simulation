using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlignToTerrain : MonoBehaviour
{
    RaycastHit hit;
    Vector3 theRay;

    void FixedUpdate()
    {
        Align();
    }

    private void Align()
    {
        theRay = -transform.up;
        if (Physics.Raycast(new Vector3(transform.position.x, transform.position.y, transform.position.z),
            theRay, out hit, 20))
        {
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.parent.rotation;
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime / 0.15f);
        }
    }
}
