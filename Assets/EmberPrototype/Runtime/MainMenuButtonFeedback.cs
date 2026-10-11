using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EmberPrototype
{
    [RequireComponent(typeof(Button))]
    public sealed class MainMenuButtonFeedback : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private Text label;
        [SerializeField] private CanvasGroup ornament;
        [SerializeField] private Color normalColor = new Color(0.85f, 0.87f, 0.86f);
        [SerializeField] private Color selectedColor = new Color(1f, 0.81f, 0.47f);
        [SerializeField, Min(0f)] private float responseSpeed = 12f;
        private Button button;
        private bool hovered;
        private bool selected;
        private float focus;

        private void Awake() => button = GetComponent<Button>();
        private void OnEnable()
        {
            focus = 0f;
            hovered = false;
            selected = EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject;
            UpdateVisuals();
        }

        private void Update()
        {
            bool active = button != null && button.IsInteractable() && (hovered || selected);
            focus = Mathf.MoveTowards(focus, active ? 1f : 0f, Time.unscaledDeltaTime * responseSpeed);
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (label != null) label.color = Color.Lerp(normalColor, selectedColor, focus);
            if (ornament != null) ornament.alpha = focus;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (button == null || !button.IsInteractable()) return;
            hovered = true;
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(gameObject);
        }
        public void OnPointerExit(PointerEventData eventData) => hovered = false;
        public void OnSelect(BaseEventData eventData) => selected = true;
        public void OnDeselect(BaseEventData eventData) => selected = false;
    }
}
