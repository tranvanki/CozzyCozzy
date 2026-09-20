using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class TargetsUI : MonoBehaviour
{
    [System.Serializable]
    public class TargetSlot
    {
        public int colorIndex;
        public TextMeshProUGUI countText;
    }

    [SerializeField] private GameManager gameManager;
    [SerializeField] private List<TargetSlot> slots;

    void OnEnable() => gameManager.OnTargetChanged += UpdateSlot;
    void OnDisable() => gameManager.OnTargetChanged -= UpdateSlot;

    void Start()
    {
        foreach (var slot in slots)
            if (gameManager.TargetRemaining.ContainsKey(slot.colorIndex))
                slot.countText.text = gameManager.TargetRemaining[slot.colorIndex].ToString();
    }

    void UpdateSlot(int colorIndex, int remaining)
    {
        foreach (var slot in slots)
            if (slot.colorIndex == colorIndex)
                slot.countText.text = remaining.ToString();
    }
}
