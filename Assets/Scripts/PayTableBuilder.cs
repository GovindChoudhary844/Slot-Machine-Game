using UnityEngine;

/// <summary>
/// Simple helper to open/close the pay-table info panel.
/// Attach to any UI button or close button inside the panel.
/// </summary>
public class PayTableBuilder : MonoBehaviour
{
    [Header("Existing UI")]
    [SerializeField] private GameObject payTablePanel;   // drag your PayTablePanel here

    /* called by InfoBtn ----------------------------------------------- */
    public void TogglePayTable()
    {
        if (payTablePanel != null)
            payTablePanel.SetActive(!payTablePanel.activeSelf);
    }

    /* called by CloseBtn ---------------------------------------------- */
    public void ClosePayTable()
    {
        if (payTablePanel != null)
            payTablePanel.SetActive(false);
    }
}