using UnityEngine;

/// <summary>
/// Dit script beheert alle geluidseffecten specifiek voor de speler/het object
/// en communiceert met de centrale SoundManager.
/// </summary>
public class PlayerSFX : MonoBehaviour
{
    [Header("Player SFX Clips")]
    [SerializeField] private AudioClip jumpSound;

    public void PlayJumpSound()
    {
        if (jumpSound != null)
        {
            SoundManager.Instance.Play3DSFX(jumpSound, transform.position);
        }
        else
        {
            Debug.LogWarning($"JumpSound is niet toegewezen op {gameObject.name}", this);
        }
    }
}