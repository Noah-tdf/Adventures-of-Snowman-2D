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
        Balloon topBalloon = balloons[0];
        Balloon bottomBalloon = balloons[0];
        Balloon leftBalloon = balloons[0];
        Balloon rightBalloon = balloons[0];

        for (int i = 0; i < balloons.Count; i++)
        {
            Vector3 position = balloons[i].GetLocalPosition();

            if (position.y > topBalloon.GetLocalPosition().y)
            {
                topBalloon = balloons[i];
            }

            if (position.y < bottomBalloon.GetLocalPosition().y)
            {
                bottomBalloon = balloons[i];
            }

            if (position.x < leftBalloon.GetLocalPosition().x)
            {
                leftBalloon = balloons[i];
            }

            if (position.x > rightBalloon.GetLocalPosition().x)
            {
                rightBalloon = balloons[i];
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
