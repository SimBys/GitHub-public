using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameController : MonoBehaviour
{
    public static GameController Instance;
    public static int score;
    public static int coins;

    [SerializeField] Slider timeScaleSlider;
    [SerializeField] Image reviveTimeImage;
    [SerializeField] float reviveTime;
    [Header("GameEndBackButton")]
    [SerializeField] GameObject backButtonAfterDeath;
    [SerializeField] Image backButtonAfterDeathImage;

    Coroutine enemySpawningCoroutine;
    bool gamePaused;

    private void Awake() // singleton
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);
    }
    private void Start()
    {
        StartGame();        
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            PauseGame();
    }
    public void StartGame()
    {
        Player.Instance.GameStart();
        enemySpawningCoroutine = StartCoroutine(EnemySpawner(500, 0.5f));
    }
    public void SetTimeScale()
    {
        Time.timeScale = timeScaleSlider.value;
    }
    void PauseGame()
    {
        gamePaused = !gamePaused;
        if (gamePaused)
        {
            Time.timeScale = 0;
            MenusManager.Instance.LoadMenu(1);
            return;
        }
        Time.timeScale = 1;
        MenusManager.Instance.LoadMenu(0);
    }
    public IEnumerator EnemySpawner(int amount, float interval)
    {
        for (int i = 0; i < amount; i++)
        {
            int random = Random.Range(0, 360);
            GameObject enemy = ObjectPool.Instance.GetEnemy(0);
            enemy.transform.position = new Vector2(Mathf.Cos(random), Mathf.Sin(random)) * Random.Range(2f, 5f);
            enemy.SetActive(true);
            yield return new WaitForSeconds(interval);
        }
    }
    public void OnPlayerDeath()
    {
        StopCoroutine(enemySpawningCoroutine);
        MenusManager.Instance.LoadMenu(2);
        StartCoroutine(ReviveTimerCountdown());
    }
    public void GameEndBackButton()
    {
        score = 0;
        MenusManager.Instance.LoadMenu(0);
        Time.timeScale = 1;
    }
    public IEnumerator ReviveTimerCountdown()
    {
        backButtonAfterDeath.SetActive(false);
        float i = reviveTime;
        Color endColor = backButtonAfterDeathImage.color;
        Color startColor = new Color(backButtonAfterDeathImage.color.r, backButtonAfterDeathImage.color.g, backButtonAfterDeathImage.color.b, 0);
        while (i > 0)
        {
            reviveTimeImage.fillAmount = i / reviveTime;
            if(reviveTimeImage.fillAmount < 0.2f)
            {
                backButtonAfterDeathImage.color = Color.Lerp(endColor, startColor, reviveTimeImage.fillAmount * 5);
                if (!backButtonAfterDeath.activeInHierarchy)
                    backButtonAfterDeath.SetActive(true);
            }
            i -= Time.deltaTime;
            yield return null;
        }
        reviveTimeImage.gameObject.SetActive(false);

    }
    public void Quit()
    {
        Application.Quit();
    }
    public void Save()
    {
        Data data = new Data();
        data.coins = coins;
        SaveLoad.Save(data);
        print("Data saved successfully!");
    }
}