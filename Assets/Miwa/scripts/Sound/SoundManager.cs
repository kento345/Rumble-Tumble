using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("オーディオソース")]
    public AudioSource bgmSource; // BGM用(LoopONにするであります!)
    public AudioSource seSource;  // SE用(LoopOFFにするであります!)
    //とりあえずテキストに合わせて変数を作って奥であります！

    [Range(0f, 1f)] public float bgmVolume = 0.1f;
    [Range(0f, 1f)] public float seVolume = 0.4f;

    [Header("■ 参加画面")]
    public AudioClip titleBGM;       
    public AudioClip playerJoinSE;   // 決定ボタンを押す14
    public AudioClip gameStartBtnSE; // 決定15

    [Header("■ ゲーム画面：BGM")]
    public AudioClip normalBattleBGM; // Threat_of_Impulse
    public AudioClip suddenDeathBGM;  // maou_bgm_neorock66

    [Header("■ ゲーム画面：SE")]
    public AudioClip gameStartGongSE; // 試合開始のゴング
    public AudioClip gameEndGongSE;   // 試合終了のゴング
    
    [Header("■ アクションSE（攻撃・隕石など）")]
    public AudioClip weakHitSE;       // 軽いパンチ１
    public AudioClip chargeSE;        // energycharge_kantai2
    public AudioClip dashStartSE;     // パンチの風切り音（スローモーション）1
    public AudioClip dashHitSE;       // 必殺技ヒット
    //public AudioClip meteorFallSE;    // ゴゴゴ 激しい地鳴り音
    //public AudioClip meteorImpactSE;  // 岩にヒビが入る
    public AudioClip groundBreakSE;   // 岩が真っ二つに割れる（地面が崩れる音）

    [Header("■ リザルト画面")]
    public AudioClip resultBGM;      // maou_game_jingle05
    public AudioClip cursorMoveSE;   // 決定ボタンを押す49
    public AudioClip decideSE;       // 決定13

    [Header("■ タイトル画面")]
    public AudioClip MainBGM;

    [Header("SE重なり防止設定")]
    [SerializeField] private float defaultSeInterval = 0.1f;
    private Dictionary<AudioClip, float> lastPlayTimes = new Dictionary<AudioClip, float>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            // ★【超重要】もしCanvasなどの下に配置されていたら、親から独立させる
            // これをしないと、DontDestroyOnLoadを指定してもシーン切り替え時に一緒に消されてしまいます
            transform.SetParent(null);

            DontDestroyOnLoad(gameObject);

            ApplyVolume();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void ApplyVolume()
    {
        if (bgmSource != null) bgmSource.volume = bgmVolume;
        if (seSource != null) seSource.volume = seVolume;
    }

    // BGM再生ロジック
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.volume = bgmVolume;
        bgmSource.Play();
    }

    //  SE再生 ロジック
    public void PlaySE(AudioClip clip)
    {
        if (clip == null) return;

        if (lastPlayTimes.ContainsKey(clip))
        {
            if (Time.time - lastPlayTimes[clip] < defaultSeInterval)
            {
                return;
            }
        }

        seSource.PlayOneShot(clip, seVolume);
        lastPlayTimes[clip] = Time.time;
    }

    public void PlaySE(AudioClip clip, float customInterval)
    {
        if (clip == null) return;

        if (lastPlayTimes.ContainsKey(clip))
        {
            if (Time.time - lastPlayTimes[clip] < customInterval) return;
        }

        seSource.PlayOneShot(clip, seVolume);
        lastPlayTimes[clip] = Time.time;
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = volume;
        if(bgmSource !=null)bgmSource.volume =bgmVolume;
    }

    public void SetSEVolume(float volume)
    {
        seVolume = volume;
        if(seSource != null) seSource.volume = seVolume;
    }

    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
            bgmSource.clip = null;
        }
    }

}