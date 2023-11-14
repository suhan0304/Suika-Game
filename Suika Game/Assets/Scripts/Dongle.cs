using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dongle : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //x축 경계설정
        float leftBorder = -4.65f + transform.localScale.x / 2f; //벽1의 x좌표는 -5이고 두께가 0.5이므로 벽의 오른쪽 끝을  -4.65으로 설정, 동글의 반지름도 + 해준다.
        float rightBorder = 4.65f - transform.localScale.x / 2f; //벽2의 x좌표는 5이고 두께가 0.5이므로 벽의 왼쪽 끝을 4.65으로 설정, 동글의 반지름도 - 해준다.

        if (mousePos.x < leftBorder)
        {
            mousePos.x = leftBorder;
        }
        else if(mousePos.x > rightBorder)
        {
            mousePos.x = rightBorder;
        }
        mousePos.y = 8;
        mousePos.z = 0;
        transform.position = Vector3.Lerp(transform.position, mousePos,0.2f);
    }
}
