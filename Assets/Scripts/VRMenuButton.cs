using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class VRMenuButton : MonoBehaviour
{
    public float requiredGazeTime = 1.0f;
    public UnityEvent OnButtonClicked;
    public UnityEvent OnGazeExitEvent;
    public Image fillImage;

    private float gazeTimer = 0f;
    private bool isGazing = false;

    public void OnGazeEnter()
    {
        isGazing = true;
        gazeTimer = 0f;
    }

    public void OnGazeStay()
    {
        if (!isGazing) return;

        gazeTimer += Time.deltaTime;
        if (fillImage != null)
        {
            fillImage.fillAmount = gazeTimer / requiredGazeTime;
        }

        if (gazeTimer >= requiredGazeTime)
        {
            TriggerAction();
            ResetGaze();
        }
    }

    public void OnGazeExit()
    {
        ResetGaze();
        OnGazeExitEvent?.Invoke();
    }

    void TriggerAction()
    {
        OnButtonClicked?.Invoke();
    }

    void ResetGaze()
    {
        isGazing = false;
        gazeTimer = 0f;
        if (fillImage != null)
        {
            fillImage.fillAmount = 0f;
        }
    }
}