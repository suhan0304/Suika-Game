using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Dongle lastDongle;
    public GameObject donglePrefab; //���� ������
    public Transform dongleGroup;   //������ ������ ��ġ
    public GameObject effectPrefab; //����Ʈ ������
    public Transform effectGroup;   //����Ʈ�� ������ ��ġ

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
        //����Ʈ ����
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();

        //���� ������ �����ؼ� ������, �� �� �θ�� ���� �׷����� ����
        GameObject instantDongleObj = Instantiate(donglePrefab, dongleGroup);
        Dongle instantDongle = instantDongleObj.GetComponent<Dongle>();
        instantDongle.effect = instantEffect;
        return instantDongle;
    }

    void NextDongle()
    {
        if(isOver)
            return;

        //������ ������ ������ new Dongle�� ����
        Dongle newDongle = GetDongle();
        lastDongle = newDongle;
        lastDongle.manager = this; //���ӸŴ����� �Ѱ��ش�.
        lastDongle.level = Random.Range(0, maxLevel); //���� 0 ~ maxLevel-1���� �����ϰ� �����ǵ��� ����
        lastDongle.gameObject.SetActive(true); //���� ���� �� Ȱ��ȭ
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
    }
}
