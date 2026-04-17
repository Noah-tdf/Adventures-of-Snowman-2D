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
    private readonly List<Balloon> balloons = new List<Balloon>();
    private bool wallsAssigned;
    private float leftWallX;
    private float rightWallX;
    private float topWallY;
    private float bottomWallY;
    private float minX;
    private float maxX;
    private float minY;
    private float maxY;

    public void ResetBalloons()
    {
        balloons.Clear();
        wallsAssigned = false;
        leftWallX = 0f;
        rightWallX = 0f;
        topWallY = 0f;
        bottomWallY = 0f;
        minX = 0f;
        maxX = 0f;
        minY = 0f;
        maxY = 0f;
    }

    public void RegisterBalloon(Balloon balloon)
    {
        if (balloon == null || balloons.Contains(balloon))
        {
            return;
        }

        balloons.Add(balloon);

        if (balloons.Count >= 4 && !wallsAssigned)
        {
            AssignWallsFromStartingPositions();
        }
    }

    public void HandleBalloonPopped(Balloon balloon)
    {
        if (balloon == null || !wallsAssigned)
        {
            return;
        }

        balloon.Hide();
        StartCoroutine(RespawnBalloon(balloon));
    }

    private IEnumerator RespawnBalloon(Balloon balloon)
    {
        yield return new WaitForSeconds(2f);

        if (balloon == null)
        {
            yield break;
        }

        Vector3 respawnPosition = GetRandomWallPosition(balloon.WallSide);
        balloon.SetLocalPosition(respawnPosition);
        balloon.Show();
        Debug.Log($"Balloon respawned on {balloon.WallSide} wall at {respawnPosition}");
    }

    private void AssignWallsFromStartingPositions()
    {
        List<Balloon> remainingBalloons = new List<Balloon>(balloons);
        Balloon topBalloon = remainingBalloons[0];
        Balloon bottomBalloon = remainingBalloons[0];

        for (int i = 1; i < remainingBalloons.Count; i++)
        {
            Vector3 position = remainingBalloons[i].GetLocalPosition();

            if (position.y > topBalloon.GetLocalPosition().y)
            {
                topBalloon = remainingBalloons[i];
            }

            if (position.y < bottomBalloon.GetLocalPosition().y)
            {
                bottomBalloon = remainingBalloons[i];
            }
        }

        remainingBalloons.Remove(topBalloon);
        if (bottomBalloon != topBalloon)
        {
            remainingBalloons.Remove(bottomBalloon);
        }

        Balloon leftBalloon;
        Balloon rightBalloon;

        if (remainingBalloons.Count >= 2)
        {
            leftBalloon = remainingBalloons[0].GetLocalPosition().x <= remainingBalloons[1].GetLocalPosition().x
                ? remainingBalloons[0]
                : remainingBalloons[1];
            rightBalloon = leftBalloon == remainingBalloons[0] ? remainingBalloons[1] : remainingBalloons[0];
        }
        else
        {
            leftBalloon = balloons[0];
            rightBalloon = balloons[0];

            for (int i = 0; i < balloons.Count; i++)
            {
                if (balloons[i].GetLocalPosition().x < leftBalloon.GetLocalPosition().x)
                {
                    leftBalloon = balloons[i];
                }

                if (balloons[i].GetLocalPosition().x > rightBalloon.GetLocalPosition().x)
                {
                    rightBalloon = balloons[i];
                }
            }
        }

        topBalloon.AssignWall(BalloonWall.Top);
        bottomBalloon.AssignWall(BalloonWall.Bottom);
        leftBalloon.AssignWall(BalloonWall.Left);
        rightBalloon.AssignWall(BalloonWall.Right);

        topWallY = topBalloon.GetLocalPosition().y;
        bottomWallY = bottomBalloon.GetLocalPosition().y;
        leftWallX = leftBalloon.GetLocalPosition().x;
        rightWallX = rightBalloon.GetLocalPosition().x;

        minX = leftWallX;
        maxX = rightWallX;
        minY = bottomWallY;
        maxY = topWallY;
        wallsAssigned = true;
    }

    private Vector3 GetRandomWallPosition(BalloonWall wall)
    {
        const float horizontalPadding = 1.5f;
        const float verticalPadding = 1.25f;

        switch (wall)
        {
            case BalloonWall.Left:
                return new Vector3(leftWallX, Random.Range(minY + verticalPadding, maxY - verticalPadding), -5f);
            case BalloonWall.Right:
                return new Vector3(rightWallX, Random.Range(minY + verticalPadding, maxY - verticalPadding), -5f);
            case BalloonWall.Top:
                return new Vector3(Random.Range(minX + horizontalPadding, maxX - horizontalPadding), topWallY, -5f);
            default:
                return new Vector3(Random.Range(minX + horizontalPadding, maxX - horizontalPadding), bottomWallY, -5f);
        }
    }
}
