using UnityEngine;
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    private int[] _scores = new int[4];
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }
    public void AddScore(int index, int amount)
    {
        if (index >= 0 && index < _scores.Length) _scores[index] += amount;
    }
    public void RemoveScore(int index, int amount)
    {
        if (index >= 0 && index < _scores.Length)
            _scores[index] = Mathf.Max(0, _scores[index] - amount);
    }
    public int GetScore(int index)
    {
        return (index >= 0 && index < _scores.Length) ? _scores[index] : 0;
    }

    public void ResetAllScores()
    {
        for (int i = 0; i < _scores.Length; i++)
        {
            _scores[i] = 0;
        }
    }
}