using UnityEngine;

[RequireComponent(typeof(CanvasGroup))] // CanvasGroupを必須にする
public class UniversalFade : MonoBehaviour
{
    [SerializeField] private float speed = 2.0f; // 速さ
    private CanvasGroup canvasGroup;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        // 0.0〜1.0の間を滑らかに往復（サイン波を使用）
        float alpha = (Mathf.Sin(Time.time * speed) + 1.0f) / 2.0f;

        // CanvasGroup全体のアルファ値を変更
        canvasGroup.alpha = alpha;
    }
}