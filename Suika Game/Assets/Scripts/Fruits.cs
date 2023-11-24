using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fruits : MonoBehaviour
{
    public GameManager manager; //nextFruits���� ���ӸŴ����� �Ѱܹ���
    public ParticleSystem effect;
    public int level;
    public bool isDrag;
    public bool isMerge;
    public bool isAttach; //�浹�� �۵��ߴ��� Ȯ���ϴ� ����

    public Rigidbody2D rigid;  //���� ȿ�� ����
    Animator anim; //�ִϸ��̼�
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
        //���� �Ӽ� �ʱ�ȭ
        level = 0;
        isDrag = false;
        isMerge = false;
        isAttach = false;

        //���� Ʈ������ �ʱ�ȭ
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.zero;

        //���� ���� �ʱ�ȭ
        rigid.simulated = false;
        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;
        circle.enabled = true;
    }

    void Update()
    {
        if(isDrag) //���콺 ���󰡴� ������ �巡�� �����϶��� �۵��ϵ��� ����
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            //x�� ��輳��
            float leftBorder = -7f + transform.localScale.x / 2f; //��1�� x��ǥ�� -5�̰� �β��� 0.5�̹Ƿ� ���� ������ ����  -4.65���� ����, ������ �������� + ���ش�.
            float rightBorder = 7f - transform.localScale.x / 2f; //��2�� x��ǥ�� 5�̰� �β��� 0.5�̹Ƿ� ���� ���� ���� 4.65���� ����, ������ �������� - ���ش�.

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

    void OnCollisionEnter2D(Collision2D collision)
    {
        StartCoroutine(AttachRoutine());
    }

    IEnumerator AttachRoutine() //�浹�� ������ ���� �ڷ�ƾ
    {
        if(isAttach) //�̹� �浹���� �����ߴٸ� 
            yield break;    //�ڷ�ƾ�� Ż���Ѵ�.

        isAttach = true;
        manager.SfxPlay(GameManager.Sfx.Attach);

        yield return new WaitForSeconds(0.2f); //0.2f ���� �ٸ� �浹�� ����

        isAttach = false;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Fruits")
        {
            //�浹�� ������ �����´�.
            Fruits other = collision.gameObject.GetComponent<Fruits>();

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
            frameCount++;//20������ ����ǵ��� 
            if(targetPos != Vector3.up * 100)
            {
                transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f);
                yield return null; //������ ������ ���
            }
            else if(targetPos == Vector3.up * 100) //���ӸŴ�����  Hide ������ ���
            {
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 0.2f);
            }
        }

        manager.score += (int)Mathf.Pow(2, level); // ���� ����

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
        EffectPlay();
        manager.SfxPlay(GameManager.Sfx.LevelUp);

        yield return new WaitForSeconds(0.3f); //�ִϸ��̼� ���� �ð� ���

        level++; //���� ������ ����

        manager.maxLevel = Mathf.Max(level, manager.maxLevel); // �� ���� ������ ��ȯ���� �ִ� ������ ������Ų��.

        isMerge = false; //�����ġ ����
    }

    void OnTriggerStay2D(Collider2D collision) //������ ���ΰ� ����
    {
        if(collision.tag == "Finish") //��輱�� ������������
        {
            deadTime += Time.deltaTime; //deadTime�� ������Ŵ

            if (deadTime > 2f)
            {
                // 2�� �̻� �ӹ��� �� ���� ����
                spriteRenderer.color = new Color(0.9f, 0.2f, 0.2f);
            }
            if (deadTime > 5f)
            {
                // 5�� �̻� �ӹ��� �� ���� ����
                manager.GameOver();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Finish") //��輱 ������
        {
            deadTime = 0; //deadTime �ʱ�ȭ
            spriteRenderer.color = Color.white; //�� �ʱ�ȭ
        }
    }

    void EffectPlay()
    {
        effect.transform.position = transform.position;     //����Ʈ�� ��ġ�� ������ ��ġ
        effect.transform.localScale = transform.localScale; //����Ʈ�� ũ��� ������ ũ��� ���
        effect.Play();
    }
}
