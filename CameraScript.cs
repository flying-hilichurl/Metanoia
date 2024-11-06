using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private bool needReturn=false;
    [SerializeField] Player player;
    private Vector3 originalPos;
    [SerializeField] private Transform transform1;
    public Vector3 shakePos;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Shake();
    }

    private void Shake()
    {
        if (player.shakeTime > 0)
        {
            shakePos = Random.insideUnitSphere*0.03f;
            this.transform.position = transform1.position +shakePos ;
            player.shakeTime -= Time.deltaTime;
            needReturn = true;
        }
        else if (needReturn)
        { 
            this.transform.position = transform1.position;
            needReturn = false;
        }
    }
}
