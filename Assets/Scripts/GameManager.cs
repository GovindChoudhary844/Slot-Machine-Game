using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Central brain of the slot-machine:
/// credits, betting, win evaluation, UI updates, audio triggers, button lock.
/// </summary>
public class GameManager : MonoBehaviour
{
    /* ---------- Events ---------- */
    public static event Action HandlePulled = delegate { }; // notifies Rows to spin

    [Header("--- UI ---")]
    [SerializeField] private TextMeshProUGUI creditsText;
    [SerializeField] private TextMeshProUGUI winText;

    [Header("--- Betting ---")]
    [SerializeField] private Button spin10Btn;
    [SerializeField] private Button spin50Btn;
    [SerializeField] private Button spin100Btn;
    [SerializeField] private Animator leverAnimator;
    [SerializeField] private GameObject btnHolder; // parent of all bet buttons

    [Header("--- Reels ---")]
    [SerializeField] private Row[] rows; // drag exactly 3 Row scripts

    /* ---------- State ---------- */
    private int credits = 500;
    private bool resultsChecked;   // prevents multiple evaluations per stop
    private bool spinning;         // blocks new spins while true
    private int currentCost;       // 10 / 50 / 100
    private bool reelsEverFired;   // guards premature jackpot on first spin

    /* ---------- Life-cycle ---------- */
    private void Awake()
    {
        // Wire the three bet buttons
        spin10Btn.onClick.AddListener(() => OnSpin(10));
        spin50Btn.onClick.AddListener(() => OnSpin(50));
        spin100Btn.onClick.AddListener(() => OnSpin(100));

        RefreshUI();
    }

    /* ---------- Betting ---------- */
    public void OnSpin(int cost)
    {
        if (spinning || credits < cost) return; // safety

        currentCost = cost;
        credits -= cost;
        spinning = true;
        reelsEverFired = false;
        resultsChecked = false;
        winText.enabled = false;
        winText.text = "";

        RefreshUI();
        btnHolder.SetActive(false); // hide all bet buttons
        StartCoroutine(SpinRoutine());
    }

    /* ---------- Spin Coroutine ---------- */
    private IEnumerator SpinRoutine()
    {
        SetAllSpinButtons(false); // cosmetic lock

        AudioManager.Instance.PlayLeverPull();
        leverAnimator.Play("LeverPull", 0, 0);
        yield return new WaitForSeconds(1f); // wait for 1-second animation

        AudioManager.Instance.StartReelScroll();
        HandlePulled?.Invoke(); // fire event -> Rows start rotating
        reelsEverFired = true;
        yield return new WaitForSeconds(0.15f);

        SetAllSpinButtons(true);
        spinning = false; // Rows will stop themselves
    }

    /* ---------- Per-frame Check ---------- */
    private void Update()
    {
        bool anySpinning = false;
        foreach (Row r in rows)
            if (!r.rowStopped) { anySpinning = true; break; }

        if (anySpinning)
        {
            resultsChecked = false;
            winText.enabled = false;
            return;
        }

        // Reels have JUST stopped this frame
        if (!resultsChecked && reelsEverFired)
        {
            resultsChecked = true;
            EvaluateWin();
            btnHolder.SetActive(true); // bring back bet buttons
        }
    }

    /* ---------- Win Evaluation ---------- */
    private void EvaluateWin()
    {
        string a = rows[0].stoppedSlot;
        string b = rows[1].stoppedSlot;
        string c = rows[2].stoppedSlot;

        if (a == b && b == c) // 3-of-a-kind
        {
            int pay = currentCost * 10;
            credits += pay;
            winText.text = $"JACKPOT {pay}!";
            winText.enabled = true;
            AudioManager.Instance.PlayWin();
        }
        else
        {
            winText.text = "";
            winText.enabled = false;
        }

        RefreshUI();
    }

    /* ---------- Helpers ---------- */
    private void SetAllSpinButtons(bool interactable)
    {
        spin10Btn.interactable = interactable;
        spin50Btn.interactable = interactable;
        spin100Btn.interactable = interactable;
    }

    private void RefreshUI() => creditsText.text = $"Credits: {credits}";
}