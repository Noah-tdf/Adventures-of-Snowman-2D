using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;

public class BalloonManager : MonoBehaviour
{
    [SerializeField] private Sprite yellowSprite;
    [SerializeField] private Sprite blackSprite;
    
    [Header("Events")]
    public UnityEvent onCollectYellow;
    public UnityEvent onCollectBlack;

    private List<Balloon> pool = new List<Balloon>();
    private List<Vector3>[] platformPositions = new List<Vector3>[6];
    private int currentPlatformIndex = -1;
    private Transform player;
    private bool initialized = false;

    private string[] rawCoords = {
        "7.03,-0.56,-5.00|10.63,-0.45,-5.00|8.69,1.69,-5.00|1.99,2.34,-5.00|-1.83,-2.31,-5.00|-6.61,0.71,-5.00|14.69,2.30,-5.00|-2.69,0.09,-5.00",
        "48.03,3.61,-5.00|27.01,0.77,-5.00|29.10,-0.68,-5.00|18.31,1.73,-5.00|40.59,-0.03,-5.00|38.02,3.54,-5.00|35.10,-1.12,-5.00|23.00,1.44,-5.00|22.00,-2.21,-5.00|45.32,-0.16,-5.00|19.00,-1.47,-5.00|32.50,0.01,-5.00|42.48,-0.72,-5.00",
        "84.39,0.92,-5.00|68.87,0.94,-5.00|53.49,0.48,-5.00|78.82,0.46,-5.00|51.08,-1.82,-5.00|81.90,2.96,-5.00|57.24,0.34,-5.00|55.66,-1.73,-5.00|63.00,-0.38,-5.00|75.85,-0.41,-5.00|74.21,-1.17,-5.00|64.81,-1.91,-5.00|60.44,1.88,-5.00",
        "90.68,1.40,-5.00|100.06,-1.94,-5.00|96.01,-0.12,-5.00|86.35,-1.30,-5.00|117.86,2.70,-5.00|109.79,-1.30,-5.00|107.86,-0.08,-5.00|89.24,-2.48,-5.00|102.31,2.67,-5.00",
        "151.77,2.25,-5.00|133.83,2.51,-5.00|143.59,-0.23,-5.00|122.90,-2.28,-5.00|135.75,-2.31,-5.00|130.74,-0.20,-5.00|137.37,1.15,-5.00|125.99,0.43,-5.00|120.41,-0.95,-5.00|145.41,2.04,-5.00",
        "190.39,1.04,-5.00|157.34,1.32,-5.00|167.10,-1.42,-5.00|178.08,1.48,-5.00|186.18,0.62,-5.00|175.80,1.50,-5.00|154.25,-1.39,-5.00|164.08,1.81,-5.00|159.87,1.39,-5.00|168.92,0.85,-5.00|172.90,4.28,-5.00"
    };

    public void Initialize(Transform playerTransform)
    {
        player = playerTransform;
        ParseCoordinates();
        CleanupExistingBalloons();
        CreatePool();
        currentPlatformIndex = 0;
        RefreshBalloons();
        initialized = true;
    }

    private void CleanupExistingBalloons()
    {
        var allBalloons = Object.FindObjectsByType<Balloon>(FindObjectsSortMode.None);
        foreach (var b in allBalloons) 
        {
            if (b.name.StartsWith("Balloon_")) DestroyImmediate(b.gameObject);
        }
        pool.Clear();
    }

    private void ParseCoordinates()
    {
        for (int i = 0; i < 6; i++)
        {
            platformPositions[i] = new List<Vector3>();
            string[] parts = rawCoords[i].Split('|');
            foreach (string p in parts)
            {
                string[] c = p.Split(',');
                if (c.Length == 3)
                {
                    platformPositions[i].Add(new Vector3(float.Parse(c[0]), float.Parse(c[1]), float.Parse(c[2])));
                }
            }
        }
    }

    private void CreatePool()
    {
        if (yellowSprite == null) {
            var sprites = Resources.LoadAll<Sprite>("cyan-balloons");
            if (sprites.Length > 0) yellowSprite = sprites[0];
        }
        if (blackSprite == null) {
            var sprites = Resources.LoadAll<Sprite>("Black Balloon");
            if (sprites.Length > 0) blackSprite = sprites[0];
        }

        for (int i = 0; i < 6; i++)
        {
            GameObject go = new GameObject("Balloon_" + i);
            go.transform.SetParent(transform);
            go.transform.localScale = new Vector3(0.08f, 0.08f, 1f); 
            
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 1000;
            
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 4.5f; 
            var b = go.AddComponent<Balloon>();
            
            if (i == 0) 
            {
                b.SetScoreValue(-3);
                sr.sprite = blackSprite;
                sr.color = Color.white; 
                if (blackSprite == null) sr.color = Color.black; 
            }
            else 
            {
                b.SetScoreValue(1);
                sr.sprite = yellowSprite;
                sr.color = Color.yellow; 
            }
            
            pool.Add(b);
            go.SetActive(false);
        }
    }

    private void Update()
    {
        if (player == null || !initialized) return;

        int newIndex = GetPlatformIndex(player.position.x);
        if (newIndex != currentPlatformIndex && newIndex < 6)
        {
            currentPlatformIndex = newIndex;
            RefreshBalloons();
        }
    }

    private int GetPlatformIndex(float x)
    {
        float[] thresholds = { 17f, 51f, 84f, 118f, 151f };
        int index = 0;
        for (int i = 0; i < thresholds.Length; i++)
        {
            if (x > thresholds[i]) index = i + 1;
        }
        return index;
    }

    private void RefreshBalloons()
    {
        if (currentPlatformIndex < 0 || currentPlatformIndex >= 6) return;

        var availablePositions = new List<Vector3>(platformPositions[currentPlatformIndex]);
        for (int i = availablePositions.Count - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1);
            Vector3 temp = availablePositions[i];
            availablePositions[i] = availablePositions[r];
            availablePositions[r] = temp;
        }

        int posIdx = 0;
        foreach (var b in pool)
        {
            if (posIdx < availablePositions.Count)
            {
                b.transform.position = availablePositions[posIdx++];
                b.gameObject.SetActive(true);
                b.Show();
            }
            else
            {
                b.gameObject.SetActive(false);
            }
        }
    }

    public void HandleBalloonPopped(Balloon balloon)
    {
        if (balloon != null)
        {
            if (balloon.ScoreValue > 0) onCollectYellow?.Invoke();
            else onCollectBlack?.Invoke();
            balloon.Hide();
            balloon.gameObject.SetActive(false);
        }
    }

    public void ResetBalloons()
    {
        foreach (var b in pool)
        {
            if (b != null) b.gameObject.SetActive(false);
        }
        currentPlatformIndex = 0;
        if (initialized && player != null) RefreshBalloons();
    }

    public void RegisterBalloon(Balloon balloon) { }
}


