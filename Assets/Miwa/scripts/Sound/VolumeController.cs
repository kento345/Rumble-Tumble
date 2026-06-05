using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    [Header("タイトルシーンのスライダーとテキストを直接アタッチしてください")]
    public Slider bgmSlider;
    public Slider seSlider;

    public TMP_Text bgmText;
    public TMP_Text seText;

    void Start()
    {
        if (SoundManager.Instance != null)
        {
            // 1. 【超重要】まずはイベントを完全にクリア（空っぽ）にする
            if (bgmSlider != null) bgmSlider.onValueChanged.RemoveAllListeners();
            if (seSlider != null) seSlider.onValueChanged.RemoveAllListeners();

            // 2. イベントが空っぽ（何をしても暴発しない状態）の時に、本物の音量を安全に代入する
            if (bgmSlider != null) bgmSlider.value = SoundManager.Instance.bgmVolume * 100f;
            if (seSlider != null) seSlider.value = SoundManager.Instance.seVolume * 100f;

            // 3. テキスト表示も本物の数値に合わせる
            UpdateBgmtext(SoundManager.Instance.bgmVolume);
            UpdateSetext(SoundManager.Instance.seVolume);

            // 4. 【安全確認】代入がすべて終わった「後」で、初めてプレイヤーが動かした時のイベントを登録する
            if (bgmSlider != null)
            {
                bgmSlider.onValueChanged.AddListener(val =>
                {
                    SoundManager.Instance.SetBGMVolume(val / 100f);
                    UpdateBgmtext(val / 100f);
                });
            }

            if (seSlider != null)
            {
                seSlider.onValueChanged.AddListener(val =>
                {
                    SoundManager.Instance.SetSEVolume(val / 100f);
                    UpdateSetext(val / 100f);
                });
            }
        }
    }

    void UpdateBgmtext(float value)
    {
        if (bgmText != null)
        {
            bgmText.text = Mathf.RoundToInt(value * 100).ToString() + "%";
        }
    }

    void UpdateSetext(float value)
    {
        if (seText != null)
        {
            seText.text = Mathf.RoundToInt(value * 100).ToString() + "%";
        }
    }
}