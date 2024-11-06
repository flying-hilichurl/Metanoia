using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Move")]
    private float XInput;
    private float YInput;
    [SerializeField] private float speed = 4f;

    [Header("Jump")]
    private float pressTime;
    private float responseTime = 0.3f;
    [SerializeField] private float addedForce = 22;
    [SerializeField] private float jumpForce = 3.7f;
    private float slowTime;
    private bool getTime = false;

    [Header("EnvironmentChange")]
    public bool isLight = true;
    public bool havingChange = false;

    [Header("Check")]
    [SerializeField] private bool isGround;
    [SerializeField] private LayerMask Sence;

    [Header("Dush")]
    private float dushTime1 = 0;
    private float dushTime2 = 0;
    private float XDir;
    private float YDir;
    private bool haveDushed = false;
    private bool isDushing=false;

    [Header("Shake")]
    public float shakeTime = 0;
    private Vector3 offestPos;

    [Header("ClimbWall")]
    private bool onWall=false;
    private bool haveClimbed=false;
    private float approachTime;
    private float waitTime;
    private float climbTime;

    [Header("AnimationCondition")]
    private bool isMoving=false;

    enum myDirection { left=-1,right=1}
    private myDirection direction = myDirection.right;

    private Rigidbody2D rb;
    private Animator anim;
    [SerializeField] CameraScript cameraScript;

    void Start()
    {
        rb = this.GetComponent<Rigidbody2D>();
        anim= this.GetComponentInChildren<Animator>();
    }


    private void Update()
    {
        GetInput();
        isGround = GroundCheck();       //本不想每帧都检测的，但还是被逼无奈
        Movement();
        Exchange();
        ClimbWall();
        Jump();
        Dush();
        //Shake();
        AnimationController();
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
        if (!isDushing&&!onWall)
        {
            if (rb.velocity.y == 0)
                rb.velocity = new Vector2(XInput * speed, rb.velocity.y);
            else
                rb.velocity = new Vector2(XInput * speed / 2, rb.velocity.y);    //在空中时移动速度减半
        }
    }

    private bool GroundCheck()
    {
        bool isGround;
        isGround = Physics2D.Raycast(this.transform.position, Vector2.down, 0.55f, Sence);    //地面检测
        return isGround;
    }

    private bool WallCheck()
    {
        bool isWall;
        isWall = Physics2D.Raycast(this.transform.position, new Vector2(1f * (float)direction,0), 0.40f, Sence);
        return isWall;
    }

    private void Jump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (isGround||onWall)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                responseTime = 0;
                pressTime = 0;
            }
        }
        else if (responseTime < 0.3f)
        {
            if (Input.GetButton("Jump"))
            {
                pressTime += Time.deltaTime;
                rb.velocity += new Vector2(0, addedForce * Time.deltaTime);    //实现长按跳跃更远
            }
        }
        if (rb.velocity.y >= 0)
            responseTime += Time.deltaTime;
        else
        {
            if (getTime)
                slowTime = pressTime;
            if (responseTime >= slowTime)         //使上升下落效果相同
                responseTime -= Time.deltaTime;
            else if (pressTime >= 0)
            {
                pressTime -= Time.deltaTime;
                rb.velocity += new Vector2(0, addedForce * Time.deltaTime);
            }
        }
    }

    private void GetInput()
    {
            XInput = Input.GetAxisRaw("Horizontal");
            YInput = Input.GetAxisRaw("Vertical");
    }

    private void Dush()
    {
        if(isGround||onWall)
            haveDushed = false;         //在天空中只能冲刺一次 

        if (Input.GetKeyDown(KeyCode.LeftShift)&&!haveDushed)
        {
            dushTime1 = 0.2f;
            dushTime2 = 0.2f;
            shakeTime = 0.10f;
            rb.gravityScale = 0;
            isDushing= true;
            if(XInput != 0 && YInput != 0)
            {
                XDir = XInput/1.41f;                 //保证始终移动方向为按下按键时的输入
                YDir = YInput/1.41f;
            }
            else if(XInput == 0 &&YInput == 0)
            {
                XDir = 1;
                YDir = 0;
            }
            else
            {
                XDir = XInput;                 
                YDir = YInput;
            }
        }


        if (dushTime1 > 0)          //冲刺速度先快后慢
        {
            dushTime1 -= Time.deltaTime;
            rb.velocity= new Vector3(XDir*9f,YDir*7f, 0);
        }
        else if (dushTime2 > 0)
        {
            dushTime2 -= Time.deltaTime;
            rb.velocity= new Vector3( XDir*9f,YDir*7f, 0);
            haveDushed = true;
        }
        else
        {
            if(!onWall)
                rb.gravityScale = 2;
            if(isDushing)
                rb.velocity = new Vector3(0, 0, 0);
            isDushing = false;
        }
    }

    /*private void Shake()
    {
        if(shakeTime>0)
        {
           // this.transform.position +=cameraScript.shakePos;
        }
    }*/

    private void ClimbWall()
    {
        if (isGround)
            haveClimbed = false;        //落地前禁止爬墙

        if(Input.GetKeyDown(KeyCode.J)&&WallCheck()&&!haveClimbed)       //爬墙
        {
            approachTime = 0.05f;
            climbTime = 3f;
            waitTime = 0.20f;
            onWall = true;
            haveClimbed=true;
        }

        if (approachTime > 0)       //在给予玩家容错的情况下，避免玩家与墙之间存在空隙
        {
            approachTime -= Time.deltaTime;
            waitTime-= Time.deltaTime;
            rb.velocity = new Vector2(10, 0);
            rb.gravityScale = 0;
        }
        else if (climbTime > 0)         //3秒后掉下
        {
            waitTime -= Time.deltaTime;
            if (Input.GetKeyUp(KeyCode.J)&&waitTime<0&&!WallCheck())
                climbTime = 0;
            if (Input.GetButtonDown("Jump"))
                climbTime = 0;
            climbTime -= Time.deltaTime;
            rb.velocity = new Vector2(0, YInput*speed / 2 );
        }
        else
        {
            onWall = false;
            rb.gravityScale = 2;
        }
    }

    private void AnimationController()
    {
        if(rb.velocity.x!=0&& rb.velocity.y==0&&!isDushing)
            isMoving = true;
        else
            isMoving = false;

        anim.SetBool("isMoving", isMoving);
    }
}