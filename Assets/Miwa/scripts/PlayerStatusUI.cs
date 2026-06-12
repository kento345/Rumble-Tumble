using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerStatusUI : MonoBehaviour
{
    [Header("背景のImageコンポーネント")]
    public Image backgroundPanel; 

    private Sprite myAliveSprite;
    private Sprite myDeadSprite;

    [Header("星の画像たち")]
    public Image[] stars; 
    [Header("星の色設定")]
    public Color starOnColor = Color.yellow; 
    public Color starOffColor = Color.gray;

    [Header("スコア表示用")]
    public Text scoreText;

    //画面揺らし用の変数
    private RectTransform rectTransform;
    private Vector3 originalLocalPosition;
    private Coroutine shakeCoroutine;



    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            originalLocalPosition = rectTransform.localPosition;
        }
    }



    public void SetupUI(int initialValue, Sprite alive, Sprite dead,bool isScoreMode)
    {
        myAliveSprite = alive;
        myDeadSprite = dead;


        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }
        if (rectTransform != null)
        {
            rectTransform.localPosition = originalLocalPosition;
        }

        if (backgroundPanel != null && myAliveSprite != null)
        {
            backgroundPanel.sprite = myAliveSprite;
            backgroundPanel.color = Color.white; 
        }
        if (stars != null)
        {
            foreach(var star in stars)
            {
                if(star != null) star.gameObject.SetActive(!isScoreMode);
            }
        }
        if(scoreText  != null)
        {
            scoreText.gameObject.SetActive(isScoreMode);
        }
        if(isScoreMode)
        {
            UpdateScore(initialValue);
        }
        else
        {
            UpdateStars(initialValue);
        }
    }

    public void UpdateStars(int score)
    {
        // ここが null だと色を変えられない
        if (stars == null || stars.Length == 0)
        {
            Debug.LogWarning("UIの星(stars)がセットされていません！");
            return;
        }

        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] != null)
            {
                // i=0(1つ目)の星は、勝利数(score)が1以上の時に色が付く
                stars[i].color = (i < score) ? starOnColor : starOffColor;
            }
        }
    }
    public void UpdateScore(int score)
    {
        if(scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }

    public void SetEliminated(bool isDead)
    {
        backgroundPanel.sprite = isDead ? myDeadSprite : myAliveSprite;

        if (isDead)
        {
            StartShake(0.4f, 15f); // 画面揺らすで～
        }
    }

    //画面揺らしのコルーチン
    public void StartShake(float duration, float magnitude)
    {
        if (rectTransform == null) return;

        //一旦止めて二重書き込みを防ぐ
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);

        shakeCoroutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }


    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            rectTransform.localPosition = new Vector3(originalLocalPosition.x + x, originalLocalPosition.y + y, originalLocalPosition.z);

            yield return null;
        }
        rectTransform.localPosition = originalLocalPosition;
        shakeCoroutine = null;
    }
}