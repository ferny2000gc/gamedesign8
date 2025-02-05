using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState{
    idle,
    walk,
    attack,
    run,
    interact,
    stagger
}

public class PlayerMovement : MonoBehaviour
{
    public PlayerState currentState;
    private float speed = 5;
    private Rigidbody2D myRigidbody;
    private Vector3 change;
    private Animator animator;
    public FloatValue currentHealth;
    public Signal playerHealthSignal;

    // Start is called before the first frame update
    void Start()
    {
        currentState = PlayerState.walk;
        animator = GetComponent<Animator>();
        myRigidbody = GetComponent<Rigidbody2D>();
        animator.SetFloat("moveX", 0);
        animator.SetFloat("moveY", -1);
    }

    // Update is called once per frame
    void Update()
    {
        change = Vector3.zero;
        change.x = Input.GetAxisRaw("Horizontal");
        change.y = Input.GetAxisRaw("Vertical");
        if (Input.GetButtonDown("attack") && currentState != PlayerState.attack
            && currentState != PlayerState.stagger) 
        {
            StartCoroutine(AttackCo());
        } 
        else if (currentState == PlayerState.walk || currentState == PlayerState.idle || currentState == PlayerState.run) 
        {
            UpdateAnimationAndMove();
        }
    }

    private IEnumerator AttackCo() 
    {
        animator.SetBool("attacking", true);
        currentState = PlayerState.attack;
        yield return null;
        animator.SetBool("attacking", false);
        yield return new WaitForSeconds(.3f);
        currentState = PlayerState.walk;
    }

    void UpdateAnimationAndMove(){
        if(change != Vector3.zero){
            MoveCharacter();
            animator.SetFloat("moveX", change.x);
            animator.SetFloat("moveY", change.y);
            animator.SetBool("moving", true);
        } else {
            animator.SetBool("moving", false);
        }
        if(change != Vector3.zero && Input.GetKey(KeyCode.LeftShift)){
            currentState = PlayerState.run;
            animator.SetBool("run", true);
            speed = 10;
        } else {
            currentState = PlayerState.walk;
            animator.SetBool("run", false);
            speed = 5;
        }
    }

    void MoveCharacter(){
        change.Normalize();
        myRigidbody.MovePosition(
            transform.position + change * speed * Time.deltaTime
        );
    }

    public void Knock(float knockTime, float damage)
    {
        currentHealth.RuntimeValue -= damage;
        playerHealthSignal.Raise();

        if(currentHealth.RuntimeValue > 0)
        {
            StartCoroutine(KnockCo(knockTime));
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }

    private IEnumerator KnockCo(float knockTime)
    {
        if(myRigidbody != null)
        {
            yield return new WaitForSeconds(knockTime);
            myRigidbody.velocity = Vector2.zero;
            currentState = PlayerState.idle;
            myRigidbody.velocity = Vector2.zero;
        }
    }
}
