using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FindItGameManager : MonoBehaviour
{
    public enum Difficulty { Easy, Medium, Hard, Extreme }

    [Header("UI Panels")]
    public GameObject mainMenuCanvas;
    public GameObject mainPanel;
    public GameObject instructionsPanel;
    public GameObject gameHUDCanvas;

    [Header("UI References")]
    public TMP_Text targetText;
    public TMP_Text timerText;
    public TMP_Text scoreText;
    public TMP_Text highScoreText;

    [Header("Difficulty Texts")]
    public TMP_Text easyText;
    public TMP_Text mediumText;
    public TMP_Text hardText;
    public TMP_Text extremeText;

    public Color selectedColor = Color.black;
    public Color defaultColor = Color.white;

    [Header("Sound Effects")]
    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip gameOverSound;
    public AudioClip highScoreSound;

    [Header("Celebration Effects")]
    public ParticleSystem confettiEffect;

    [Header("Room Groups")]
    public GameObject room1Group;
    public GameObject room2Group;
    public GameObject room3Group;

    private List<string> availableObjects = new List<string>();
    private string currentTarget;
    private float timeRemaining;
    private int score = 0;
    private bool isGameActive = false;

    private Difficulty selectedDifficulty = Difficulty.Medium;
    private int chosenStartingRoom = 1;
    private int pointsToNextRoom = 3;

    void Start()
    {
        SetupHUDOverlay();
        if (highScoreText == null && mainPanel != null)
        {
            Transform cr = mainPanel.transform.Find("CurrentRecord");
            if (cr != null) highScoreText = cr.GetComponent<TMP_Text>();
        }
        SetDifficulty("Medium");
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        CancelInvoke(); // Cancela timers de Invoke pendentes (como o do GameOver)
        isGameActive = false;

        if (confettiEffect != null)
        {
            confettiEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);
        if (mainPanel != null) mainPanel.SetActive(true);
        if (instructionsPanel != null) instructionsPanel.SetActive(false);
        SetGameHUDActive(false);

        if (targetText != null)
        {
            targetText.gameObject.SetActive(false);
        }

        UpdateMenuHighScore();

        SetRoomsActive(0);
        UpdateDifficultyTextColors();
    }

    public void ShowInstructions()
    {
        if (instructionsPanel != null) instructionsPanel.SetActive(true);
        if (mainPanel != null) mainPanel.SetActive(false);
    }

    public void HideInstructions()
    {
        if (instructionsPanel != null) instructionsPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true);
    }

    public void SetDifficulty(string diff)
    {
        if (diff == "Easy") selectedDifficulty = Difficulty.Easy;
        else if (diff == "Medium") selectedDifficulty = Difficulty.Medium;
        else if (diff == "Hard") selectedDifficulty = Difficulty.Hard;
        else if (diff == "Extreme") selectedDifficulty = Difficulty.Extreme;

        UpdateDifficultyTextColors();
        UpdateMenuHighScore();
    }

    private void UpdateMenuHighScore()
    {
        int record = GetCurrentDifficultyHighScore();
        string text = selectedDifficulty.ToString().ToUpper() + " RECORD: " + record;

        if (highScoreText != null)
        {
            highScoreText.text = text;
        }
        else if (targetText != null)
        {
            targetText.text = text;
        }
    }

    private string GetHighScoreKey()
    {
        return "HighScore_" + selectedDifficulty.ToString();
    }

    public int GetCurrentDifficultyHighScore()
    {
        return PlayerPrefs.GetInt(GetHighScoreKey(), 0);
    }

    private void UpdateDifficultyTextColors()
    {
        if (easyText != null) easyText.color = (selectedDifficulty == Difficulty.Easy) ? selectedColor : defaultColor;
        if (mediumText != null) mediumText.color = (selectedDifficulty == Difficulty.Medium) ? selectedColor : defaultColor;
        if (hardText != null) hardText.color = (selectedDifficulty == Difficulty.Hard) ? selectedColor : defaultColor;
        if (extremeText != null) extremeText.color = (selectedDifficulty == Difficulty.Extreme) ? selectedColor : defaultColor;
    }

    public void SetStartingRoom(int roomIndex)
    {
        chosenStartingRoom = roomIndex;
    }

    public void StartGameFromMenu()
    {
        CancelInvoke(); // Limpa chamadas de ShowMainMenu pendentes de partidas anteriores
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);
        if (instructionsPanel != null) instructionsPanel.SetActive(false);
        SetGameHUDActive(true);

        score = 0;
        isGameActive = true;
        pointsToNextRoom = Random.Range(2, 6);
        SetupRoomAndObjects();
        NextRound();
    }

    private void SetGameHUDActive(bool active)
    {
        if (targetText != null) targetText.gameObject.SetActive(active);
        if (timerText != null) timerText.gameObject.SetActive(active);
        if (scoreText != null) scoreText.gameObject.SetActive(active);

        if (gameHUDCanvas != null)
        {
            // Se o retículo for filho de gameHUDCanvas (ex: VR_HUD), mantemos o canvas ativo
            // para que a mira/retículo continue visível no menu principal.
            VRGazeReticle reticle = FindAnyObjectByType<VRGazeReticle>();
            bool containsReticle = false;
            if (reticle != null && reticle.reticleFillImage != null)
            {
                containsReticle = reticle.reticleFillImage.transform.IsChildOf(gameHUDCanvas.transform);
            }
            else if (gameHUDCanvas.transform.Find("ReticleOuter") != null)
            {
                containsReticle = true;
            }

            if (!containsReticle)
            {
                gameHUDCanvas.SetActive(active);
            }
            else
            {
                gameHUDCanvas.SetActive(true);
            }
        }
    }

    void SetupRoomAndObjects()
    {
        int activeRoom = chosenStartingRoom;
        if (chosenStartingRoom == 0)
        {
            activeRoom = Random.Range(1, 4);
        }
        SetRoomsActive(activeRoom);
        RefreshObjectList();
    }

    void SetRoomsActive(int roomNum)
    {
        if (room1Group != null) room1Group.SetActive(roomNum == 1);
        if (room2Group != null) room2Group.SetActive(roomNum == 2);
        if (room3Group != null) room3Group.SetActive(roomNum == 3);
    }

    void RefreshObjectList()
    {
        availableObjects.Clear();
        GameObject activeRoomObj = null;

        if (room1Group != null && room1Group.activeSelf) activeRoomObj = room1Group;
        else if (room2Group != null && room2Group.activeSelf) activeRoomObj = room2Group;
        else if (room3Group != null && room3Group.activeSelf) activeRoomObj = room3Group;

        if (activeRoomObj != null)
        {
            Transform[] allChildren = activeRoomObj.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in allChildren)
            {
                if (child.CompareTag("TargetObject"))
                {
                    if (!availableObjects.Contains(child.gameObject.name))
                    {
                        availableObjects.Add(child.gameObject.name);
                    }
                }
            }
        }

        if (availableObjects.Count == 0)
        {
            GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag("TargetObject");
            foreach (GameObject obj in taggedObjects)
            {
                if (!availableObjects.Contains(obj.name))
                {
                    availableObjects.Add(obj.name);
                }
            }
        }
    }

    void NextRound()
    {
        // Reset do tempo atribuído IMEDIATAMENTE no início da ronda
        if (selectedDifficulty == Difficulty.Extreme)
        {
            timeRemaining = 5.0f;
        }
        else
        {
            float initial = 10f;
            float minimum = 4f;
            if (selectedDifficulty == Difficulty.Easy) { initial = 12f; minimum = 7f; }
            else if (selectedDifficulty == Difficulty.Medium) { initial = 10f; minimum = 5f; }
            else if (selectedDifficulty == Difficulty.Hard) { initial = 7f; minimum = 3f; }

            float calculatedTime = initial - (score * 1.0f);
            timeRemaining = Mathf.Max(calculatedTime, minimum);
        }

        // Troca de sala se o modo for aleatório (chosenStartingRoom == 0)
        if (chosenStartingRoom == 0)
        {
            if (score >= pointsToNextRoom)
            {
                int currentActiveRoom = 1;
                if (room2Group != null && room2Group.activeSelf) currentActiveRoom = 2;
                else if (room3Group != null && room3Group.activeSelf) currentActiveRoom = 3;

                int nextRoom;
                do
                {
                    nextRoom = Random.Range(1, 4);
                } while (nextRoom == currentActiveRoom);

                SetRoomsActive(nextRoom);
                RefreshObjectList();
                pointsToNextRoom = score + Random.Range(2, 6);
            }
        }

        if (availableObjects.Count == 0) RefreshObjectList();
        if (availableObjects.Count == 0) return;

        int randomIndex = Random.Range(0, availableObjects.Count);
        currentTarget = availableObjects[randomIndex];

        UpdateUI();
    }

    void Update()
    {
        if (!isGameActive) return;

        timeRemaining -= Time.deltaTime;

        if (timerText != null)
        {
            timerText.text = Mathf.Max(0f, timeRemaining).ToString("F1") + "s";
            if (timeRemaining <= 3.0f)
            {
                timerText.color = (Mathf.FloorToInt(Time.time * 5) % 2 == 0) ? Color.red : Color.white;
            }
            else
            {
                timerText.color = Color.white;
            }
        }

        if (timeRemaining <= 0)
        {
            GameOver();
        }
    }

    public void AddTime(float amount)
    {
        timeRemaining += amount;
    }

    public void ValidateSelection(string selectedObjectName)
    {
        if (!isGameActive) return;

        if (selectedObjectName.Equals(currentTarget))
        {
            score++;
            
            // Remove o objeto encontrado para não se repetir na mesma lista
            availableObjects.Remove(selectedObjectName);

            if (audioSource != null && correctSound != null)
            {
                audioSource.Stop(); // Para qualquer som a tocar antes de emitir o som de acerto
                audioSource.PlayOneShot(correctSound);
            }

            NextRound();
        }
    }

    void UpdateUI()
    {
        if (targetText != null) targetText.text = "FIND: " + currentTarget.ToUpper();
        if (scoreText != null) scoreText.text = "Score: " + score;
    }

    void GameOver()
    {
        if (!isGameActive) return; // Evita múltiplas chamadas consecutivas no mesmo frame

        isGameActive = false;

        string key = GetHighScoreKey();
        int bestScore = PlayerPrefs.GetInt(key, 0);
        bool isNewRecord = score > bestScore;

        if (isNewRecord)
        {
            bestScore = score;
            PlayerPrefs.SetInt(key, bestScore);
            PlayerPrefs.Save();
            TriggerConfetti();
        }

        string diffName = selectedDifficulty.ToString().ToUpper();

        if (targetText != null)
        {
            targetText.text = isNewRecord ? "NEW " + diffName + " RECORD: " + score + "!" : "GAME OVER!";
        }

        if (timerText != null)
        {
            timerText.text = "BEST: " + bestScore;
            timerText.color = isNewRecord ? Color.yellow : Color.white;
        }

        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
            if (isNewRecord && highScoreSound != null)
            {
                audioSource.PlayOneShot(highScoreSound);
            }
            else if (gameOverSound != null)
            {
                audioSource.PlayOneShot(gameOverSound);
            }
        }

        float delay = (isNewRecord && highScoreSound != null) ? 4.0f : 3.5f;
        Invoke("ShowMainMenu", delay);
    }

    private void TriggerConfetti()
    {
        if (confettiEffect == null)
        {
            confettiEffect = CreateProceduralConfetti();
        }

        if (confettiEffect != null)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                confettiEffect.transform.position = cam.transform.position + cam.transform.forward * 2.2f + cam.transform.up * 0.8f;
                confettiEffect.transform.rotation = Quaternion.LookRotation(cam.transform.forward) * Quaternion.Euler(30f, 0f, 0f);
            }
            confettiEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            confettiEffect.Play();
        }
    }

    private ParticleSystem CreateProceduralConfetti()
    {
        GameObject confettiObj = new GameObject("ConfettiCelebration");
        DontDestroyOnLoad(confettiObj);

        ParticleSystem ps = confettiObj.AddComponent<ParticleSystem>();
        ParticleSystemRenderer psRenderer = confettiObj.GetComponent<ParticleSystemRenderer>();

        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("UI/Default");

        Material mat = new Material(shader);
        psRenderer.material = mat;

        var main = ps.main;
        main.duration = 3.5f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(2.0f, 3.2f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 4.0f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.12f);
        main.gravityModifier = 0.35f;
        main.playOnAwake = false;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.85f, 0.2f), 0.0f),
                new GradientColorKey(new Color(0.2f, 0.85f, 1f), 0.25f),
                new GradientColorKey(new Color(1f, 0.25f, 0.6f), 0.5f),
                new GradientColorKey(new Color(0.3f, 1f, 0.4f), 0.75f),
                new GradientColorKey(new Color(1f, 0.35f, 0.2f), 1.0f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(1.0f, 0.8f),
                new GradientAlphaKey(0.0f, 1.0f)
            }
        );
        main.startColor = new ParticleSystem.MinMaxGradient(gradient) { mode = ParticleSystemGradientMode.RandomColor };

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0.0f, 70, 90),
            new ParticleSystem.Burst(0.4f, 40, 60),
            new ParticleSystem.Burst(0.9f, 30, 50)
        });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 35f;
        shape.radius = 0.6f;

        var rot = ps.rotationOverLifetime;
        rot.enabled = true;
        rot.z = new ParticleSystem.MinMaxCurve(-180f * Mathf.Deg2Rad, 180f * Mathf.Deg2Rad);

        var velocity = ps.limitVelocityOverLifetime;
        velocity.enabled = true;
        velocity.dampen = 0.15f;

        return ps;
    }

    private void SetupHUDOverlay()
    {
        Shader overlayShader = Shader.Find("TextMeshPro/Mobile/Distance Field Overlay");
        if (overlayShader == null)
        {
            overlayShader = Shader.Find("TextMeshPro/Distance Field Overlay");
        }

        if (gameHUDCanvas != null)
        {
            Canvas canvas = gameHUDCanvas.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.overrideSorting = true;
                canvas.sortingLayerName = "ReticleTop";
                canvas.sortingOrder = 100;
            }

            TMP_Text[] hudTexts = gameHUDCanvas.GetComponentsInChildren<TMP_Text>(true);
            foreach (var txt in hudTexts)
            {
                ApplyOverlayShader(txt, overlayShader);
            }

            Image[] hudImages = gameHUDCanvas.GetComponentsInChildren<Image>(true);
            foreach (var img in hudImages)
            {
                ApplyImageOverlay(img);
            }
        }
        else
        {
            ApplyOverlayShader(targetText, overlayShader);
            ApplyOverlayShader(timerText, overlayShader);
            ApplyOverlayShader(scoreText, overlayShader);
        }

        VRGazeReticle reticle = FindAnyObjectByType<VRGazeReticle>();
        if (reticle != null && reticle.reticleFillImage != null)
        {
            ApplyImageOverlay(reticle.reticleFillImage);
            Transform outerTrans = reticle.reticleFillImage.transform.parent;
            if (outerTrans != null)
            {
                Image outerImg = outerTrans.GetComponent<Image>();
                if (outerImg != null) ApplyImageOverlay(outerImg);
            }
        }
    }

    private void ApplyOverlayShader(TMP_Text textComponent, Shader shader)
    {
        if (textComponent == null) return;
        Material baseMat = textComponent.fontSharedMaterial != null ? textComponent.fontSharedMaterial : textComponent.fontMaterial;
        if (baseMat != null)
        {
            Material overlayMat = new Material(baseMat);
            if (shader != null)
            {
                overlayMat.shader = shader;
            }
            overlayMat.SetInt("unity_GUIZTestMode", (int)UnityEngine.Rendering.CompareFunction.Always);
            textComponent.fontMaterial = overlayMat;
            textComponent.SetMaterialDirty();
        }
    }

    private void ApplyImageOverlay(Image image)
    {
        if (image == null) return;
        Shader uiShader = Shader.Find("UI/Default");
        if (uiShader != null)
        {
            Material mat = new Material(uiShader);
            mat.SetInt("unity_GUIZTestMode", (int)UnityEngine.Rendering.CompareFunction.Always);
            image.material = mat;
        }
    }
}