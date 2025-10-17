using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static event Action HandlePulled = delegate { };

    [Header("UI & Pay-table")]
    [SerializeField] private TextMeshProUGUI creditsText;
    [SerializeField] private TextMeshProUGUI winText;

    [Header("Input & FX")]
    [SerializeField] private Button spin10Btn;
    [SerializeField] private Button spin50Btn;
    [SerializeField] private Button spin100Btn;
    [SerializeField] private Animator leverAnimator;
    [SerializeField] private GameObject btnHolder;

    [Header("Reels")]
    [SerializeField] private Row[] rows;

    /* ---------------------------------------------------------- */
    private int credits = 500;
    private bool resultsChecked;
    private bool spinning;
    private int currentCost;   // 10, 50 or 100

    private bool reelsEverFired;

    /* ---------------------------------------------------------- */
    private void Awake()
    {
        // bind the three buttons
        spin10Btn.onClick.AddListener(() => OnSpin(10));
        spin50Btn.onClick.AddListener(() => OnSpin(50));
        spin100Btn.onClick.AddListener(() => OnSpin(100));

        RefreshUI();
    }

    /* ---------------------------------------------------------- */
    /*  1 entry-point for every cost                              */
    /* ---------------------------------------------------------- */
    public void OnSpin(int cost)
    {
        if (spinning || credits < cost) return;

        currentCost = cost;
        credits -= currentCost;
        spinning = true;
        reelsEverFired = false;
        resultsChecked = false;
        winText.enabled = false;
        winText.text = "";
        RefreshUI();

        btnHolder.SetActive(false);
        StartCoroutine(SpinRoutine());
    }

    /* ---------------------------------------------------------- */
    private IEnumerator SpinRoutine()
    {
        SetAllSpinButtons(false);   // lock while spinning

        leverAnimator.Play("LeverPull", 0, 0);
        yield return new WaitForSeconds(1f); // let 1-s animation finish

        HandlePulled?.Invoke();      // fire reels
        reelsEverFired = true;
        yield return new WaitForSeconds(0.15f);

        SetAllSpinButtons(true);     // unlock after reels start
        spinning = false;            // rows will stop themselves
    }

    /* ---------------------------------------------------------- */
    private void Update()
    {
        bool anySpinning = false;
        foreach (Row r in rows) if (!r.rowStopped) { anySpinning = true; break; }

        if (anySpinning)
        {
            resultsChecked = false;
            winText.enabled = false;
            return;
        }

        if (!resultsChecked && reelsEverFired)
        {
            resultsChecked = true;
            EvaluateWin();

            btnHolder.SetActive(true);
        }
    }

    /* ---------------------------------------------------------- */
    private void EvaluateWin()
    {
        string a = rows[0].stoppedSlot;
        string b = rows[1].stoppedSlot;
        string c = rows[2].stoppedSlot;

        if (a == b && b == c)              // JACKPOT
        {
            int pay = currentCost * 10;    // e.g. 10→100, 50→500, 100→1000
            credits += pay;
            winText.text = $"JACKPOT {pay}!";
            winText.enabled = true;
        }
        else
        {
            winText.text = "";
            winText.enabled = false;
        }

        RefreshUI();
    }

    /* ---------------------------------------------------------- */
    private void SetAllSpinButtons(bool interactable)
    {
        spin10Btn.interactable = interactable;
        spin50Btn.interactable = interactable;
        spin100Btn.interactable = interactable;
    }

    private void RefreshUI() => creditsText.text = $"Credits: {credits}";
}