using UnityEngine;
using TMPro;

public class VRPowerUp : MonoBehaviour
{
    public float requiredGazeTime = 0.8f;
    public AudioSource audioSource;
    public AudioClip collectSound;
    public TMP_Text bonusText;

    private float timeBonus;
    private float gazeTimer = 0f;
    private Renderer sphereRenderer;

    void Awake()
    {
        timeBonus = Random.Range(3, 8);
        if (bonusText != null)
        {
            bonusText.text = "+" + timeBonus + "s";
        }

        sphereRenderer = GetComponent<Renderer>();
        if (sphereRenderer != null)
        {
            sphereRenderer.material.color = new Color(0.2f, 0.6f, 1.0f, 0.8f);
        }
    }

    public void OnGazeEnter()
    {
        gazeTimer = 0f;
    }

    public void OnGazeStay(FindItGameManager gameManager)
    {
        gazeTimer += Time.deltaTime;

        if (gazeTimer >= requiredGazeTime)
        {
            CollectPowerUp(gameManager);
            gazeTimer = 0f;
        }
    }

    public void OnGazeExit()
    {
        gazeTimer = 0f;
    }

    void CollectPowerUp(FindItGameManager gameManager)
    {
        if (gameManager != null)
        {
            gameManager.AddTime(timeBonus);
        }

        if (audioSource != null && collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }

        Destroy(gameObject);
    }
}