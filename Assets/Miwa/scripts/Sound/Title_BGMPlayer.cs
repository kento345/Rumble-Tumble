using UnityEngine;

public class SceneBGMPlayer : MonoBehaviour
{
    [SerializeField] private bool playOnStart = true;

    void Start()
    {
        
        Time.timeScale = 1.0f;
        AudioListener.pause = false;

        if (playOnStart && SoundManager.Instance != null)
        {
            
            SoundManager.Instance.StopBGM();

            SoundManager.Instance.PlayBGM(SoundManager.Instance.MainBGM);
        }
    }
}
