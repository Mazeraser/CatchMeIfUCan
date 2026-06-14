using UnityEngine;
using UnityEngine.UI;

public class StartButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private GameManager gameManager;

    private void Awake() => button.onClick.AddListener(() => gameManager.StartGame());
}
public class RestartButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private GameManager gameManager;

    private void Awake() => button.onClick.AddListener(() => gameManager.RestartGame());
}