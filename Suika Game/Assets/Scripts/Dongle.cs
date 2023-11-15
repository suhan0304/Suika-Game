using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dongle : MonoBehaviour
{
    public int level;
    public bool isDrag; 
    Rigidbody2D rigid;  //물리 효과 제어
    Animator anim; //애니메이션

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void OnEnable()
    {
        anim.SetInteger("Level", level);
    }

    void Update()
    {
        if(isDrag) //마우스 따라가는 로직이 드래그 상태일때만 작동하도록 수정
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            //x축 경계설정
            float leftBorder = -4.65f + transform.localScale.x / 2f; //벽1의 x좌표는 -5이고 두께가 0.5이므로 벽의 오른쪽 끝을  -4.65으로 설정, 동글의 반지름도 + 해준다.
            float rightBorder = 4.65f - transform.localScale.x / 2f; //벽2의 x좌표는 5이고 두께가 0.5이므로 벽의 왼쪽 끝을 4.65으로 설정, 동글의 반지름도 - 해준다.

            if (mousePos.x < leftBorder)
            {
                mousePos.x = leftBorder;
            }
            else if (mousePos.x > rightBorder)
            {
                mousePos.x = rightBorder;
            }
            mousePos.y = 8;
            mousePos.z = 0;
            transform.position = Vector3.Lerp(transform.position, mousePos, 0.2f);
        }
    }

    public void Drag()
    {
        isDrag = true; //드래그 중
    }

    public void Drop()
    {
        isDrag = false; //드래그 종료
        rigid.simulated = true; //다시 simulated를 true로 바꿔서 물리 시뮬레이션과 상호작용하도록 활성화
    }
}
