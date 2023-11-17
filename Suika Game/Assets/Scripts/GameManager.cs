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

    public int maxLevel;


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
        return instantDongle;
    }

    void NextDongle()
    {
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
}
