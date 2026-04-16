using UnityEngine;

public class PurlyMovementModel : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public float RotationInput { get; private set; }

    public void SetMoveInput(Vector2 newMoveInput)
    {
        MoveInput = Vector2.ClampMagnitude(newMoveInput, 1f);
    }

    public void SetRotationInput(float newRotationInput)
    {
        RotationInput = Mathf.Clamp(newRotationInput, -1f, 1f);
    }
}
