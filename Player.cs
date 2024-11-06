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
        isGround = GroundCheck();       //������ÿ֡�����ģ������Ǳ�������
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
            havingChange = true;    //����Ҫ�л�����ʱ����ͨ��hanvingChange�򳡾��ű������ź�
        }
        else
            havingChange = false;   //����һ֡ʱ�������л���ϣ���havingChange��ֵ�Ļ���
    }
    private void Movement()
    {
        if (!isDushing&&!onWall)
        {
            if (rb.velocity.y == 0)
                rb.velocity = new Vector2(XInput * speed, rb.velocity.y);
            else
                rb.velocity = new Vector2(XInput * speed / 2, rb.velocity.y);    //�ڿ���ʱ�ƶ��ٶȼ���
        }
    }

    private bool GroundCheck()
    {
        bool isGround;
        isGround = Physics2D.Raycast(this.transform.position, Vector2.down, 0.55f, Sence);    //������
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
                rb.velocity += new Vector2(0, addedForce * Time.deltaTime);    //ʵ�ֳ�����Ծ��Զ
            }
        }
        if (rb.velocity.y >= 0)
            responseTime += Time.deltaTime;
        else
        {
            if (getTime)
                slowTime = pressTime;
            if (responseTime >= slowTime)         //ʹ��������Ч����ͬ
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
            haveDushed = false;         //�������ֻ�ܳ��һ�� 

        if (Input.GetKeyDown(KeyCode.LeftShift)&&!haveDushed)
        {
            dushTime1 = 0.2f;
            dushTime2 = 0.2f;
            shakeTime = 0.10f;
            rb.gravityScale = 0;
            isDushing= true;
            if(XInput != 0 && YInput != 0)
            {
                XDir = XInput/1.41f;                 //��֤ʼ���ƶ�����Ϊ���°���ʱ������
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


        if (dushTime1 > 0)          //����ٶ��ȿ����
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
            haveClimbed = false;        //���ǰ��ֹ��ǽ

        if(Input.GetKeyDown(KeyCode.J)&&WallCheck()&&!haveClimbed)       //��ǽ
        {
            approachTime = 0.05f;
            climbTime = 3f;
            waitTime = 0.20f;
            onWall = true;
            haveClimbed=true;
        }

        if (approachTime > 0)       //�ڸ�������ݴ������£����������ǽ֮����ڿ�϶
        {
            approachTime -= Time.deltaTime;
            waitTime-= Time.deltaTime;
            rb.velocity = new Vector2(10, 0);
            rb.gravityScale = 0;
        }
        else if (climbTime > 0)         //3������
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