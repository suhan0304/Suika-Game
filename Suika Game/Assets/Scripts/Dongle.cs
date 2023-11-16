using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dongle : MonoBehaviour
{
    public GameManager manager; //nextDongle에서 게임매니저를 넘겨받음

    public int level;
    public bool isDrag;
    public bool isMerge;

    Rigidbody2D rigid;  //물리 효과 제어
    Animator anim; //애니메이션
    CircleCollider2D circle;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        circle = GetComponent<CircleCollider2D>();
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

    void OnCollisionStay2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Dongle")
        {
            //충돌한 동글을 가져온다.
            Dongle other = collision.gameObject.GetComponent<Dongle>();

            //1. 상대와 나의 레벨이 같을 때
            //2. 내가 합치는 중이 아닐때
            //3. 상대가 합치는 중이 아닐때
            //4. level이 7보다 낮을때
            if(level == other.level && !isMerge && !other.isMerge && level < 7)
            {
                //동글 합치기 로직

                //나와 상대편 위치 가져오기
                float meX = transform.position.x;
                float meY = transform.position.y;
                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;

                //1. 내가 아래에 있을때
                //2. 동일한 높이에 있을때, 내가 오른쪽에 있을때
                if(meY < otherY || (meY == otherY && meX > otherX))
                {
                    //상대방은 숨기기
                    other.Hide(transform.position); //상대방은 나를 향해 움직이면서 숨긴다.

                    //나는 레벨업
                    LevelUp();
                }
            }
        }
    }

    public void Hide(Vector3 targetPos)
    {
        isMerge = true; //잠금장치 걸어두기

        //흡수 이동을 위해 물리효과 모두 비활성화

        rigid.simulated = false; //리지드바디 2D 물리효과 중지
        circle.enabled = false;  //서클 콜라이더 2D 비활성화

        StartCoroutine(HideRoutine(targetPos));
    }

    IEnumerator HideRoutine(Vector3 targetPos)
    {
        int frameCount = 0;

        while(frameCount < 20)
        {
            frameCount++;//20프레임 실행되도록 
            transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f);
            yield return null; //프레임 단위로 대기
        }

        isMerge = false; //합치기 종료
        gameObject.SetActive(false); //숨김이 완료됐으므로 비활성화
    }

    void LevelUp()
    {
        isMerge = true; //잠금장치 걸어두기

        rigid.velocity = Vector2.zero; // 움직이지 않도록 속도 0으로 설정
        rigid.angularVelocity = 0; // 회전 속도도 0으로 설정

        StartCoroutine(LevelUpRoutine());
    }

    IEnumerator LevelUpRoutine()
    {
        yield return new WaitForSeconds(0.2f);

        anim.SetInteger("Level", level + 1); //레벨 올려서 애니메이션 실행

        yield return new WaitForSeconds(0.3f); //애니메이션 실행 시간 대기

        level++; //실제 레벨도 증가

        manager.maxLevel = Mathf.Max(level, manager.maxLevel); // 더 높은 레벨을 반환시켜 최대 레벨을 유지시킨다.

        isMerge = false; //잠금장치 해제
    }
}
