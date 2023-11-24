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
    public GameObject FruitsPrefab; //���� ������
    public Transform FruitsGroup;   //������ ������ ��ġ
    public List<Fruits> FruitsPool;
    public GameObject effectPrefab; //����Ʈ ������
    public Transform effectGroup;   //����Ʈ�� ������ ��ġ
    public List<ParticleSystem> effectPool;
    [Range(1, 30)]
    public int poolSize;
    public int poolCursor;

    public Fruits lastFruits;
    
    [Header("--------------[ Audio ]")]
    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer;
    public AudioClip[] sfxClip; //���� ȿ�������� ��� ����
    public enum Sfx { LevelUp, Next, Attach, Button, Over };
    int sfxCursor; //������ ����� AudioSource�� ����ų ����

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

        FruitsPool = new List<Fruits>();
        effectPool = new List<ParticleSystem>();

        for (int i = 0; i < poolSize; i++)
        {
            MakeFruits(); //Ǯ �����
        }

        if (!PlayerPrefs.HasKey("MaxScore")) //����� �ְ� ������ ���ٸ�
        {
            PlayerPrefs.SetInt("MaxScore", 0); //MaxScore��� �̸����� �ְ� ���� ����
        }
        maxScoreText.text = PlayerPrefs.GetInt("MaxScore").ToString();
}

    public void GameStart()
    {
        //������Ʈ Ȱ��ȭ
        line.SetActive(true);
        bottom.SetActive(true);
        wall[0].SetActive(true);
        wall[1].SetActive(true);
        scoreText.gameObject.SetActive(true);
        maxScoreText.gameObject.SetActive(true);

        //���� ���� UI ��Ȱ��ȭ
        startGroup.SetActive(false);

        bgmPlayer.Play();
        SfxPlay(Sfx.Button);

        Invoke("NextFruits", 1.5f);
    }

    Fruits MakeFruits()
    {
        //����Ʈ ����
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        instantEffectObj.name = "Effect " + effectPool.Count; //�̸��� ����
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);

        //���� ������ �����ؼ� ������, �� �� �θ�� ���� �׷����� ����
        GameObject instantFruitsObj = Instantiate(FruitsPrefab, FruitsGroup);
        instantFruitsObj.name = "Fruits " + FruitsPool.Count; //�̸��� ����
        Fruits instantFruits = instantFruitsObj.GetComponent<Fruits>();
        instantFruits.manager = this; //manager ������ �ʱ�ȭ
        instantFruits.effect = instantEffect;
        FruitsPool.Add(instantFruits);

        return instantFruits;
    }

    Fruits GetFruits()
    {
        for(int i=0;i<FruitsPool.Count;i++)
        {
            poolCursor = (poolCursor+1) % FruitsPool.Count; //Ŀ���� ������Ʈ Ǯ�� ��� ȸ���ϵ��� ����
            if(!FruitsPool[poolCursor].gameObject.activeSelf) //��Ȱ��ȭ�� ������Ʈ�� ã���� �ش� ������Ʈ�� ���
            {
                return FruitsPool[poolCursor]; //��Ȱ��ȭ �Ǿ��ִ� ������ �ѱ�
            }
        }
        return MakeFruits(); //���� FruitsPool�� ���ٸ� MakeFruits�� Ǯ�� �ϳ� �߰��ؼ� �ѱ�
    }

    void NextFruits()
    {
        if(isOver)
            return;

        //������ ������ ������ new Fruits�� ����
        lastFruits = GetFruits();
        lastFruits.level = Random.Range(0, maxLevel); //���� 0 ~ maxLevel-1���� �����ϰ� �����ǵ��� ����
        lastFruits.gameObject.SetActive(true); //���� ���� �� Ȱ��ȭ

        SfxPlay(Sfx.Next);
        StartCoroutine(WaitNext()); //����� NextFruits�� �����ϴ� �ڷ�ƾ ����
    }

    IEnumerator WaitNext()
    {
        while(lastFruits != null)
        {
            yield return null; //�� �������� ����Ѵ�.
        }

        yield return new WaitForSeconds(2.5f); //2.5�ʸ� ����Ѵ�

        NextFruits();
    }

    public void TouchDown()
    {
        if (lastFruits == null) //lastFruits�� ������ �������� ����
            return;

        lastFruits.Drag();
    }

    public void TouchUp()
    {
        if (lastFruits == null) //lastFruits�� ������ �������� ����
            return;
        lastFruits.Drop();
        lastFruits = null; //����ϸ鼭 �����뵵�� �����ص� ������ null�� ����.
    }

    public void GameOver()
    {
        if (isOver) //�̹� ���� ������ ���¸� return
            return;

        isOver = true;

        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        //1. ��� �ȿ� Ȱ��ȭ �Ǿ��ִ� ��� ���� ��������
        Fruits[] Fruitss = GameObject.FindObjectsOfType<Fruits>();

        //2. ����� ���� ��� ������ ����ȿ�� ��Ȱ��ȭ
        for (int i = 0; i < Fruitss.Length; i++)
        {
            Fruitss[i].rigid.simulated = false; //����ȿ�� ��ȭ��ȭ
        }

        //3. 1�� ����� �ϳ��� �����ؼ� �����
        for (int i = 0; i < Fruitss.Length; i++)
        {
            Fruitss[i].Hide(Vector3.up * 100); //���� �÷��� �߿��� ���� �� ���� ū���� �����Ͽ� �����

            yield return new WaitForSeconds(0.1f); //�ð����� �ΰ� ������ ��������� ���
        }

        yield return new WaitForSeconds(1f);


        //�ְ� ���� ����
        int maxSocre = Mathf.Max(score, PlayerPrefs.GetInt("MaxScore"));
        PlayerPrefs.SetInt("MaxScore", maxSocre);

        //���� ���� UI ǥ��
        endGroup.SetActive(true);
        subScoreText.text = "���� : " + scoreText.text;

        bgmPlayer.Stop();
        SfxPlay(Sfx.Over);
    }

    public void Reset()
    {
        SfxPlay(Sfx.Button); //��ư ������ 
        StartCoroutine(ResetCoroutine()); //���� �ڷ�ƾ ����
    }

    IEnumerator ResetCoroutine()
    {
        yield return new WaitForSeconds(1f); //1�� �����
        SceneManager.LoadScene(0);           //0�� ���(���Ӿ�) �ε�
    }

    public void SfxPlay(Sfx type)
    {
        switch(type)
        {
            case Sfx.LevelUp:
                sfxPlayer[sfxCursor].clip = sfxClip[Random.Range(0, 3)]; //�������� �Ҹ��� 3���� �����ϰ� ����
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

        sfxPlayer[sfxCursor].Play(); //�����ų AudioClip�� �� Audio Source�� ����

        sfxCursor = (sfxCursor + 1) % sfxPlayer.Length; //����ؼ� 3���� ����� �ҽ��� ��ȯ�ϵ��� ����
    }

    void Update()
    {
        if(Input.GetButtonDown("Cancel")) //����Ͽ� ���������Լ�
        {
            Application.Quit();
        }    
    }

    private void LateUpdate()
    {
        scoreText.text = score.ToString();
    }
}
