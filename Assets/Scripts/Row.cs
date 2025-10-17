using System.Collections;
using UnityEngine;

/// <summary>
/// Visual behaviour of ONE reel: scroll, wrap, snap, report final icon.
/// </summary>
public class Row : MonoBehaviour
{
    [Header("Icon Sprites (bottom → top)")]
    [SerializeField] private SpriteRenderer[] iconRenderers = new SpriteRenderer[4];

    [Header("Mechanical settings")]
    public float topY = 1.75f;      // world-Y where icon-0 sits when index = 0
    public float bottomY = -3.5f;   // world-Y below which we teleport icons upward

    /* ---------- Runtime State ---------- */
    [HideInInspector] public bool rowStopped = true;   // lets GameManager know we’re idle
    [HideInInspector] public string stoppedSlot = "";  // name of the symbol that finally landed

    /* ---------- Private Fields ---------- */
    private string[] iconNames;   // sprite names built in Awake
    private float iconHeight;     // vertical distance between icons (set in Awake)

    /* ---------- Timing Constants ---------- */
    private const float Fast = 0.025f; // initial step speed (seconds per icon)
    private const float Slow = 0.20f;  // final step speed (creates natural slowdown)

    /* ---------------------------------------------------------- */
    private void Awake()
    {
        // Safety check – exact 4 icons required
        if (iconRenderers.Length != 4)
        {
            Debug.LogError("Exactly 4 SpriteRenderers required!", this);
            enabled = false;
            return;
        }

        /* Build icon-names array for later comparison in SnapToNearestIcon() */
        iconNames = new string[4];
        for (int i = 0; i < 4; i++)
            iconNames[i] = iconRenderers[i].sprite.name;

        /* Pre-designed Y positions (bottom → top) */
        float[] fixedY = { 1f, -0.7f, -2.4f, -4.1f };
        iconHeight = 1.7f; // gap between icons

        /* Offset entire strip upward by 1 icon → top icon starts above viewport */
        for (int i = 0; i < 4; ++i) fixedY[i] += iconHeight;

        topY = fixedY[0];               // highest icon
        bottomY = fixedY[3] - iconHeight;  // wrap point (lowest icon - gap)

        LayoutIconsVertically(fixedY);
    }

    /* Places the 4 sprites at exact world-Y positions (Awake only) */
    private void LayoutIconsVertically(float[] yPos)
    {
        for (int i = 0; i < 4; i++)
        {
            Vector3 p = iconRenderers[i].transform.position;
            p.y = yPos[i];
            iconRenderers[i].transform.position = p;
        }
    }

    /* ---------------------------------------------------------- */
    /* Event subscription – GameManager tells us when to spin    */
    private void OnEnable() => GameManager.HandlePulled += StartRotating;
    private void OnDisable() => GameManager.HandlePulled -= StartRotating;

    /* Entry point from GameManager */
    private void StartRotating()
    {
        if (!rowStopped) return; // already spinning – ignore
        stoppedSlot = "";        // clear previous result
        StartCoroutine(Rotate());
    }

    /* ---------------------------------------------------------- */
    /* Main rotation coroutine – speed ramps down naturally        */
    private IEnumerator Rotate()
    {
        rowStopped = false;

        /* 1) Fast warm-up – gives immediate feedback */
        for (int i = 0; i < 30; i++)
        {
            StepReel();
            yield return new WaitForSeconds(Fast);
        }

        /* 2) Variable-length main spin with slowdown */
        int extraSteps = Random.Range(60, 100); // randomised so reels stop at different times
        for (int i = 0; i < extraSteps; i++)
        {
            StepReel();
            float t = Mathf.InverseLerp(0, extraSteps, i); // 0→1
            yield return new WaitForSeconds(Mathf.Lerp(Fast, Slow, t)); // smooth slowdown
        }

        /* 3) Snap to nearest icon and report result */
        SnapToNearestIcon();

        /* 4) Tell AudioManager to stop loop & play stop-clunk */
        AudioManager.Instance.StopReelScroll();

        rowStopped = true; // GameManager sees this
    }

    /* ---------------------------------------------------------- */
    /* Moves every icon downward by one icon-height               */
    private void StepReel()
    {
        foreach (var sr in iconRenderers)
            sr.transform.position += Vector3.down * iconHeight;

        /* Teleport icons that went too far down back to the top */
        foreach (var sr in iconRenderers)
        {
            if (sr.transform.position.y < bottomY - iconHeight * 0.5f)
                sr.transform.position += Vector3.up * (4 * iconHeight);
        }
    }

    /* ---------------------------------------------------------- */
    /* Finds closest icon to the centre line and snaps reel to it */
    private void SnapToNearestIcon()
    {
        // Use first icon as reference point
        float y = iconRenderers[0].transform.position.y;
        int index = Mathf.RoundToInt((topY - y) / iconHeight);
        index = Mathf.Clamp(index, 0, 3); // keep inside 0-3 range

        // Calculate how far we need to move to perfectly align
        float delta = topY - index * iconHeight - y;

        // Apply offset to entire reel
        foreach (var sr in iconRenderers)
            sr.transform.position += Vector3.up * delta;

        // Store final symbol name for GameManager evaluation
        stoppedSlot = iconNames[index];
    }
}