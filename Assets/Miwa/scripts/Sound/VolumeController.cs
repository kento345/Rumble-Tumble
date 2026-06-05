using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    [Header("タイトルシーンのスライダーとテキストをインスペクターから設定してください")]
    public Slider bgmSlider;
    public Slider seSlider;

    public TMP_Text bgmText;
    public TMP_Text seText;

    void Start()
    {
        // タイトルシーンが読み込まれた（または設定画面が開いた）瞬間に毎回走る
        if (SoundManager.Instance != null)
        {
            // 1. ずっと生き残っている SoundManager から現在の音量を取得して、新しいスライダーに同期
            if (bgmSlider != null) bgmSlider.value = SoundManager.Instance.bgmVolume;
            if (seSlider != null) seSlider.value = SoundManager.Instance.seVolume;

            // 2. テキスト表示も現在の音量に合わせる
            UpdateBgmtext(SoundManager.Instance.bgmVolume);
            UpdateSetext(SoundManager.Instance.seVolume);

            // 3. 一度イベントを掃除して、今画面に見えているスライダーに処理を結び直す
            if (bgmSlider != null)
            {
                bgmSlider.onValueChanged.RemoveAllListeners();
                bgmSlider.onValueChanged.AddListener(val =>
                {
                    SoundManager.Instance.SetBGMVolume(val);
                    UpdateBgmtext(val);
                });
            }

            if (seSlider != null)
            {
                seSlider.onValueChanged.RemoveAllListeners();
                seSlider.onValueChanged.AddListener(val =>
                {
                    SoundManager.Instance.SetSEVolume(val);
                    UpdateSetext(val);
                });
            }

            Debug.Log("<color=green>[VolumeController] 新しいタイトルUIとSoundManagerの同期に成功しました！</color>");
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