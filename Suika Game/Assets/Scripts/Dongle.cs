using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dongle : MonoBehaviour
{
    public GameManager manager; //nextDongle���� ���ӸŴ����� �Ѱܹ���

    public int level;
    public bool isDrag;
    public bool isMerge;

    Rigidbody2D rigid;  //���� ȿ�� ����
    Animator anim; //�ִϸ��̼�
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

    void OnCollisionStay2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Dongle")
        {
            //�浹�� ������ �����´�.
            Dongle other = collision.gameObject.GetComponent<Dongle>();

            //1. ���� ���� ������ ���� ��
            //2. ���� ��ġ�� ���� �ƴҶ�
            //3. ��밡 ��ġ�� ���� �ƴҶ�
            //4. level�� 7���� ������
            if(level == other.level && !isMerge && !other.isMerge && level < 7)
            {
                //���� ��ġ�� ����

                //���� ����� ��ġ ��������
                float meX = transform.position.x;
                float meY = transform.position.y;
                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;

                //1. ���� �Ʒ��� ������
                //2. ������ ���̿� ������, ���� �����ʿ� ������
                if(meY < otherY || (meY == otherY && meX > otherX))
                {
                    //������ �����
                    other.Hide(transform.position); //������ ���� ���� �����̸鼭 �����.

                    //���� ������
                    LevelUp();
                }
            }
        }
    }

    public void Hide(Vector3 targetPos)
    {
        isMerge = true; //�����ġ �ɾ�α�

        //��� �̵��� ���� ����ȿ�� ��� ��Ȱ��ȭ

        rigid.simulated = false; //������ٵ� 2D ����ȿ�� ����
        circle.enabled = false;  //��Ŭ �ݶ��̴� 2D ��Ȱ��ȭ

        StartCoroutine(HideRoutine(targetPos));
    }

    IEnumerator HideRoutine(Vector3 targetPos)
    {
        int frameCount = 0;

        while(frameCount < 20)
        {
            frameCount++;//20������ ����ǵ��� 
            transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f);
            yield return null; //������ ������ ���
        }

        isMerge = false; //��ġ�� ����
        gameObject.SetActive(false); //������ �Ϸ�����Ƿ� ��Ȱ��ȭ
    }

    void LevelUp()
    {
        isMerge = true; //�����ġ �ɾ�α�

        rigid.velocity = Vector2.zero; // �������� �ʵ��� �ӵ� 0���� ����
        rigid.angularVelocity = 0; // ȸ�� �ӵ��� 0���� ����

        StartCoroutine(LevelUpRoutine());
    }

    IEnumerator LevelUpRoutine()
    {
        yield return new WaitForSeconds(0.2f);

        anim.SetInteger("Level", level + 1); //���� �÷��� �ִϸ��̼� ����

        yield return new WaitForSeconds(0.3f); //�ִϸ��̼� ���� �ð� ���

        level++; //���� ������ ����

        manager.maxLevel = Mathf.Max(level, manager.maxLevel); // �� ���� ������ ��ȯ���� �ִ� ������ ������Ų��.

        isMerge = false; //�����ġ ����
    }
}
