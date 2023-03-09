using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX = 200f; 
    public float sensY = 200f;

    public Transform orientation;
    public GameObject player;
    private Transform armsPivot;
    private Transform cameraPos;

    float xRotation;
    float yRotation;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        armsPivot = player.transform.Find("PlayerObj/ArmsPivot");
        cameraPos = player.transform.Find("CameraPos");
        orientation = player.transform.Find("PlayerObj/Orientation");
        
        armsPivot.transform.position = cameraPos.transform.position;
    }

    private void Update()
    {
        // get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // rotate cam and orientation
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        armsPivot.rotation = Quaternion.Euler(xRotation, yRotation, 0);
    }
}