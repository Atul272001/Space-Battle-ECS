using System.Collections;
using Unity.Entities;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    public static UIHandler Instance;


    [SerializeField] Slider playerHealth;
    [SerializeField] GameObject StartGamePanel;
    [SerializeField] GameObject GameOverPanel;
    [SerializeField] GameObject GamePausePanel;

    // Start is called before the first frame update
    void Awake()
    {
        playerHealth.value = 1000;
        playerHealth.maxValue = 1000;
        StopGame(true);
        StartGamePanel.SetActive(true);
        GamePausePanel.SetActive(false);
        GameOverPanel.SetActive(false);
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartGame()
    {
        
        StopGame(false);
        StartGamePanel.SetActive(false);
    }

    public void UpdatePlayerHealth(float health)
    {
        playerHealth.value = health;
    }

    public void PauseGame()
    {
        GamePausePanel.SetActive(true);
        StopGame(true);
    }

    public void ResumeGame()
    {
        GamePausePanel.SetActive(false);
        StopGame(false);
    }

    public void StopGame(bool IsPaued)
    {
        Cursor.visible = IsPaued;
        var world = World.DefaultGameObjectInjectionWorld;
        var sim = world.GetExistingSystemManaged<SimulationSystemGroup>();
        var fixedSim = world.GetExistingSystemManaged<FixedStepSimulationSystemGroup>();
        if (sim != null) sim.Enabled = !IsPaued;
        if (fixedSim != null) fixedSim.Enabled = !IsPaued;
    }

    public void GameOverPannel()
    {
        StopGame(true);
        GameOverPanel.SetActive(true);
    }

    public void Retry()
    {
        GameOverPanel.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
