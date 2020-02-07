﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class gameManager : MonoBehaviour
{
    public bool mobileTesting = true;
    
    [Header("Game")]
    public int round = 1;
    public int transitionDuration;
    public int startTransitionDuration;
    public int effectDelayDuration;
    public int pointsOnHit;
    public int pointsOnDeath;
    public int pointsOnRebuild;
    public int points = 0;
    public GameObject pauseMenu;
    public GameObject gameOverMenu;
    private AdManager adManager;

    [Header("Audio")]
    public AudioClip backgroundMusic;
    public AudioClip roundSound;
    public AudioClip playerDefeatedSound;
    public AudioClip[] catMeowSounds;
    public AudioClip[] catHitSounds;
    public AudioClip doorOpenSound;
    public AudioClip snackPickupSound;
    public AudioClip balloonPickupSound;
    public AudioClip[] woodPlankSounds;
    public AudioClip[] balloonThrowSounds;

    private AudioSource musicAudioSource;
    private AudioSource sfxAudioSource;

    [Header("Cats")]
    public GameObject catPrefab;
    public List<GameObject> catSpawns;
    public int maxCatsOnMap;
    public float spawnDelayDuration;
    public int baseCatCount;
    public int baseCatHealth;
    public int difficulty;
    public float kittenSpeed;
    public int kittenCountMultiplier;
    public int kittenHealthMultiplier;
    public float catSpeed;
    public int catCountMultiplier;
    public int catHealthMultiplier;
    public float wildcatSpeed;
    public int wildcatCountMultiplier;
    public int wildcatHealthMultiplier;
    [HideInInspector] public float catMaxHealth;

    [Header("Balloons")]
    public int balloonPackCost;
    public int pickUpBalloonAmount;
    
    [Header("Snacks")]
    public List<GameObject> snacks;
    public int snackCost;
    public int snackAmount;

    [Header("Player")]
    public float playerMaxHealth;
    public int playerFullBalloonStartCount;
    public int playerEmptyBalloonStartCount;

    private bool transitioning = false;
    private float remainingTransitionTime;
    private int roundCatCount = 0;
    private int spawnedCats = 0;
    private float spawnDelay = 0;
    private Text roundTitle;
    private Text roundNumber;
    private Text pointsText;
    private GameObject pointsAddEffect;
    private GameObject pointsSubtractEffect;
    private float pointsAddEffectDelay = 0;
    private float pointsSubtractEffectDelay = 0;
    private GameObject infoTextObject;
    public bool isPaused = false;
    public bool gameOver = false;

    [Header("Stats")]
    public int roundsSurvived;
    public float surviveTime;
    public int catsSpawned;
    public int catsDefeated;
    public int beanBucksEarned;
    public int beanBucksSpent;
    public int waterBalloonsCollected;
    public int waterBalloonsThrown;
    public int waterBalloonsBought;
    public int snacksBought;
    public int planksKnockedOff;
    public int planksPutUp;
    public Text[] endGameScoreTexts;


    // Start is called before the first frame update
    void Start()
    {
        difficulty = PlayerPrefs.GetInt("Difficulty");

        roundTitle = GameObject.FindGameObjectWithTag("roundTitle").GetComponent<UnityEngine.UI.Text>();
        roundNumber = GameObject.FindGameObjectWithTag("roundNumber").GetComponent<UnityEngine.UI.Text>();
        pointsText = GameObject.FindGameObjectWithTag("pointsText").GetComponent<UnityEngine.UI.Text>();
        pointsAddEffect = GameObject.FindGameObjectWithTag("pointsAddEffect");
        pointsSubtractEffect = GameObject.FindGameObjectWithTag("pointsSubtractEffect");
        infoTextObject = GameObject.FindGameObjectWithTag("infoTextObject");

        musicAudioSource = GameObject.FindGameObjectWithTag("musicAudioSource").GetComponent<AudioSource>();
        sfxAudioSource = GameObject.FindGameObjectWithTag("sfxAudioSource").GetComponent<AudioSource>();

        adManager = GameObject.FindGameObjectWithTag("adDatabase").GetComponent<AdManager>();

        pointsAddEffect.SetActive(false);
        pointsSubtractEffect.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        bool inPlay = StillInPlay();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) {
                Resume();
            } else {
                Pause();
            }
        }

        if (!isPaused && !gameOver) {
            if (inPlay && !transitioning) {
                surviveTime += Time.deltaTime;
            }

            if (!inPlay && !transitioning) {
                Transition();
            }

            if (transitioning && remainingTransitionTime > 0) {
                roundTitle.text = "NEXT ROUND IN";
                roundTitle.fontSize = 26;
                roundNumber.text = ((int)remainingTransitionTime).ToString();
                remainingTransitionTime-=Time.deltaTime;
            }
            else if (transitioning && remainingTransitionTime <= 0) {
                BeginRound();
            }

            if (!transitioning && 
                spawnDelay <= 0 && 
                inPlay &&
                GameObject.FindGameObjectsWithTag("cat").Length < maxCatsOnMap &&
                spawnedCats < roundCatCount) {
                SpawnCat();
            }
            else if (!transitioning && 
                spawnDelay > 0 && 
                inPlay) {
                spawnDelay-=Time.deltaTime;
            }

            if (pointsAddEffectDelay > 0) {
                pointsAddEffectDelay-=Time.deltaTime;
            }
            if (pointsAddEffectDelay <= 0) {
                pointsAddEffect.SetActive(false);
            }

            if (pointsSubtractEffectDelay > 0) {
                pointsSubtractEffectDelay-=Time.deltaTime;
            }
            if (pointsSubtractEffectDelay <= 0) {
                pointsSubtractEffect.SetActive(false);
            }
        }

        pointsText.text = points.ToString();
    }

    void SpawnCat() {
        spawnDelay = spawnDelayDuration;

        int tmp = Random.Range(0,catSpawns.Count);
        GameObject newCat = Instantiate(catPrefab,
            catSpawns[tmp].transform.position,
            Quaternion.identity
        );
        newCat.GetComponent<catNavigation>().spawn = catSpawns[tmp];

        spawnedCats++;
        catsSpawned++;
    }

    void Transition() {
        transitioning = true;

        remainingTransitionTime = transitionDuration;

        if (round != 0) {
            infoTextObject.transform.GetChild(1).GetComponent<UnityEngine.UI.Text>().text = 
                "ROUND " + round + " OVER";
            infoTextObject.GetComponent<Animator>().Play("infoTextFade");
            roundsSurvived++;
        } else {
            remainingTransitionTime = startTransitionDuration;
        }

        round++;

        if (difficulty == 0) {
            roundCatCount = baseCatCount + (round * kittenCountMultiplier);
            catMaxHealth = baseCatHealth + (round * kittenHealthMultiplier);
        } else if (difficulty == 1) {
            roundCatCount = baseCatCount + (round * catCountMultiplier);
            catMaxHealth = baseCatHealth + (round * catHealthMultiplier);
        } else {
            roundCatCount = baseCatCount + (round * wildcatCountMultiplier);
            catMaxHealth = baseCatHealth + (round * wildcatHealthMultiplier);
        }

        // roundNumber.GetComponent<Animator>().Play("fade");

        spawnedCats = 0;
    }

    void BeginRound() {
        transitioning = false;

        sfxAudioSource.clip = roundSound;
        sfxAudioSource.Play();

        infoTextObject.transform.GetChild(1).GetComponent<UnityEngine.UI.Text>().text = 
            "ROUND " + round + " STARTING";
        infoTextObject.GetComponent<Animator>().Play("infoTextFade");
        
        roundTitle.text = "ROUND";
        roundTitle.fontSize = 38;
        roundNumber.text = round.ToString();
        // roundNumber.GetComponent<Animator>().Play("fadeIn");
    }

    bool StillInPlay() {
        if (spawnedCats == roundCatCount &&
            GameObject.FindGameObjectsWithTag("cat").Length == 0) {
            return false;
        }
        else {
            return true;
        }
    }

    public void AddPoints(int amount) {
        points += amount;
        beanBucksEarned += amount;

        if (pointsAddEffectDelay > 0) {
            pointsAddEffect.SetActive(false);
        }
        pointsAddEffect.SetActive(true);
        pointsAddEffect.GetComponent<Animator>().Play("pointsAddEffect");
        pointsAddEffect.GetComponent<UnityEngine.UI.Text>().text = "+" + amount;

        pointsAddEffectDelay = effectDelayDuration;
    }

    public void SubtractPoints(int amount) {
        points -= amount;
        beanBucksSpent += amount;

        if (pointsSubtractEffectDelay > 0) {
            pointsSubtractEffect.SetActive(false);
        }
        pointsSubtractEffect.SetActive(true);
        pointsSubtractEffect.GetComponent<Animator>().Play("pointsSubtractEffect");
        pointsSubtractEffect.GetComponent<UnityEngine.UI.Text>().text = "-" + amount;

        pointsSubtractEffectDelay = effectDelayDuration;
    }

    public void AddSpawns(GameObject[] newSpawns) {
        for (int x = 0; x < newSpawns.Length; x++) {
            if (catSpawns.IndexOf(newSpawns[x]) == -1) {
                catSpawns.Add(newSpawns[x]);
            }
        }
    }

    public void PurchaseBalloons() {
        SubtractPoints(balloonPackCost);
        GameObject.FindGameObjectWithTag("player").GetComponent<playerAttackController>().AddEmptyBalloons(pickUpBalloonAmount);
        waterBalloonsBought += pickUpBalloonAmount;
    }

    public void PurchaseSnack() {
        SubtractPoints(snackCost);
        snacksBought += snackAmount;
    }


    public void GameOver() {
        gameOver = true;

        sfxAudioSource.clip = playerDefeatedSound;
        sfxAudioSource.Play();

        endGameScoreTexts[0].text = 
            roundsSurvived.ToString() + 
            "\n" + (int)surviveTime;
        endGameScoreTexts[1].text = 
            catsSpawned.ToString() + 
            "\n" + catsDefeated.ToString();
        endGameScoreTexts[2].text = 
            beanBucksEarned.ToString() + 
            "\n" + beanBucksSpent.ToString();
        endGameScoreTexts[3].text = 
            planksKnockedOff.ToString() + 
            "\n" + planksPutUp.ToString();
        endGameScoreTexts[4].text = 
            waterBalloonsCollected.ToString() + 
            "\n" + waterBalloonsThrown.ToString() + 
            "\n" + waterBalloonsBought.ToString();
        endGameScoreTexts[5].text = 
            snacksBought.ToString();

        gameOverMenu.SetActive(true);
        
        Cursor.lockState = CursorLockMode.None;
    }

    public void Restart() {
        adManager.ShowAd("restart");
    }

    public void Quit() {
        adManager.ShowAd("quit");
    }

    public void Pause() {
        isPaused = true;
        pauseMenu.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        musicAudioSource.Pause();
        foreach (GameObject b in GameObject.FindGameObjectsWithTag("balloon")) {
            waterBalloonScript balloon = b.GetComponent<waterBalloonScript>();
            balloon.Paused();
        }
    }

    public void Resume() {
        print("Unpaused");
        isPaused = false;
        pauseMenu.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        musicAudioSource.Play();
        foreach (GameObject b in GameObject.FindGameObjectsWithTag("balloon")) {
            waterBalloonScript balloon = b.GetComponent<waterBalloonScript>();
            balloon.Unpaused();
        }
    }

    public void ShowLeaderboard() {
        // show leaderboard UI
        // Social.ShowLeaderboardUI();
    }
}
