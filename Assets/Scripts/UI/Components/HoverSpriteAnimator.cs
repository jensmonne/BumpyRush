using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverSpriteAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover Animation Settings")]
    [SerializeField] private Sprite[] hoverSprites; // Your 3 animation sprites
    [SerializeField] private float timePerFrame = 0.33f; // Time in seconds per frame

    private Button targetButton;
    private int currentFrameIndex;
    private float timer;
    private bool isHovered;

    private void Awake()
    {
        targetButton = GetComponent<Button>();
    }

    private void Update()
    {
        if (!isHovered || hoverSprites == null || hoverSprites.Length == 0) return;

        // Run the timer while the mouse is hovering
        timer += Time.deltaTime;
        if (timer >= timePerFrame)
        {
            timer = 0f;
            
            // Cycle to the next animation frame
            currentFrameIndex = (currentFrameIndex + 1) % hoverSprites.Length;
            
            // Inject the new sprite directly into the Button's SpriteState struct
            UpdateButtonHighlightedSprite(hoverSprites[currentFrameIndex]);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        timer = 0f;
        currentFrameIndex = 0;

        // Instantly set the first frame when the hover begins
        if (hoverSprites != null && hoverSprites.Length > 0)
        {
            UpdateButtonHighlightedSprite(hoverSprites[0]);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        // The Button component automatically handles reverting back to the 'Normal' sprite!
    }

    private void UpdateButtonHighlightedSprite(Sprite newSprite)
    {
        if (targetButton == null || targetButton.transition != Selectable.Transition.SpriteSwap) return;

        // Because targetButton.spriteState is a struct, we have to copy it, 
        // modify it, and assign it back to the button.
        SpriteState state = targetButton.spriteState;
        state.highlightedSprite = newSprite;
        targetButton.spriteState = state;
    }

    private void OnDisable()
    {
        isHovered = false;
    }
}