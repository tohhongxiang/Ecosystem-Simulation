using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Animator))]
public class Character : MonoBehaviour
{
    // components
    private CharacterController characterController;
    private Animator animator;
    private int velocityXHash;
    private int velocityZHash;
    private int isDrinkingHash;
    private int isEatingHash;
    private int isAttackingHash;

    [Header("Movement parameters")]
    [SerializeField] private float speed = 2.0f;
    private float gravity = 9.81f;
    private float yVelocity = 0;
    [SerializeField] private float gravityMultiplier = 3.0f;


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

        Drink(Input.GetKey(KeyCode.Space));
        Eat(Input.GetKey(KeyCode.E));
        Attack(Input.GetKey(KeyCode.R));
    }

    private void Move(Vector3 direction) {
        animator.SetFloat(velocityXHash, direction.x);
        animator.SetFloat(velocityZHash, direction.z);
    }

    private void Drink(bool isDrinking) {
        animator.SetBool(isDrinkingHash, isDrinking);
    }

    private void Attack(bool isAttacking) {
        animator.SetBool(isAttackingHash, isAttacking);
    }

    private void Eat(bool isEating) {
        animator.SetBool(isEatingHash, isEating);
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
