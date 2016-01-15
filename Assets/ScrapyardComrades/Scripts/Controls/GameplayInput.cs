using UnityEngine;

public static class GameplayInput
{
    //TODO - Calculates things like axes in pre update?
    public static Vector2 GetMovementAxis(bool normalized = false)
    {
        Vector2 movementAxis = new Vector2();

        if (Input.anyKey)
        {
            // Construct movment axis from 4-directional keyboard input
            float x = 0;
            float y = 0;

            if (Input.GetKey(KeyCode.W)) y += 1;
            if (Input.GetKey(KeyCode.A)) x -= 1;
            if (Input.GetKey(KeyCode.S)) y -= 1;
            if (Input.GetKey(KeyCode.D)) x += 1;

            if (y != 0 && x != 0)
            {
                x = Mathf.Sign(x) * 0.70710678f;
                y = Mathf.Sign(y) * 0.70710678f;
            }

            movementAxis.x = x;
            movementAxis.y = y;
        }

        if (movementAxis.x == 0.0f && movementAxis.y == 0.0f)
        {
            // Use controller input
            movementAxis.x = Input.GetAxis("Horizontal");
            movementAxis.y = Input.GetAxis("Vertical");

            if (normalized)
                movementAxis.Normalize();
        }

        return movementAxis;
    }

    public static Vector2 GetAimingAxis(bool normalized = true)
    {
        Vector2 aimAxis = new Vector2();

        if (Input.anyKey)
        {
            // Construct movment axis from 4-directional keyboard input
            float x = 0;
            float y = 0;

            if (Input.GetKey(KeyCode.UpArrow)) y += 1;
            if (Input.GetKey(KeyCode.LeftArrow)) x -= 1;
            if (Input.GetKey(KeyCode.DownArrow)) y -= 1;
            if (Input.GetKey(KeyCode.RightArrow)) x += 1;

            if (y != 0 && x != 0)
            {
                x = Mathf.Sign(x) * 0.70710678f;
                y = Mathf.Sign(y) * 0.70710678f;
            }

            aimAxis.x = x;
            aimAxis.y = y;
        }

        if (aimAxis.x == 0.0f && aimAxis.y == 0.0f)
        {
            // Use controller input
            aimAxis.x = Input.GetAxis("Horizontal 2");
            aimAxis.y = Input.GetAxis("Vertical 2");

            if (normalized)
                aimAxis.Normalize();
        }

        return aimAxis;
    }

    public static bool GetFireButton()
    {
        return Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.RightArrow) || Input.GetAxis("Fire1") != 0.0f;
    }

    public static bool JumpStarted()
    {
        return Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Jump");
    }

    public static bool Jump()
    {
        return Input.GetKey(KeyCode.Space) || Input.GetButton("Jump");
    }

    public static bool AttackStarted()
    {
        return Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl) || Input.GetButtonDown("Attack");
    }
}
