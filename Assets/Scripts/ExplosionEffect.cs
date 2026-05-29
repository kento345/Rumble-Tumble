using UnityEngine;
using System.Collections;

public class ExplosionEffect : MonoBehaviour
{
    public GameObject Explosive;

    [Header("Light Settings")]
    public Light explosionLight;
    public float peakIntensity = 8f;      // 閃光のピーク強度
    public float peakRange = 15f;         // 光の最大範囲
    public float flashDuration = 0.05f;   // 閃光の長さ（短くするほどシャープ）
    public float fadeDuration = 0.6f;     // 減衰時間

    [Header("Color")]
    public Color flashColor = new Color(1f, 0.9f, 0.5f);  // 白に近い黄色
    public Color emberColor = new Color(1f, 0.3f, 0.05f); // 燃え残りのオレンジ

    [Header("Particle Systems")]
    public ParticleSystem flashPS;
    public ParticleSystem firePS;
    public ParticleSystem smokePS;
    public ParticleSystem debrisPS;

    void Start()
    {
        StartCoroutine(ExplodeSequence());
    }

    IEnumerator ExplodeSequence()
    {
        // --- 1. 閃光フェーズ ---
        explosionLight.color = flashColor;
        explosionLight.intensity = peakIntensity;
        explosionLight.range = peakRange;

        flashPS?.Play();
        firePS?.Play();
        debrisPS?.Play();

        // 閃光を維持
        yield return new WaitForSeconds(flashDuration);

        // --- 2. 減衰フェーズ ---
        smokePS?.Play(); // 煙は少し遅れて

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            float curve = 1f - Mathf.Pow(t, 0.5f); // 急速に落ちて残る曲線

            explosionLight.intensity = peakIntensity * curve * 0.4f;
            explosionLight.range = peakRange * (0.5f + 0.5f * curve);

            // 色を閃光色 → 燠火色に遷移
            explosionLight.color = Color.Lerp(emberColor, flashColor, curve);

            yield return null;
        }

        // --- 3. 終了 ---
        explosionLight.intensity = 0f;

        // パーティクルが終わったらオブジェクトを破棄
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
        Destroy(Explosive);
    }
}