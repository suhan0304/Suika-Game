using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("--------------[ Core ]")]
    public int score;
    public int maxLevel;
    public bool isOver;

    [Header("--------------[ Object Pooling ]")]
    public GameObject donglePrefab; //동글 프리팹
    public Transform dongleGroup;   //동글이 생성될 위치
    public List<Dongle> donglePool;
    public GameObject effectPrefab; //이펙트 프리팹
    public Transform effectGroup;   //이펙트가 생성될 위치
    public List<ParticleSystem> effectPool;
    [Range(1, 30)]
    public int poolSize;
    public int poolCursor;

    public Dongle lastDongle;
    
    [Header("--------------[ Audio ]")]
    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer;
    public AudioClip[] sfxClip; //여러 효과음들이 담길 변수
    public enum Sfx { LevelUp, Next, Attach, Button, Over };
    int sfxCursor; //다음에 재생할 AudioSource를 가리킬 변수

    [Header("--------------[ UI ]")]
    public GameObject startGroup;
    public GameObject endGroup;
    public Text scoreText;
    public Text maxScoreText;
    public Text subScoreText;

    [Header("--------------[ ETC ]")]
    public GameObject bottom;
    public GameObject line;
    public GameObject[] wall;

    private void Awake()
    {
        Application.targetFrameRate = 60;

        donglePool = new List<Dongle>();
        effectPool = new List<ParticleSystem>();

        for (int i = 0; i < poolSize; i++)
        {
            MakeDongle(); //풀 만들기
        }

        if (!PlayerPrefs.HasKey("MaxScore")) //저장된 최고 점수가 없다면
        {
            PlayerPrefs.SetInt("MaxScore", 0); //MaxScore라는 이름으로 최고 점수 저장
        }
        maxScoreText.text = PlayerPrefs.GetInt("MaxScore").ToString();
}

    public void GameStart()
    {
        //오브젝트 활성화
        line.SetActive(true);
        bottom.SetActive(true);
        wall[0].SetActive(true);
        wall[1].SetActive(true);
        scoreText.gameObject.SetActive(true);
        maxScoreText.gameObject.SetActive(true);

        //게임 시작 UI 비활성화
        startGroup.SetActive(false);

        bgmPlayer.Play();
        SfxPlay(Sfx.Button);

        Invoke("NextDongle", 1.5f);
    }

    Dongle MakeDongle()
    {
        //이펙트 생성
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        instantEffectObj.name = "Effect " + effectPool.Count; //이름을 설정
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);

        //동글 프리팹 복사해서 가져옴, 이 때 부모는 동글 그룹으로 설정
        GameObject instantDongleObj = Instantiate(donglePrefab, dongleGroup);
        instantDongleObj.name = "Dongle " + donglePool.Count; //이름을 설정
        Dongle instantDongle = instantDongleObj.GetComponent<Dongle>();
        instantDongle.manager = this; //manager 변수도 초기화
        instantDongle.effect = instantEffect;
        donglePool.Add(instantDongle);

        return instantDongle;
    }

    Dongle GetDongle()
    {
        for(int i=0;i<donglePool.Count;i++)
        {
            poolCursor = (poolCursor+1) % donglePool.Count; //커서가 오브젝트 풀을 계속 회전하도록 설정
            if(!donglePool[poolCursor].gameObject.activeSelf) //비활성화된 오브젝트를 찾으면 해당 오브젝트를 사용
            {
                return donglePool[poolCursor]; //비활성화 되어있는 동글을 넘김
            }
        }
        return MakeDongle(); //만약 donglePool에 없다면 MakeDongle로 풀에 하나 추가해서 넘김
    }

    void NextDongle()
    {
        if(isOver)
            return;

        //생성된 동글을 가져와 new Dongle로 지정
        lastDongle = GetDongle();
        lastDongle.level = Random.Range(0, maxLevel); //레벨 0 ~ maxLevel-1에서 랜덤하게 생성되도록 구현
        lastDongle.gameObject.SetActive(true); //레벨 설정 후 활성화

        SfxPlay(Sfx.Next);
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

        yield return new WaitForSeconds(1f);


        //최고 점수 갱신
        int maxSocre = Mathf.Max(score, PlayerPrefs.GetInt("MaxScore"));
        PlayerPrefs.SetInt("MaxScore", maxSocre);

        //게임 오버 UI 표시
        endGroup.SetActive(true);
        subScoreText.text = "점수 : " + scoreText.text;

        bgmPlayer.Stop();
        SfxPlay(Sfx.Over);
    }

    public void Reset()
    {
        SfxPlay(Sfx.Button); //버튼 선택음 
        StartCoroutine(ResetCoroutine()); //리셋 코루틴 실행
    }

    IEnumerator ResetCoroutine()
    {
        yield return new WaitForSeconds(1f); //1초 대기후
        SceneManager.LoadScene(0);           //0번 장면(게임씬) 로드
    }

    public void SfxPlay(Sfx type)
    {
        switch(type)
        {
            case Sfx.LevelUp:
                sfxPlayer[sfxCursor].clip = sfxClip[Random.Range(0, 3)]; //레벨업은 소리가 3개라서 랜덤하게 실행
                break;
            case Sfx.Next:
                sfxPlayer[sfxCursor].clip = sfxClip[3];
                break;
            case Sfx.Attach:
                sfxPlayer[sfxCursor].clip = sfxClip[4];
                break;
            case Sfx.Button:
                sfxPlayer[sfxCursor].clip = sfxClip[5];
                break;
            case Sfx.Over:
                sfxPlayer[sfxCursor].clip = sfxClip[6];
                break;
        }

        sfxPlayer[sfxCursor].Play(); //재생시킬 AudioClip이 들어간 Audio Source를 실행

        sfxCursor = (sfxCursor + 1) % sfxPlayer.Length; //계속해서 3개의 오디오 소스를 순환하도록 구현
    }

    void Update()
    {
        if(Input.GetButtonDown("Cancel")) //모바일용 게임종료함수
        {
            Application.Quit();
        }    
    }

    private void LateUpdate()
    {
        scoreText.text = score.ToString();
    }
}
