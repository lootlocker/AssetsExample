using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomLookAt : MonoBehaviour
{
    public Transform head;

    public Transform cameraTransform;

    public float lerpSpeed;

    Quaternion targetRotationHead;
    Quaternion previousFrameRotationHead;

    void LateUpdate()
    {
        RotateHead();
    }

    void RotateHead()
    {
        Quaternion savedRotation = head.rotation;
        head.rotation = Quaternion.LookRotation(cameraTransform.position - head.position);
        if ((head.localEulerAngles.y >= 0 && head.localEulerAngles.y <= 90) || (head.localEulerAngles.y >= 270 && head.localEulerAngles.y <= 360))
        {
            targetRotationHead = Quaternion.Lerp(previousFrameRotationHead, Quaternion.LookRotation(cameraTransform.position - head.position), Time.deltaTime * 8f);
        }
        else
        {
            targetRotationHead = Quaternion.Lerp(previousFrameRotationHead, savedRotation, Time.deltaTime * 8f);
        }
        head.rotation = targetRotationHead;
        previousFrameRotationHead = targetRotationHead;
    }
}
