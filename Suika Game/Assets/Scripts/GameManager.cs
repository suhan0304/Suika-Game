using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Dongle lastDongle;
    public GameObject donglePrefab; //동글 프리팹
    public Transform dongleGroup;   //동글이 생성될 위치
    public GameObject effectPrefab; //이펙트 프리팹
    public Transform effectGroup;   //이펙트가 생성될 위치

    public int score;
    public int maxLevel;
    public bool isOver;

    private void Awake()
    {
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        NextDongle();
    }

    Dongle GetDongle()
    {
        //이펙트 생성
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();

        //동글 프리팹 복사해서 가져옴, 이 때 부모는 동글 그룹으로 설정
        GameObject instantDongleObj = Instantiate(donglePrefab, dongleGroup);
        Dongle instantDongle = instantDongleObj.GetComponent<Dongle>();
        instantDongle.effect = instantEffect;
        return instantDongle;
    }

    void NextDongle()
    {
        if(isOver)
            return;

        //생성된 동글을 가져와 new Dongle로 지정
        Dongle newDongle = GetDongle();
        lastDongle = newDongle;
        lastDongle.manager = this; //게임매니저를 넘겨준다.
        lastDongle.level = Random.Range(0, maxLevel); //레벨 0 ~ maxLevel-1에서 랜덤하게 생성되도록 구현
        lastDongle.gameObject.SetActive(true); //레벨 설정 후 활성화
        StartCoroutine(WaitNext()); //대기후 NextDongle을 실행하는 코루틴 시작
    }

    IEnumerator WaitNext()
    {
        while(lastDongle != null)
        {
            yield return null; //한 프레임을 대기한다.
        }

        yield return new WaitForSeconds(2.5f); //2.5초를 대기한다

        NextDongle();
    }

    public void TouchDown()
    {
        if (lastDongle == null) //lastDongle이 없으면 실행하지 않음
            return;

        lastDongle.Drag();
    }

    public void TouchUp()
    {
        if (lastDongle == null) //lastDongle이 없으면 실행하지 않음
            return;
        lastDongle.Drop();
        lastDongle = null; //드랍하면서 보관용도로 저장해둔 변수는 null로 비운다.
    }

    public void GameOver()
    {
        if (isOver) //이미 게임 오버된 상태면 return
            return;

        isOver = true;

        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        //1. 장면 안에 활성화 되어있는 모든 동글 가져오기
        Dongle[] dongles = GameObject.FindObjectsOfType<Dongle>();

        //2. 지우기 전에 모든 동글의 물리효과 비활성화
        for (int i = 0; i < dongles.Length; i++)
        {
            dongles[i].rigid.simulated = false; //물리효과 비화성화
        }

        //3. 1번 목록을 하나씩 접근해서 지우기
        for (int i = 0; i < dongles.Length; i++)
        {
            dongles[i].Hide(Vector3.up * 100); //게임 플레이 중에는 나올 수 없는 큰값을 전달하여 숨기기

            yield return new WaitForSeconds(0.1f); //시간차를 두고 동글이 사라지도록 대기
        }
    }
}
