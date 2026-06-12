using System.Collections;
using Unity.Entities.UniversalDelegates;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;



public class MainManager : MonoBehaviour
{
    [SerializeField] private GameObject botPrefab = default;

    [SerializeField] private Transform[] pos = default;         //生成位置
    [SerializeField] private GameObject timeUpPanel;

    private GameObject joinobj;
    int i = 0;

    IEnumerator Start()
    {
        yield return null; // 1フレーム待つ
        timeUpPanel.gameObject.SetActive(false);

        for (int i = 0; i < PlayerDataHolder.Instance.players.Count; i++)
        {
            var player = PlayerDataHolder.Instance.players[i];
            var helth = player.GetComponent<PlayerHealth>();

            if (helth != null)
            {
                helth.OnStart(player, PlayerDataHolder.Instance.ID[i]);
            }
        }
        while (i < pos.Length)
        {
            var bot = Instantiate(botPrefab, pos[i].position, pos[i].rotation);
            var health = bot.GetComponent<PlayerHealth>();
            health.OnStart(bot.gameObject, i);
            var botCon = bot.GetComponent<BOTController>();
            if (botCon != null)
            {
                botCon.enabled = false;
            }
            i++;
        }
    }

    void Awake()
    {
        // プレイヤー生成のロジックをここに記述
        joinobj = GameObject.Find("JoinedManager");
        if(PlayerDataHolder.Instance == null) {return; }
        var players = PlayerDataHolder.Instance.players;

        if(players == null) { return; }
        for (i = 0; i < players.Count; i++)
        {
            players[i].transform.position =
                pos[i].position;
            players[i].transform.rotation =
                pos[i].rotation;
        }
    }

    public void OnReset()
    {
        SceneManager.LoadScene("Start");
        Destroy(joinobj);
    }
}
