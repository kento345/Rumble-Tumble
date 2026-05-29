using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{

    

    public Slider bgmSlider;
    public Slider seSlider;

    public TMP_Text bgmText;
    public TMP_Text seText;

    void Start()
    {
        if (SoundManager.Instance != null)
        {
            // ★超安全対策：もしSoundManagerの初期値が0になってしまっていたら、コード上の初期値を強制注入する
            if (SoundManager.Instance.bgmVolume == 0f) SoundManager.Instance.SetBGMVolume(0.1f);
            if (SoundManager.Instance.seVolume == 0f) SoundManager.Instance.SetSEVolume(0.4f);

            // 最初に現在の音量をスライダーとテキストに反映
            bgmSlider.value = SoundManager.Instance.bgmVolume;
            seSlider.value = SoundManager.Instance.seVolume;

            UpdateBgmtext(bgmSlider.value);
            UpdateSetext(seSlider.value);

            // スライダーを動かした時の処理を登録（ここからは手動操作のみ受け付ける）
            bgmSlider.onValueChanged.AddListener(val =>
            {
                SoundManager.Instance.SetBGMVolume(val);
                UpdateBgmtext(val);
            });

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
            bgmText.text = Mathf.RoundToInt(value * 100).ToString();
        }
    }

    void UpdateSetext(float value)
    {
        if (seText != null)
        {
            seText.text = Mathf.RoundToInt(value * 100).ToString();
        }
    }
}
