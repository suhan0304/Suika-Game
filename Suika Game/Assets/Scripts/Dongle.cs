using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dongle : MonoBehaviour
{
    public GameManager manager; //nextDongle에서 게임매니저를 넘겨받음
    public ParticleSystem effect;
    public int level;
    public bool isDrag;
    public bool isMerge;
    public bool isAttach; //충돌이 작동했는지 확인하는 변수

    public Rigidbody2D rigid;  //물리 효과 제어
    Animator anim; //애니메이션
    CircleCollider2D circle;
    SpriteRenderer spriteRenderer;

    float deadTime;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        circle = GetComponent<CircleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        anim.SetInteger("Level", level);
    }

    void OnDisable()
    {
        //동글 속성 초기화
        level = 0;
        isDrag = false;
        isMerge = false;
        isAttach = false;

        //동글 트랜스폼 초기화
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.zero;

        //동글 물리 초기화
        rigid.simulated = false;
        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;
        circle.enabled = true;
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

    void OnCollisionEnter2D(Collision2D collision)
    {
        StartCoroutine(AttachRoutine());
    }

    IEnumerator AttachRoutine() //충돌음 제한을 위한 코루틴
    {
        if(isAttach) //이미 충돌음을 실행했다면 
            yield break;    //코루틴을 탈출한다.

        isAttach = true;
        manager.SfxPlay(GameManager.Sfx.Attach);

        yield return new WaitForSeconds(0.2f); //0.2f 동안 다른 충돌은 무시

        isAttach = false;
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

        if(targetPos == Vector3.up * 100)
        {
            EffectPlay();
        }

        StartCoroutine(HideRoutine(targetPos));
    }

    IEnumerator HideRoutine(Vector3 targetPos)
    {
        int frameCount = 0;

        while(frameCount < 20)
        {
            frameCount++;//20프레임 실행되도록 
            if(targetPos != Vector3.up * 100)
            {
                transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f);
                yield return null; //프레임 단위로 대기
            }
            else if(targetPos == Vector3.up * 100) //게임매니저가  Hide 시켜준 경우
            {
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 0.2f);
            }
        }

        manager.score += (int)Mathf.Pow(2, level); // 점수 증가

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
        EffectPlay();
        manager.SfxPlay(GameManager.Sfx.LevelUp);

        yield return new WaitForSeconds(0.3f); //애니메이션 실행 시간 대기

        level++; //실제 레벨도 증가

        manager.maxLevel = Mathf.Max(level, manager.maxLevel); // 더 높은 레벨을 반환시켜 최대 레벨을 유지시킨다.

        isMerge = false; //잠금장치 해제
    }

    void OnTriggerStay2D(Collider2D collision) //동글이 라인과 접촉
    {
        if(collision.tag == "Finish") //경계선에 접촉해있으면
        {
            deadTime += Time.deltaTime; //deadTime을 증가시킴

            if (deadTime > 2f)
            {
                // 2초 이상 머무를 시 색을 변경
                spriteRenderer.color = new Color(0.9f, 0.2f, 0.2f);
            }
            if (deadTime > 5f)
            {
                // 5초 이상 머무를 시 게임 오버
                manager.GameOver();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Finish") //경계선 나가면
        {
            deadTime = 0; //deadTime 초기화
            spriteRenderer.color = Color.white; //색 초기화
        }
    }

    void EffectPlay()
    {
        effect.transform.position = transform.position;     //이펙트의 위치는 동글의 위치
        effect.transform.localScale = transform.localScale; //이펙트의 크기는 동글의 크기와 비례
        effect.Play();
    }
}
