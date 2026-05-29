using UnityEngine;
using UnityEngine.SceneManagement;

public class Loop : MonoBehaviour
{
    [SerializeField] private float loopInterval = 3f; // ループ間隔（秒）

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InvokeRepeating(nameof(LoopScene), loopInterval, loopInterval);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoopScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
