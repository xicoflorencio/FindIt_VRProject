using UnityEngine;
using UnityEngine.UI;

public class VRGazeReticle : MonoBehaviour
{
    [Header("Ray Settings")]
    public float maxDistance = 15f;
    public float requiredGazeTime = 1.0f;

    [Header("UI References")]
    public Image reticleFillImage;
    public FindItGameManager gameManager;

    private float timer = 0f;
    private GameObject currentGazedObject;
    private VRMenuButton currentMenuButton;

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            Transform hitTransform = hit.transform;
            GameObject hitObject = hitTransform.gameObject;

            VRMenuButton menuButton = hitTransform.GetComponent<VRMenuButton>();
            if (menuButton != null)
            {
                if (currentGazedObject != hitObject)
                {
                    ResetGaze();
                    currentGazedObject = hitObject;
                    currentMenuButton = menuButton;
                    currentMenuButton.OnGazeEnter();
                }

                if (timer < menuButton.requiredGazeTime)
                {
                    timer += Time.deltaTime;

                    if (reticleFillImage != null)
                    {
                        reticleFillImage.fillAmount = Mathf.Clamp01(timer / menuButton.requiredGazeTime);
                    }

                    currentMenuButton.OnGazeStay();

                    if (timer >= menuButton.requiredGazeTime)
                    {
                        if (reticleFillImage != null)
                        {
                            reticleFillImage.fillAmount = 0f;
                        }
                    }
                }
                return;
            }

            VRPowerUp powerUp = hitTransform.GetComponent<VRPowerUp>();
            if (powerUp != null)
            {
                if (currentGazedObject != hitObject)
                {
                    ResetGaze();
                    currentGazedObject = hitObject;
                    powerUp.OnGazeEnter();
                }

                timer += Time.deltaTime;

                if (reticleFillImage != null)
                {
                    reticleFillImage.fillAmount = timer / powerUp.requiredGazeTime;
                }

                powerUp.OnGazeStay(gameManager);
                return;
            }

            if (hitObject.CompareTag("TargetObject"))
            {
                if (currentGazedObject != hitObject)
                {
                    ResetGaze();
                    currentGazedObject = hitObject;
                }

                timer += Time.deltaTime;

                if (reticleFillImage != null)
                {
                    reticleFillImage.fillAmount = timer / requiredGazeTime;
                }

                if (timer >= requiredGazeTime)
                {
                    if (gameManager != null && currentGazedObject != null)
                    {
                        gameManager.ValidateSelection(currentGazedObject.name);
                    }
                    ResetGaze();
                }
                return;
            }
        }

        ResetGaze();
    }

    private void ResetGaze()
    {
        if (currentMenuButton != null)
        {
            currentMenuButton.OnGazeExit();
            currentMenuButton = null;
        }

        if (currentGazedObject != null)
        {
            VRPowerUp powerUp = currentGazedObject.GetComponent<VRPowerUp>();
            if (powerUp != null)
            {
                powerUp.OnGazeExit();
            }
        }

        currentGazedObject = null;
        timer = 0f;
        if (reticleFillImage != null)
        {
            reticleFillImage.fillAmount = 0f;
        }
    }
}