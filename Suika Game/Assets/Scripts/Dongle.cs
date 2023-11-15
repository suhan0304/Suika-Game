using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dongle : MonoBehaviour
{
    public int level;
    public bool isDrag; 
    Rigidbody2D rigid;  //���� ȿ�� ����
    Animator anim; //�ִϸ��̼�

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
        if(isDrag) //���콺 ���󰡴� ������ �巡�� �����϶��� �۵��ϵ��� ����
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            //x�� ��輳��
            float leftBorder = -4.65f + transform.localScale.x / 2f; //��1�� x��ǥ�� -5�̰� �β��� 0.5�̹Ƿ� ���� ������ ����  -4.65���� ����, ������ �������� + ���ش�.
            float rightBorder = 4.65f - transform.localScale.x / 2f; //��2�� x��ǥ�� 5�̰� �β��� 0.5�̹Ƿ� ���� ���� ���� 4.65���� ����, ������ �������� - ���ش�.

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
        isDrag = true; //�巡�� ��
    }

    public void Drop()
    {
        isDrag = false; //�巡�� ����
        rigid.simulated = true; //�ٽ� simulated�� true�� �ٲ㼭 ���� �ùķ��̼ǰ� ��ȣ�ۿ��ϵ��� Ȱ��ȭ
    }
}
