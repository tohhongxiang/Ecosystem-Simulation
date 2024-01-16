using System.Collections;
using System.Collections.Generic;
using Unity.Entities.UniversalDelegates;
using UnityEngine;

public class AlignToSurface : MonoBehaviour
{
    RaycastHit hit;
    Vector3 theRay;

    public LayerMask terainMask;

    private int frameCount = 0;
    public int updateFrequency = 3;
    void FixedUpdate()
    {
        if (frameCount % updateFrequency == 0)
        {
            Align();
        }

        frameCount += 1;
    }

    private void Align()
    {
        theRay = -transform.up;

        if (Physics.Raycast(transform.position,
            theRay, out hit, 2, terainMask))
        {

            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.parent.rotation;
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime / 0.15f);
        }
    }
}
