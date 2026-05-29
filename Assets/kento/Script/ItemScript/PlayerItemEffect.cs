using System.Collections;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class PlayerItemEffect : MonoBehaviour
{
    [SerializeField] private DecalProjector circle;
    //[SerializeField] private DecalProjector arraw;
    [SerializeField] private GameObject arraw;

    Vector3 defaultScale;
    float defaultSpeed;
    float defaultWeakKnockbackForce;
    float defaultStrongKnockbackForce;
    Vector3 defaltCircleSize;
    Vector3 defaltArrawSize;
    Vector3 circleSize;
    Vector3 arrawSize;
    Image paint;
        Text text;

    

    public bool isEffectActive { get; private set; } = false;

    AtackController ac;
    MoveController mc;
    Reception reception;

    private void OnEnable()
    {
        GameObject obj = GameObject.Find("PaintImage");
        paint = obj.GetComponent<Image>();
        GameObject a = GameObject.Find("ItemTxt");
        text = a.GetComponent<Text>();
        PlayerItemEffect[] players = FindObjectsByType<PlayerItemEffect>(FindObjectsSortMode.None);
        foreach (var p in players)
        {
            if (p == this) continue;
            var playerInput = p.GetComponent<PlayerInputController>();
            if (playerInput != null)
            {
                playerInput.SetReverse(false);
            }
        }

        paint.enabled = false;
        text.enabled = false;
    }

    private void Start()
    {
        ac = GetComponent<AtackController>();
        mc = GetComponent<MoveController>();
        reception = GetComponent<Reception>();

        defaultScale = transform.localScale;
        defaultWeakKnockbackForce = ac.WeakKnockbackForce;
        defaultSpeed = mc.Speed;
        defaultStrongKnockbackForce = ac.StrongKnockbackForce;
        defaltCircleSize = circle.size;
        defaltArrawSize = arraw.transform.localScale;
       
    }

    public void ApplyItem(Item item)
    {
        switch (item.type)
        {
            case Item.Type.RandomBox:
                RandomEfect(item);
                break;
            case Item.Type.ReverseBox:
                StartCoroutine(ReverseEfect(item));
                break;
            case Item.Type.BigBox:
                StartCoroutine(BigEfect(item));
                break;
            case Item.Type.PaintBox:
                StartCoroutine(PaintEfect(item));
                break;
        }
    }

    void RandomEfect(Item item)
    {
        int r = Random.Range(0, 3);

        if (r == 0)
        {
            StartCoroutine(BigEfect(item));
        }
        else if (r == 1)
        {
            StartCoroutine(ReverseEfect(item));
        }
        else if (r == 2)
        {
            StartCoroutine(PaintEfect(item));
        }
    }
    IEnumerator BigEfect(Item item)
    {
        transform.localScale = defaultScale * item.effectValue;
        DecalScale(0, item);
        isEffectActive = true;
        yield return new WaitForSeconds(item.duration);
        transform.localScale = defaultScale;
        DecalScale(1, item);
        isEffectActive = false;
    }

    /*IEnumerator SmallEfect(Item item)
    {
        transform.localScale = defaultScale / item.effectValue;
        DecalScale(2, item);
        mc.Speed = defaultSpeed * item.effectValue;
        reception.smallKnockback = 20;
        isEffectActive = true;
        yield return new WaitForSeconds(item.duration);
        transform.position += new Vector3(0,1,0);
        transform.localScale = defaultScale;
        DecalScale(1, item);
        mc.Speed = defaultSpeed;
        reception.smallKnockback = 1;
        isEffectActive = false;
    }*/

    IEnumerator ReverseEfect(Item item)
    {
        isEffectActive = true;

        PlayerItemEffect[] players = FindObjectsByType<PlayerItemEffect>(FindObjectsSortMode.None);
        text.enabled = true;

        foreach (var p in players)
        {
            if (p == this) continue;

            var playerInput = p.GetComponent<PlayerInputController>();
            if (playerInput != null)
            {
                playerInput.SetReverse(true);
            }

            /* var bot = p.GetComponent<BOTController>();
             if (bot != null)
             {
                 bot.SetReverse(true);
             }*/
        }
        yield return new WaitForSeconds(item.duration);

        foreach (var p in players)
        {
            if (p == this) continue;

            var playerInput = p.GetComponent<PlayerInputController>();
            if (playerInput != null)
            {
                playerInput.SetReverse(false);
            }

            /*  var bot = p.GetComponent<BOTController>();
              if (bot != null)
              {
                  bot.SetReverse(false);
              }*/
        }
        text.enabled = false;
        isEffectActive = false;
    }
    IEnumerator PaintEfect(Item item)
    {
        isEffectActive = true;
        paint.enabled = true;
        yield return new WaitForSeconds(item.duration);
        paint.enabled = false;
        isEffectActive = false ;
    }
    void DecalScale(int i, Item item)
    {
        if (i == 0)
        {
            //デカール拡大;
            circleSize = defaltCircleSize * item.effectValue;
            circleSize.z = defaltCircleSize.z;
            circle.size = circleSize;
            arrawSize = defaltArrawSize * item.effectValue;
            arrawSize.z = defaltArrawSize.z;
            arraw.transform.localScale = arrawSize;
            //攻撃力アップ
            ac.WeakKnockbackForce = defaultWeakKnockbackForce * item.effectValue;
            ac.StrongKnockbackForce = defaultStrongKnockbackForce * item.effectValue;
        }
        else if (i == 1)
        {
            //デカールノーマル
            circle.size = defaltCircleSize;
            arraw.transform.localScale = defaltArrawSize;
            //攻撃力ノーマル
            ac.WeakKnockbackForce = defaultWeakKnockbackForce;
            ac.StrongKnockbackForce = defaultStrongKnockbackForce;
        }
        else
        {
            //デカール縮小
            circleSize = defaltCircleSize / item.effectValue;
            circleSize.z = defaltCircleSize.z;
            circle.size = circleSize;
            arrawSize = defaltArrawSize / item.effectValue;
            arrawSize.z = defaltArrawSize.z;
            arraw.transform.localScale = arrawSize;
            //攻撃力ダウン
            ac.WeakKnockbackForce = defaultWeakKnockbackForce / item.effectValue;
            ac.StrongKnockbackForce = defaultStrongKnockbackForce / item.effectValue;
        }
    }
}
