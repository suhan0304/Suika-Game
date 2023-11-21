using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("--------------[ Core ]")]
    public int score;
    public int maxLevel;
    public bool isOver;

    [Header("--------------[ Object Pooling ]")]
    public GameObject donglePrefab; //���� ������
    public Transform dongleGroup;   //������ ������ ��ġ
    public List<Dongle> donglePool;
    public GameObject effectPrefab; //����Ʈ ������
    public Transform effectGroup;   //����Ʈ�� ������ ��ġ
    public List<ParticleSystem> effectPool;
    [Range(1, 30)]
    public int poolSize;
    public int poolCursor;

    public Dongle lastDongle;
    
    [Header("--------------[ Audio ]")]
    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer;
    public AudioClip[] sfxClip; //���� ȿ�������� ��� ����
    public enum Sfx { LevelUp, Next, Attach, Button, Over };
    int sfxCursor; //������ ����� AudioSource�� ����ų ����

    [Header("--------------[ UI ]")]
    public Text scoreText;

    private void Awake()
    {
        Application.targetFrameRate = 60;

        donglePool = new List<Dongle>();
        effectPool = new List<ParticleSystem>();

        for (int i = 0; i < poolSize; i++)
        {
            MakeDongle(); //Ǯ �����
        }
    }

    private void Start()
    {
        bgmPlayer.Play();
        NextDongle();
    }

    Dongle MakeDongle()
    {
        //����Ʈ ����
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        instantEffectObj.name = "Effect " + effectPool.Count; //�̸��� ����
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);

        //���� ������ �����ؼ� ������, �� �� �θ�� ���� �׷����� ����
        GameObject instantDongleObj = Instantiate(donglePrefab, dongleGroup);
        instantDongleObj.name = "Dongle " + donglePool.Count; //�̸��� ����
        Dongle instantDongle = instantDongleObj.GetComponent<Dongle>();
        instantDongle.manager = this; //manager ������ �ʱ�ȭ
        instantDongle.effect = instantEffect;
        donglePool.Add(instantDongle);

        return instantDongle;
    }

    Dongle GetDongle()
    {
        for(int i=0;i<donglePool.Count;i++)
        {
            poolCursor = (poolCursor+1) % donglePool.Count; //Ŀ���� ������Ʈ Ǯ�� ��� ȸ���ϵ��� ����
            if(!donglePool[poolCursor].gameObject.activeSelf) //��Ȱ��ȭ�� ������Ʈ�� ã���� �ش� ������Ʈ�� ���
            {
                return donglePool[poolCursor]; //��Ȱ��ȭ �Ǿ��ִ� ������ �ѱ�
            }
        }
        return MakeDongle(); //���� donglePool�� ���ٸ� MakeDongle�� Ǯ�� �ϳ� �߰��ؼ� �ѱ�
    }

    void NextDongle()
    {
        if(isOver)
            return;

        //������ ������ ������ new Dongle�� ����
        lastDongle = GetDongle();
        lastDongle.level = Random.Range(0, maxLevel); //���� 0 ~ maxLevel-1���� �����ϰ� �����ǵ��� ����
        lastDongle.gameObject.SetActive(true); //���� ���� �� Ȱ��ȭ

        SfxPlay(Sfx.Next);
        StartCoroutine(WaitNext()); //����� NextDongle�� �����ϴ� �ڷ�ƾ ����
    }

    IEnumerator WaitNext()
    {
        while(lastDongle != null)
        {
            yield return null; //�� �������� ����Ѵ�.
        }

        yield return new WaitForSeconds(2.5f); //2.5�ʸ� ����Ѵ�

        NextDongle();
    }

    public void TouchDown()
    {
        if (lastDongle == null) //lastDongle�� ������ �������� ����
            return;

        lastDongle.Drag();
    }

    public void TouchUp()
    {
        if (lastDongle == null) //lastDongle�� ������ �������� ����
            return;
        lastDongle.Drop();
        lastDongle = null; //����ϸ鼭 �����뵵�� �����ص� ������ null�� ����.
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
        Dongle[] dongles = GameObject.FindObjectsOfType<Dongle>();

        //2. ����� ���� ��� ������ ����ȿ�� ��Ȱ��ȭ
        for (int i = 0; i < dongles.Length; i++)
        {
            dongles[i].rigid.simulated = false; //����ȿ�� ��ȭ��ȭ
        }

        //3. 1�� ����� �ϳ��� �����ؼ� �����
        for (int i = 0; i < dongles.Length; i++)
        {
            dongles[i].Hide(Vector3.up * 100); //���� �÷��� �߿��� ���� �� ���� ū���� �����Ͽ� �����

            yield return new WaitForSeconds(0.1f); //�ð����� �ΰ� ������ ��������� ���
        }

        yield return new WaitForSeconds(1f);

        SfxPlay(Sfx.Over);
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

    private void LateUpdate()
    {
        scoreText.text = score.ToString();
    }
}
