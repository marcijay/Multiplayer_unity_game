using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnMovement : NetworkBehaviour
{
    [SerializeField]
    private float speed;

    [SerializeField]
    private float jumpSpeed;

    [SerializeField]
    private float gravityScale;

    private Vector3 _velocity;

    private CharacterController _characterController;
    private PawnInput _input;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        _characterController = GetComponent<CharacterController>();
        _input = GetComponent<PawnInput>();
    }

    private void Update()
    {
        if (!IsOwner) return;

        Vector3 desiredVelocity = Vector3.ClampMagnitude(((transform.forward * _input.vertical) + (transform.right * _input.horizontal)) * speed, speed);

        _velocity.x = desiredVelocity.x;
        _velocity.z = desiredVelocity.z;

        if (_characterController.isGrounded)
        {
            _velocity.y = 0.0f;

            if (_input.jump)
            {
                _velocity.y = jumpSpeed;
            }
        }
        else
        {
            _velocity.y += Physics.gravity.y * gravityScale * Time.deltaTime;
        }

        _characterController.Move(_velocity * Time.deltaTime);
    }
}
