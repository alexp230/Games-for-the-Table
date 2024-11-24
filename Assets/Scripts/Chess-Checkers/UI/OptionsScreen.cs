using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsScreen : MonoBehaviour
{
    [SerializeField] private Toggle ShowValidMovesToggle;

    void OnEnable()
    {
        BoardMaterials.ShowValidMoves = ShowValidMovesToggle.isOn;
        BoardMaterials.IsPaused = true;
    }

    void OnDisable()
    {
        BoardMaterials.ShowValidMoves = ShowValidMovesToggle.isOn;
        BoardMaterials.IsPaused = false;
    }

    public void OnResignButton()
    {
        NetworkManager.Singleton?.Shutdown();
        SceneManager.LoadScene(sceneName:"MainMenu");
    }
}
