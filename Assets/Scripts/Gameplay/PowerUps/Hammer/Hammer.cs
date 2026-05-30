using System.Collections;
using UnityEngine;

public class Hammer : MonoBehaviour
{
    [Header("Swing Settings")]
    public float preSwingDuration = 0.15f;
    public float swingDuration = 0.08f;
    public float destroyDelay = 0.3f;
    private bool isSwinging;
    private void Start()
    {
        SwingHammer();
    }
    private void SwingHammer()
    {
        if (gameObject == null || isSwinging) return;
        StartCoroutine(SwingRoutine());
    }

    private IEnumerator SwingRoutine()
    {
        isSwinging = true;
        Transform hammer = gameObject.transform;

        yield return RotateOverTime(hammer, 20f, preSwingDuration);

        yield return RotateOverTime(hammer, -100f, swingDuration);

        yield return new WaitForSeconds(destroyDelay);

        Destroy(gameObject);
        isSwinging = false;
    }

    private IEnumerator RotateOverTime(Transform target, float targetAngleX, float duration)
    {
        Quaternion startRot = target.localRotation;
        Quaternion endRot = Quaternion.Euler(targetAngleX, startRot.eulerAngles.y, startRot.eulerAngles.z);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            target.localRotation = Quaternion.Slerp(startRot, endRot, elapsed / duration);
            yield return null;
        }

        target.localRotation = endRot;
    }
}