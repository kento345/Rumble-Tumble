using UnityEngine;
public class GameMode : MonoBehaviour
{

    public interface IGameMode
    {
        void OnEnter();
        void OnUpdate();
        void OnExit();
    }
}
