using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Dongle lastDongle;
    public GameObject donglePrefab; //동글 프리팹
    public Transform dongleGroup;   //동글이 생성될 위치

    private void Start()
    {
        NextDongle();
    }

    Dongle GetDongle()
    {
        //동글 프리팹 복사해서 가져옴, 이 때 부모는 동글 그룹으로 설정
        GameObject instant = Instantiate(donglePrefab, dongleGroup);
        Dongle instantDongle = instant.GetComponent<Dongle>();
        return instantDongle;
    }

    void NextDongle()
    {
        //생성된 동글을 가져와 new Dongle로 지정
        Dongle newDongle = GetDongle();
        lastDongle = newDongle;

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
}
