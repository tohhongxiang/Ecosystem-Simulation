using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    private CharacterController characterController;
    private Animator animator;
    public float speed = 5.0f;

    private int velocityXHash;
    private int velocityZHash;
    private int isDrinkingHash;
    private int isEatingHash;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        animator.speed = speed;

        velocityXHash = Animator.StringToHash("velocityX");
        velocityZHash = Animator.StringToHash("velocityZ");
        isDrinkingHash = Animator.StringToHash("isDrinking");
        isEatingHash = Animator.StringToHash("isEating");
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        animator.SetFloat(velocityXHash, move.x);
        animator.SetFloat(velocityZHash, move.z);

        animator.SetBool(isDrinkingHash, Input.GetKey(KeyCode.Space));
        animator.SetBool(isEatingHash, Input.GetKey(KeyCode.E));

        // characterController.Move(move * speed * Time.deltaTime);
    }
}
