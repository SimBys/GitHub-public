using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    [SerializeField] CrossSceneRef crossSceneReference;
    [Header("revive time fade")]
    [SerializeField] Image reviveTimeImage;
    [SerializeField] Image tryAgainButtonImage;
    [SerializeField] Image backButtonAfterDeathImage;
    [SerializeField] TMP_Text TMP_endScore;
    [SerializeField] TMP_Text TMP_highscore;
    [SerializeField] TMP_Text TMP_endCoins;
    [Header("In-Game UI")]
    [SerializeField] TMP_Text TMP_enemiesToSpawn;
    [SerializeField] TMP_Text TMP_enemiesKilled;
    [SerializeField] TMP_Text TMP_coins;
    [Header("Others")]
    [SerializeField] EdgeCollider2D arenaCollider2D;
    [SerializeField] LevelSO[] SOlevels;
    [SerializeField] LevelSO level;

    int coins;
    int score;
    Enemy[] spawnedEnemies;
    float arenaBorderSemidiameter;
    Coroutine enemySpawningCoroutine;
    bool gamePaused, revived;
    int enemiesToSpawn, enemiesKilled; // for UI
    Data data;

    void Debug()
    {
        Data data = new Data();
        SaveLoad.Save(data);
        print("clean save");
    }
    private void Awake()
    {
        Instance = this;
        level = SOlevels[crossSceneReference.selectedLevel - 1];
        enemiesToSpawn = level.enemiesToSpawn;
        TMP_enemiesToSpawn.text = enemiesToSpawn.ToString();
    }
    private void Start()
    {
        arenaBorderSemidiameter = arenaCollider2D.points[0].y *
            arenaCollider2D.gameObject.transform.localScale.x - 0.5f;
        Player.Instance.GameStart();
        enemySpawningCoroutine = StartCoroutine(EnemySpawner());
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Semicolon))
            Debug();
        if (Input.GetKeyDown(KeyCode.Escape))
            PauseGame();
    }
    public IEnumerator EnemySpawner()
    {
        if (revived)
            enemiesToSpawn = level.enemiesToSpawn - spawnedEnemies.Length;
        float randomX, randomY;
        Vector2 playerPos;
        float enemyFreeZone = Player.Instance.enemyFreeZone;
        yield return new WaitForSeconds(2); // initial delay
        for (int i = 0; i < enemiesToSpawn;)
        {
            GameObject enemy = ObjectPool.Instance.GetEnemy(0);
            randomX = Random.Range(-arenaBorderSemidiameter, arenaBorderSemidiameter);
            randomY = Random.Range(-arenaBorderSemidiameter, arenaBorderSemidiameter);
            // check if the spawn is in enemyFreeZone and if yes, then change it
            playerPos = Player.Instance.transform.position;
            if (randomX < playerPos.x + enemyFreeZone && randomY < playerPos.y + enemyFreeZone &&
                randomX > playerPos.x - enemyFreeZone && randomY > playerPos.y - enemyFreeZone)
            {
                if (playerPos.x >= 0)
                    randomX -= enemyFreeZone;
                else
                    randomX += enemyFreeZone;
                if (playerPos.y >= 0)
                    randomY -= enemyFreeZone;
                else
                    randomY += enemyFreeZone;
            }
            enemy.transform.position = new Vector2(randomX, randomY);
            enemy.SetActive(true);
            enemiesToSpawn--;
            TMP_enemiesToSpawn.text = enemiesToSpawn.ToString();
            yield return new WaitForSeconds(level.spawnRate);
        }
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
    public void OnEnemyDeath()
    {
        enemiesKilled++;
        Player.Instance.AddAmmo(5);
        TMP_enemiesKilled.text = enemiesKilled.ToString();
    }
    public void OnPlayerDeath()
    {
        Player.Instance.DisableLaser();
        data = SaveLoad.Load();
        TMP_endCoins.text = coins.ToString();
        if (score > data.highscore)
        {
            data.highscore = score;
            SaveLoad.Save(data);
        }
        TMP_highscore.text = "HIGHSCORE: " + data.highscore.ToString();
        TMP_endScore.text = "SCORE: " + score.ToString();
        TMP_endCoins.text = "COINS: " + coins.ToString();
        StopCoroutine(enemySpawningCoroutine);
        spawnedEnemies = ObjectPool.Instance.GetAllEnemies();
        for (int i = 0; i < spawnedEnemies.Length; i++)
        {
            spawnedEnemies[i].move = false;
        }
        MenusManager.Instance.LoadMenu(2);
        if (!revived)
            StartCoroutine(ReviveTimerCountdown());
        else
            reviveTimeImage.transform.parent.gameObject.SetActive(false);
    }
    public IEnumerator ReviveTimerCountdown()
    {
        float fillAmount = 1;
        Color newColor;
        Color newBlueColor;
        Color startColor = new Color(1, 1, 1, 0);
        Color endColor = Color.white;
        Color endBlueColor = new Color(0, 1, 1, 1); ;
        backButtonAfterDeathImage.color = startColor;
        tryAgainButtonImage.color = startColor;
        TMP_endScore.color = startColor;
        TMP_highscore.color = startColor;
        TMP_endCoins.color = startColor;
        backButtonAfterDeathImage.gameObject.GetComponent<Button>().interactable = false;
        tryAgainButtonImage.gameObject.GetComponent<Button>().interactable = false;
        while (fillAmount > 0)
        {
            reviveTimeImage.fillAmount = fillAmount;
            if (reviveTimeImage.fillAmount < 0.2f)
            {
                newColor = Color.Lerp(endColor, startColor, reviveTimeImage.fillAmount * 5);
                newBlueColor = Color.Lerp(endBlueColor, startColor, reviveTimeImage.fillAmount * 5);
                backButtonAfterDeathImage.color = newColor;
                tryAgainButtonImage.color = newColor;
                TMP_endScore.color = newBlueColor;
                TMP_highscore.color = newBlueColor;
                TMP_endCoins.color = newBlueColor;
            }
            fillAmount -= Time.deltaTime * 0.4f;
            yield return null;
        }
        backButtonAfterDeathImage.gameObject.GetComponent<Button>().interactable = true;
        tryAgainButtonImage.gameObject.GetComponent<Button>().interactable = true;
        reviveTimeImage.gameObject.SetActive(false);
    }
    public void ReviveButton()
    {
        AdsManager.Instance.Revive();
    }
    public void Revive()
    {
        revived = true;
        Player.Instance.Revive();
        MenusManager.Instance.LoadMenu(0);
        for (int i = 0; i < spawnedEnemies.Length; i++)
        {
            spawnedEnemies[i].move = true;
        }
        enemySpawningCoroutine = StartCoroutine(EnemySpawner());
    }
    public void RestartGame()
    {
        SaveCoins();
        foreach (var enemy in ObjectPool.Instance.GetAllEnemies())
        {
            enemy.gameObject.SetActive(false);
        }
        Player.Instance.GameStart();
        Player.Instance.transform.position = Vector2.zero;
        Player.Instance.SetOpacity(1);
        MenusManager.Instance.LoadMenu(0);
        coins = 0;
        score = 0;
        revived = false;
        enemiesToSpawn = level.enemiesToSpawn;
        TMP_enemiesToSpawn.text = enemiesToSpawn.ToString();
        TMP_enemiesKilled.text = 0.ToString();
        enemySpawningCoroutine = StartCoroutine(EnemySpawner());
    }
    public void GameEndBackButton()
    {
        SaveCoins();
        SceneManager.LoadScene(0);
    }
    public void SaveCoins()
    {
        Data data = SaveLoad.Load();
        data.coins += coins;
        SaveLoad.Save(data);
        print(data.coins + " coins saved!");
    }
    public void CoinCollect(int coinValue)
    {
        coins += coinValue;
        TMP_coins.text = coins.ToString();
    }
    public void Quit()
    {
        Application.Quit();
    }
}