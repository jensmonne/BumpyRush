using System.Collections;
using UnityEngine;

public class PlayerFlattenHandler : MonoBehaviour
{
    private Coroutine flattenCoroutine;

    public void StartFlatten()
    {
        if (flattenCoroutine != null)
        {
            StopCoroutine(flattenCoroutine);
        }
        flattenCoroutine = StartCoroutine(FlattenPlayer());
    }

    private IEnumerator FlattenPlayer()
    {
        Debug.Log($"Flattening {gameObject.name}!");
        Vector3 originalScale = new Vector3(1f, 1f, 1f);
        Vector3 flattenedScale = new Vector3(originalScale.x, originalScale.y * 0.3f, originalScale.z);
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, flattenedScale, elapsed / duration);
            yield return null;
        }
        transform.localScale = flattenedScale;

        Debug.Log("Waiting...");
        yield return new WaitForSeconds(1f);

        Debug.Log($"Restoring {gameObject.name} scale...");
        elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(flattenedScale, originalScale, elapsed / duration);
            yield return null;
        }

        transform.localScale = originalScale;
        Debug.Log($"Restored {gameObject.name} to original scale!");
    }
}