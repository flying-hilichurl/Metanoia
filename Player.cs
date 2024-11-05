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
            havingChange = true;    //����Ҫ�л�����ʱ����ͨ��hanvingChange�򳡾��ű������ź�
        }
        else
            havingChange = false;   //����һ֡ʱ�������л���ϣ���havingChange��ֵ�Ļ���
    }
    private void Movement()
    {
        XInput = Input.GetAxisRaw("Horizontal");
        if (rb.velocity.y == 0)
            rb.velocity = new Vector2(XInput * speed, rb.velocity.y);
        else
            rb.velocity = new Vector2(XInput * speed / 2, rb.velocity.y);    //�ڿ���ʱ�ƶ��ٶȼ���
    }

    private void GroundCheck()
    {
        isGround=Physics2D.Raycast(this.transform.position, Vector2.down, 0.55f, Sence);    //������
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
                rb.velocity += new Vector2(0,addedForce*Time.deltaTime);    //ʵ�ֳ�����Ծ��Զ
            }
        }

        if (rb.velocity.y >= 0)
            responseTime += Time.deltaTime;
        else
        {
            if(responseTime>=pressTime)         //ʹ��������Ч����ͬ
                responseTime-=Time.deltaTime;
            else if(pressTime>=0)
            {
                pressTime-=Time.deltaTime;
                rb.velocity += new Vector2(0, addedForce * Time.deltaTime);
            }
        }
    }
}