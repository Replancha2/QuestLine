using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Stores game-over stats so they persist across the scene transition.
/// </summary>
public static class GameOverData
{
    public static int DayReached;
    public static int TotalFame;
}

/// <summary>
/// Listens for OnGameOver, saves data to GameOverData, then loads the GameOver scene.
/// </summary>
public class GameOverHandler : MonoBehaviour
{
    private void OnEnable()
    {
        EventBus.Subscribe<OnGameOver>(HandleGameOver);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<OnGameOver>(HandleGameOver);
    }

    private void HandleGameOver(OnGameOver e)
    {
        GameOverData.DayReached = e.DayReached;
        GameOverData.TotalFame  = e.TotalFameEarned;
        SceneManager.LoadScene("GameOver");
    }
}
