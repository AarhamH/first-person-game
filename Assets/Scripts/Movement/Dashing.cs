using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dashing : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerCam;
    private Rigidbody rb;
    private PlayerMovement pm;

    [Header("Dashing Settings")]
    public float dashForce = 20f;
    public float dashUpwardForce;
    public float dashDuration = 0.25f;
    private Vector3 delayedForceToApply;

    [Header("CoolDown")]
    public float dashCooldown = 1.5f;
    public float dashCooldownTimer;

    [Header("Key Binds")]
    public KeyCode dashkey = KeyCode.E;

    private void Start() {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
    }

    private void Update() {
        if(Input.GetKeyDown(dashkey))
        {
            Dash();
        }

        if(dashCooldownTimer>0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
    }

    private void Dash()
    {
        if(dashCooldownTimer > 0) { return; }
        else { dashCooldownTimer = dashCooldown;}

        pm.isDashing = true;
        Vector3 forceToApply = orientation.forward * dashForce + orientation.up * dashUpwardForce;
        delayedForceToApply = forceToApply;
        Invoke(nameof(DelayedDashForce),0.025f);

        Invoke(nameof(ResetDash), dashDuration);
    }

    private void DelayedDashForce()
    {
        rb.AddForce(delayedForceToApply,ForceMode.Impulse);
    }

    private void ResetDash()
    {
        pm.isDashing = false;
    }
}
