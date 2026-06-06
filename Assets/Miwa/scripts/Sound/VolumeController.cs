using TMPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    public Slider bgmSlider;
    public Slider seSlider;

    public TMP_Text bgmText;
    public TMP_Text seText;

    [Header("【音量の初期値設定】 0.0 ～ 1.0 の間で指定")]
    [SerializeField] private float defaultBGM = 0.3f;
    [SerializeField] private float defaultSE = 0.5f;

    void Start()
    {
        if (SoundManager.Instance != null)
        {
            // 初回起動時、またはデータが不正（0以下など）な場合に強制リセットして指定の初期値を流し込む
            if (!PlayerPrefs.HasKey("VolumeInitialized") || (SoundManager.Instance.bgmVolume <= 0f && SoundManager.Instance.seVolume <= 0f))
            {
                SoundManager.Instance.SetBGMVolume(defaultBGM);
                SoundManager.Instance.SetSEVolume(defaultSE);
                PlayerPrefs.SetInt("VolumeInitialized", 1);
                PlayerPrefs.Save();
            }

            // 現在 SoundManager が保持している「実際の音量」をスライダーに正確に同期
            bgmSlider.value = SoundManager.Instance.bgmVolume;
            seSlider.value = SoundManager.Instance.seVolume;

            // テキスト表示も現在の音量に同期
            UpdateBgmtext(bgmSlider.value);
            UpdateSetext(seSlider.value);

            // スライダーを動かした時のイベント登録（重複登録防止）
            bgmSlider.onValueChanged.RemoveAllListeners();
            bgmSlider.onValueChanged.AddListener(val =>
            {
                SoundManager.Instance.SetBGMVolume(val);
                UpdateBgmtext(val);
            });

            seSlider.onValueChanged.RemoveAllListeners();
            seSlider.onValueChanged.AddListener(val =>
            {
                SoundManager.Instance.SetSEVolume(val);
                UpdateSetext(val);
            });
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