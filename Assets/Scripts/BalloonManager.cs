using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BalloonWall
{
    Left,
    Right,
    Top,
    Bottom
}

public class BalloonManager : MonoBehaviour
{
    // Each zone is xMin, xMax, yMin, yMax in world space.
    // The zones sit above the rock/platform tops so balloons do not appear under the rocks.
    private static readonly Vector4[] SafePlatformSpawnZones =
    {
        new Vector4(-4.85f, -4.05f, 1.25f, 2.05f),
        new Vector4(-1.55f, -0.75f, 0.55f, 1.25f),
        new Vector4(0.95f, 2.25f, 0.55f, 1.3f),
        new Vector4(2.85f, 4.2f, 0.55f, 1.35f),
        new Vector4(6.25f, 7.35f, 2.1f, 2.85f),
        new Vector4(7.9f, 8.8f, 0.55f, 1.35f)
    };

    private readonly List<Balloon> balloons = new List<Balloon>();
    private readonly Dictionary<Balloon, int> occupiedSpawnZones = new Dictionary<Balloon, int>();
    private bool spawnZonesReady;
    private int lastSpawnZoneIndex = -1;

    public void ResetBalloons()
    {
        balloons.Clear();
        occupiedSpawnZones.Clear();
        spawnZonesReady = false;
        lastSpawnZoneIndex = -1;
    }

    public void RegisterBalloon(Balloon balloon)
    {
        if (balloon == null || balloons.Contains(balloon))
        {
            return;
        }

        balloons.Add(balloon);

        if (balloons.Count >= 4 && !spawnZonesReady)
        {
            spawnZonesReady = true;
            MoveAllBalloonsToSpawnZones();
        }
    }

    public void HandleBalloonPopped(Balloon balloon)
    {
        if (balloon == null || !spawnZonesReady)
        {
            return;
        }

        balloon.Hide();
        occupiedSpawnZones.Remove(balloon);
        StartCoroutine(RespawnBalloon(balloon));
    }

    private IEnumerator RespawnBalloon(Balloon balloon)
    {
        yield return new WaitForSeconds(2f);

        if (balloon == null)
        {
            yield break;
        }

        int spawnZoneIndex = GetRandomAvailableSpawnZoneIndex(balloon);
        Vector3 respawnPosition = GetRandomPositionInsideZone(spawnZoneIndex);
        occupiedSpawnZones[balloon] = spawnZoneIndex;
        SetBalloonWorldPosition(balloon, respawnPosition);
        balloon.Show();
        Debug.Log($"Balloon respawned on platform at {respawnPosition}");
    }

    private void MoveAllBalloonsToSpawnZones()
    {
        List<int> availableSpawnZoneIndexes = new List<int>();
        for (int i = 0; i < SafePlatformSpawnZones.Length; i++)
        {
            availableSpawnZoneIndexes.Add(i);
        }

        for (int i = 0; i < balloons.Count; i++)
        {
            int spawnZoneIndex;

            if (availableSpawnZoneIndexes.Count > 0)
            {
                int availableIndex = Random.Range(0, availableSpawnZoneIndexes.Count);
                spawnZoneIndex = availableSpawnZoneIndexes[availableIndex];
                availableSpawnZoneIndexes.RemoveAt(availableIndex);
            }
            else
            {
                spawnZoneIndex = Random.Range(0, SafePlatformSpawnZones.Length);
            }

            SetBalloonWorldPosition(balloons[i], GetRandomPositionInsideZone(spawnZoneIndex));
            occupiedSpawnZones[balloons[i]] = spawnZoneIndex;
            balloons[i].Show();
        }
    }

    private int GetRandomAvailableSpawnZoneIndex(Balloon balloonToPlace)
    {
        List<int> availableSpawnZoneIndexes = new List<int>();
        for (int i = 0; i < SafePlatformSpawnZones.Length; i++)
        {
            if (!IsSpawnZoneOccupiedByAnotherBalloon(i, balloonToPlace))
            {
                availableSpawnZoneIndexes.Add(i);
            }
        }

        if (availableSpawnZoneIndexes.Count > 0)
        {
            int availableIndex = Random.Range(0, availableSpawnZoneIndexes.Count);
            int selectedZone = availableSpawnZoneIndexes[availableIndex];
            lastSpawnZoneIndex = selectedZone;
            return selectedZone;
        }

        return GetRandomSpawnZoneIndex();
    }

    private int GetRandomSpawnZoneIndex()
    {
        int spawnZoneIndex = Random.Range(0, SafePlatformSpawnZones.Length);

        if (SafePlatformSpawnZones.Length > 1)
        {
            int attempts = 0;
            while (spawnZoneIndex == lastSpawnZoneIndex && attempts < 8)
            {
                spawnZoneIndex = Random.Range(0, SafePlatformSpawnZones.Length);
                attempts++;
            }
        }

        lastSpawnZoneIndex = spawnZoneIndex;
        return spawnZoneIndex;
    }

    private bool IsSpawnZoneOccupiedByAnotherBalloon(int spawnZoneIndex, Balloon balloonToPlace)
    {
        foreach (KeyValuePair<Balloon, int> occupiedSpawnZone in occupiedSpawnZones)
        {
            if (occupiedSpawnZone.Key == null || occupiedSpawnZone.Key == balloonToPlace)
            {
                continue;
            }

            if (occupiedSpawnZone.Value == spawnZoneIndex)
            {
                return true;
            }
        }

        return false;
    }

    private static Vector3 GetRandomPositionInsideZone(int spawnZoneIndex)
    {
        Vector4 zone = SafePlatformSpawnZones[spawnZoneIndex];
        return new Vector3(
            Random.Range(zone.x, zone.y),
            Random.Range(zone.z, zone.w),
            -5f);
    }

    private static void SetBalloonWorldPosition(Balloon balloon, Vector3 worldPosition)
    {
        if (balloon == null)
        {
            return;
        }

        if (balloon.transform.parent != null)
        {
            balloon.SetLocalPosition(balloon.transform.parent.InverseTransformPoint(worldPosition));
        }
        else
        {
            balloon.transform.position = worldPosition;
        }
    }
}
