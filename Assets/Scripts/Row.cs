using System.Collections;
using UnityEngine;

public class Row : MonoBehaviour
{
    [Header("Icon Sprites (drag 4, bottom -> top)")]
    [SerializeField] private SpriteRenderer[] iconRenderers = new SpriteRenderer[4];

    [Header("Mechanical settings")]
    public float topY = 1.75f;          // world-Y of the top icon
    public float bottomY = -3.5f;       // world-Y the reel wraps to

    [HideInInspector] public bool rowStopped = true;
    [HideInInspector] public string stoppedSlot = "";

    /* ---------- auto-filled ---------- */
    private string[] iconNames;         // built at runtime
    private float iconHeight = 1.5f;    // vertical spacing

    private const float Fast = 0.025f;
    private const float Slow = 0.20f;

    /*----------------------------------------------------------*/

    private void Awake()
    {
        // safety
        if (iconRenderers.Length != 4)
        {
            Debug.LogError("Exactly 4 SpriteRenderers required!", this);
            enabled = false;
            return;
        }

        iconNames = new string[4];
        for (int i = 0; i < 4; i++)
            iconNames[i] = iconRenderers[i].sprite.name;

        /*  NEW: fixed sequence 1, -0.7, -2.4, -4.1  */
        float[] fixedY = {1f, -0.7f, -2.4f, -4.1f};
        iconHeight = 1.7f;                    // gap between icons

        /* shift whole strip up by one icon so the top icon is above the view */
        for (int i = 0; i < 4; ++i) fixedY[i] += iconHeight;

        topY = fixedY[0];               // 1
        bottomY = fixedY[3] - iconHeight;  // -4.1 - 1.7 = -5.8 (wrap point)

        LayoutIconsVertically(fixedY);
    }

    /* place the 4 sprites at the exact Y positions */
    private void LayoutIconsVertically(float[] yPos)
    {
        for (int i = 0; i < 4; i++)
        {
            Vector3 p = iconRenderers[i].transform.position;
            p.y = yPos[i];
            iconRenderers[i].transform.position = p;
        }
    }

    /*----------------------------------------------------------*/
    /*  everything below is UNTOUCHED – same as your old file  */
    /*----------------------------------------------------------*/

    private void OnEnable() => GameManager.HandlePulled += StartRotating;
    private void OnDisable() => GameManager.HandlePulled -= StartRotating;

    private void StartRotating()
    {
        if (!rowStopped) return;
        stoppedSlot = "";
        StartCoroutine(Rotate());
    }

    private IEnumerator Rotate()
    {
        rowStopped = false;

        /* warm-up */
        for (int i = 0; i < 30; i++) { StepReel(); yield return new WaitForSeconds(Fast); }

        /* main spin + slowdown */
        int extra = Random.Range(60, 100);
        for (int i = 0; i < extra; i++)
        {
            StepReel();
            float t = Mathf.InverseLerp(0, extra, i);
            yield return new WaitForSeconds(Mathf.Lerp(Fast, Slow, t));
        }

        SnapToNearestIcon();
        rowStopped = true;
    }

    /*----------------------------------------------------------*/

    private void StepReel()
    {
        foreach (var sr in iconRenderers)
            sr.transform.position += Vector3.down * iconHeight;

        // wrap-around
        foreach (var sr in iconRenderers)
        {
            if (sr.transform.position.y < bottomY - iconHeight * 0.5f)
                sr.transform.position += Vector3.up * (4 * iconHeight);
        }
    }

    private void SnapToNearestIcon()
    {
        // use the first icon as reference
        float y = iconRenderers[0].transform.position.y;
        int index = Mathf.RoundToInt((topY - y) / iconHeight);
        index = Mathf.Clamp(index, 0, 3);

        // snap whole reel
        float delta = topY - index * iconHeight - y;
        foreach (var sr in iconRenderers)
            sr.transform.position += Vector3.up * delta;

        stoppedSlot = iconNames[index];
    }
}