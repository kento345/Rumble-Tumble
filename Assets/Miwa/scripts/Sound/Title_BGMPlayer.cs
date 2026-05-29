using UnityEngine;

public class SceneBGMPlayer : MonoBehaviour
{
    [SerializeField] private bool playOnStart = true;

    void Start()
    {
        if (playOnStart && SoundManager.Instance != null)
        {
            // TitleBGMなど、鳴らしたいクリップを指定
            SoundManager.Instance.PlayBGM(SoundManager.Instance.MainBGM);
        }
    }
}
