using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swinging : MonoBehaviour
{
    [Header("References")]
    public LineRenderer lr;
    public Transform gunTip, cam, player;
    public LayerMask whatIsGrappleable;
    public PlayerMovement pm;

    [Header("Swinging")]
    private float maxSwingDistance = 25f;
    private Vector3 swingPoint;
    private SpringJoint joint;

    [Header("ODM Gear")]
    public Transform orientation;
    public Rigidbody rb;
    public float horizontalThrust = 2000f;
    public float forwardThrust = 3000f;
    public float extendCableSpeed = 20f;


    public KeyCode swingKey = KeyCode.Mouse1;

    private void Start() {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
    }
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(swingKey)) { StartSwing(); }
        if(Input.GetKeyUp(swingKey)) { StopSwing(); }

        if(joint!= null) OdmGearMovement();
    }

    private void LateUpdate() {
        DrawRope();
    }

    private void StartSwing()
    {
        pm.isSwinging = true;
        RaycastHit hit;

        if(Physics.Raycast(cam.position, cam.forward, out hit, maxSwingDistance, whatIsGrappleable))
        {
            swingPoint = hit.point;
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = swingPoint;

            float distanceFromPoint = Vector3.Distance(player.position, swingPoint);

            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;

            joint.spring = 4.5f;
            joint.damper = 7f;
            joint.massScale = 4.5f;

            lr.positionCount = 2;
        }
    }

    private void StopSwing()
    {
        pm.isSwinging = false;
        lr.positionCount = 0;
        Destroy(joint);
    }

    private void OdmGearMovement()
    {
         if(Input.GetKey(KeyCode.D)) rb.AddForce(orientation.right * horizontalThrust * Time.deltaTime);
         if(Input.GetKey(KeyCode.A)) rb.AddForce(-orientation.right * horizontalThrust * Time.deltaTime);

         if(Input.GetKey(KeyCode.W)) rb.AddForce(orientation.forward * forwardThrust * Time.deltaTime);

         if(Input.GetKey(KeyCode.Space))
         {
            Vector3 directionToPoint = swingPoint - transform.position;
            rb.AddForce(directionToPoint.normalized * forwardThrust * Time.deltaTime);
            
            float distanceFromPoint = Vector3.Distance(transform.position,swingPoint);
            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;
         }

         if(Input.GetKey(KeyCode.S))
         {
            float extendDistanceFromPoint = Vector3.Distance(transform.position,swingPoint) + extendCableSpeed;
            joint.maxDistance = extendDistanceFromPoint * 0.8f;
            joint.minDistance = extendDistanceFromPoint * 0.25f;
         }
    }

    private Vector3 currentGrapplePos;
    private void DrawRope()
    {
        if(!joint){ return; }
        currentGrapplePos = Vector3.Lerp(currentGrapplePos, swingPoint, Time.deltaTime * 8f);
        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, currentGrapplePos);
    }
}
