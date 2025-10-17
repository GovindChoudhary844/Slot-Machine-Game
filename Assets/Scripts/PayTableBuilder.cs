using UnityEngine;

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