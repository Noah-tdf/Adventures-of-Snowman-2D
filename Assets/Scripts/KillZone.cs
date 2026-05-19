using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ============================================================
// Regular Kill Zone
// ============================================================
// This is used for simple death trigger areas, such as falling below
// the platform. When Purly touches this trigger, the game ends.
public class KillZone : MonoBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField] private Color debugColor = new Color(1f, 0f, 0f, 0.4f);

    private void OnTriggerEnter2D(Collider2D other)
    {
        PurlyMovement purly = other.GetComponent<PurlyMovement>();
        if (purly == null)
        {
            purly = other.GetComponentInParent<PurlyMovement>();
        }

        if (purly != null)
        {
            if (SnowmanGameManager.Instance != null)
            {
                SnowmanGameManager.Instance.HandlePurlyHit(purly);
            }
        }
    }

    private void OnDrawGizmos()
    {
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            Gizmos.color = debugColor;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(boxCollider.offset, boxCollider.size);
            Gizmos.color = new Color(debugColor.r, debugColor.g, debugColor.b, 1.0f);
            Gizmos.DrawWireCube(boxCollider.offset, boxCollider.size);
        }
    }
}

// ============================================================
// Waterfall Hazard Volume (Manual Object)
// ============================================================
// Attach this script to an empty GameObject with a BoxCollider2D (IsTrigger = true).
// This allows you to manually place, move, and resize waterfall death zones.
[RequireComponent(typeof(BoxCollider2D))]
public class WaterfallHazardVolume : MonoBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField] private Color hazardColor = new Color(0f, 0.5f, 1f, 0.4f);

    private BoxCollider2D boxCollider;
    private PurlyMovement purly;
    private Collider2D purlyCollider;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            boxCollider.isTrigger = true;
        }
    }

    private void Update()
    {
        if (boxCollider == null) return;

        if (purly == null)
        {
            purly = Object.FindAnyObjectByType<PurlyMovement>();
            purlyCollider = purly != null ? purly.GetComponent<Collider2D>() : null;
        }

        if (purly == null || purlyCollider == null) return;

        // Perform a precise intersection check between bounds
        if (boxCollider.bounds.Intersects(purlyCollider.bounds))
        {
            MeltPurly(purly);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryMelt(other);
    }

    private void TryMelt(Collider2D other)
    {
        PurlyMovement p = other.GetComponent<PurlyMovement>();
        if (p == null) p = other.GetComponentInParent<PurlyMovement>();
        
        if (p != null)
        {
            MeltPurly(p);
        }
    }

    private void MeltPurly(PurlyMovement p)
    {
        if (SnowmanGameManager.Instance != null)
        {
            SnowmanGameManager.Instance.HandlePurlyMelted(p);
        }
    }

    // This makes the hazard volume visible in the scene view for easier editing
    private void OnDrawGizmos()
    {
        if (boxCollider == null) boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            Gizmos.color = hazardColor;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(boxCollider.offset, boxCollider.size);
            
            // Draw a slightly brighter outline
            Gizmos.color = new Color(hazardColor.r, hazardColor.g, hazardColor.b, 1.0f);
            Gizmos.DrawWireCube(boxCollider.offset, boxCollider.size);
        }
    }
}

