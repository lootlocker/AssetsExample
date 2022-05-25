using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSpinner : MonoBehaviour
{
    public Transform playerTransform;

    public float mouseClickingLerpSpeed;

    public float distanceMultiplier;

    public float rotationBreakLerpSpeed;

    private Vector3 startClickPos;

    private Vector3 endClickPos;

    private float rotationValue;

    private Camera mainCamera;

    [SerializeField]
    private List<Transform> cameraPositions = new List<Transform>();

    public float cameraLerpSpeed;

    private Vector3 cameraTargetPosition;
    private Quaternion cameraTargetRotation;
    // Start is called before the first frame update
    void Start()
    {
        SetCameraPosition(0);
        mainCamera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        float mousePosX = mainCamera.ScreenToViewportPoint(Input.mousePosition).x;
        if (Input.GetMouseButtonDown(0))
        {
            startClickPos.x = mousePosX;
        }
        else if(Input.GetMouseButton(0))
        {
            endClickPos.x = mousePosX;
            startClickPos = Vector3.Lerp(startClickPos, endClickPos, mouseClickingLerpSpeed * Time.deltaTime);
            rotationValue += (startClickPos.x - endClickPos.x) * distanceMultiplier;
        }
        else
        {
            startClickPos = Vector3.Lerp(startClickPos, endClickPos, mouseClickingLerpSpeed * Time.deltaTime);
            rotationValue += (startClickPos.x - endClickPos.x)*distanceMultiplier;
        }
        rotationValue = Mathf.Lerp(rotationValue, 0f, Time.deltaTime * rotationBreakLerpSpeed);
        playerTransform.localEulerAngles += new Vector3(0, rotationValue, 0);

        transform.localPosition = Vector3.Lerp(transform.localPosition, cameraTargetPosition, Time.deltaTime * cameraLerpSpeed);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, cameraTargetRotation, Time.deltaTime * cameraLerpSpeed);
    }

    public void SetCameraPosition(int index)
    {
        cameraTargetPosition = cameraPositions[index].localPosition;
        cameraTargetRotation = cameraPositions[index].localRotation;
    }
}
