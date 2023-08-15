using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] GameManager gameManager;
    [SerializeField] LayerMask blockLayer;
    public enum DIRECTION_TYPE
    {
        STOP,
        RIGHT,
        LEFT
    }
    DIRECTION_TYPE direction = DIRECTION_TYPE.STOP;

    Rigidbody2D rigidbody2D;
    float speed;

    Animator animator;
    //SE
    [SerializeField] AudioClip getItemSE;
    [SerializeField] AudioClip jumpSE;
    [SerializeField] AudioClip stampSE;
    AudioSource audioSource;

    float jumpPower = 350;
    bool isDead = false;

    private void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }
    private void Update()
    {
        if(isDead)
        {
            return;
            naichilab.RankingLoader.Instance.SendScoreAndShowRanking(200);
        }
        float x = Input.GetAxis("Horizontal"); //方向キーの取得
        animator.SetFloat("speed",Mathf.Abs(x));
        if(x == 0)
        {
            //止まっている
            direction = DIRECTION_TYPE.STOP;
        }
        else if (x > 0)
        {
            //右へ
            direction = DIRECTION_TYPE.RIGHT;
        }
        else if(x < 0)
        {
            //左へ
            direction = DIRECTION_TYPE.LEFT;
        }
        // スペースが”一回”押されたらJumpさせる
        if(IsGround())
        {
            if(Input.GetKeyDown("space"))
            {
                Jump();
            }
            else
            {
                animator.SetBool("isjumping" , false);
            }
        }
   }

   private void FixedUpdate() //FIxedUpdateとは：定期的に決まった感覚で呼ばれるもの
   {
    if(isDead)
        {
            return;
            naichilab.RankingLoader.Instance.SendScoreAndShowRanking(200);
        }
    switch (direction)
    {
        case DIRECTION_TYPE.STOP:
        speed = 0;
        break;
        case DIRECTION_TYPE.RIGHT:
        speed = 3;
        transform.localScale = new Vector3(1,1,1);
        break;
        case DIRECTION_TYPE.LEFT:
        speed = -3;
        transform.localScale = new Vector3(-1,1,1);
        break;
    }
    rigidbody2D.velocity = new Vector2(speed,rigidbody2D.velocity.y);
   }

   void Jump()
   {
    //上に力を加える
    rigidbody2D.AddForce(Vector2.up * jumpPower);
    audioSource.PlayOneShot(jumpSE);
    animator.SetBool("isjumping" , true);
   }

   bool IsGround()
   {
    //始点と終点を作成
    Vector3 leftStartPoint = transform.position - transform.right * 0.3f + Vector3.up * 0.1f;
    Vector3 rightStartPoint = transform.position + transform.right * 0.3f + Vector3.up * 0.1f;
    Vector3 endPoint = transform.position - Vector3.up * 0.1f;
    Debug.DrawLine(leftStartPoint,endPoint);
    Debug.DrawLine(rightStartPoint,endPoint);
    return Physics2D.Linecast(leftStartPoint,endPoint,blockLayer)
        || Physics2D.Linecast(rightStartPoint,endPoint,blockLayer);
   }

   void OnTriggerEnter2D(Collider2D collision)
   {
    if(isDead)
    {
        return;
        naichilab.RankingLoader.Instance.SendScoreAndShowRanking(200);
    }

    if(collision.gameObject.tag == "Trap")
    {
        PlayerDeath();
    }

    if(collision.gameObject.tag == "Finish")
    {
        gameManager.GameClear();
    }

    if(collision.gameObject.tag == "Item")
    {
        //アイテム取得
        audioSource.PlayOneShot(getItemSE);
        collision.gameObject.GetComponent<ItemManager>().GetItem();
    }

    if(collision.gameObject.tag == "Enemy")
    {
        EnemyManager enemy = collision.gameObject.GetComponent<EnemyManager>();
        if(this.transform.position.y + 0.2f  > enemy.transform.position.y)
        {
            //上から踏んだら
            rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x,0);
            Jump();
            audioSource.PlayOneShot(stampSE);
            enemy.DestroyEnemy();
            //敵を削除
        }
        else
        {
        //横からぶつかったら
        PlayerDeath();
        }
    }
   }
   void PlayerDeath()
   {
    isDead = true;
    rigidbody2D.velocity = new Vector2(0,0);
    rigidbody2D.AddForce(Vector2.up * jumpPower);
    animator.Play("PlayerDeathAnimation");
    BoxCollider2D boxCollider2D = GetComponent<BoxCollider2D>();
    Destroy(boxCollider2D);
    gameManager.GameOver();
    naichilab.RankingLoader.Instance.SendScoreAndShowRanking(200);
   }
}
