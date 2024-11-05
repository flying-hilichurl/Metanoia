using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float time;
    [Header("Move")]
    private float XInput;
    [SerializeField] private float speed = 5.6f;

    [Header("Jump")]
    [SerializeField] private float pressTime;
    private float responseTime=0.3f;
    [SerializeField] private float addedForce=30;
    [SerializeField] private float jumpForce = 8;

    [Header("EnvironmentChange")]
    public bool isLight = true;
    public bool havingChange = false;

    [Header("Check")]
    [SerializeField] private bool isGround;
    [SerializeField] private bool isWall;
    [SerializeField] private LayerMask Sence;

    private Rigidbody2D rb;
    

    void Start()
    {
        rb = this.GetComponent<Rigidbody2D>();
    }


    private void Update()
    {
        if (rb.velocity.y!=0) 
            time += Time.deltaTime;
        Movement();
        Exchange();
        Jump();
    }

    public void Exchange()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            isLight = !isLight;
            havingChange = true;    //在需要切换场景时，将通过hanvingChange向场景脚本传递信号
        }
        else
            havingChange = false;   //在下一帧时，场景切换完毕，把havingChange的值改回来
    }
    private void Movement()
    {
        XInput = Input.GetAxisRaw("Horizontal");
        if (rb.velocity.y == 0)
            rb.velocity = new Vector2(XInput * speed, rb.velocity.y);
        else
            rb.velocity = new Vector2(XInput * speed / 2, rb.velocity.y);    //在空中时移动速度减半
    }

    private void GroundCheck()
    {
        isGround=Physics2D.Raycast(this.transform.position, Vector2.down, 0.55f, Sence);    //地面检测
    }

    private void Jump()
    {
        if (Input.GetButtonDown("Jump"))
        {


            GroundCheck();
            if (isGround)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                responseTime = 0;
                pressTime = 0;
            }
        }
        else if(responseTime<0.3f)
        {
            if (Input.GetButton("Jump"))
            {
                pressTime += Time.deltaTime;
                rb.velocity += new Vector2(0,addedForce*Time.deltaTime);    //实现长按跳跃更远
            }
        }

        if (rb.velocity.y >= 0)
            responseTime += Time.deltaTime;
        else
        {
            if(responseTime>=pressTime)         //使上升下落效果相同
                responseTime-=Time.deltaTime;
            else if(pressTime>=0)
            {
                pressTime-=Time.deltaTime;
                rb.velocity += new Vector2(0, addedForce * Time.deltaTime);
            }
        }
    }
}