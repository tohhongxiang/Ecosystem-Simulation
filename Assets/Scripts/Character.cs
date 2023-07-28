using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Animator))]
public class Character : MonoBehaviour
{
    private CharacterController characterController;
    private Animator animator;
    
    [SerializeField] private float speed = 2.0f;
    private float gravity = 9.81f;
    private float yVelocity = 0;
    [SerializeField] private float gravityMultiplier = 3.0f;

    private int velocityXHash;
    private int velocityZHash;
    private int isDrinkingHash;
    private int isEatingHash;
    private int isAttackingHash;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        animator.speed = speed;

        velocityXHash = Animator.StringToHash("velocityX");
        velocityZHash = Animator.StringToHash("velocityZ");
        isDrinkingHash = Animator.StringToHash("isDrinking");
        isEatingHash = Animator.StringToHash("isEating");
        isAttackingHash = Animator.StringToHash("isAttacking");
    }

    // Update is called once per frame
    void Update()
    {
        ApplyGravity();

        Vector3 direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        Move(direction);

        animator.SetBool(isDrinkingHash, Input.GetKey(KeyCode.Space));
        animator.SetBool(isEatingHash, Input.GetKey(KeyCode.E));
        animator.SetBool(isAttackingHash, Input.GetKey(KeyCode.R));

        // characterController.Move(move * speed * Time.deltaTime);
    }

    private void Move(Vector3 direction) {
        animator.SetFloat(velocityXHash, direction.x);
        animator.SetFloat(velocityZHash, direction.z);
    }

    private void ApplyGravity() {
        if (characterController.isGrounded) {
            yVelocity = 1;
        } else {
            yVelocity += gravity * gravityMultiplier * Time.deltaTime;
        }

        characterController.Move(Vector3.down * yVelocity * Time.deltaTime);
    }
}
