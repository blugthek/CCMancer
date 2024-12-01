using Godot;
using System;

public static class Util
{
    // Clamp a Vector2 magnitude to a maximum value
    public static Vector2 ClampMagnitude(Vector2 vector, float maxLength)
    {
        return vector.Length() > maxLength ? vector.Normalized() * maxLength : vector;
    }

    // Get the angle between two vectors (in degrees)
    public static float AngleBetween(Vector2 from, Vector2 to)
    {
        return Mathf.RadToDeg(from.AngleTo(to));
    }

    // Smoothly move a vector towards a target
    public static Vector2 MoveTowards(Vector2 current, Vector2 target, float maxDistanceDelta)
    {
        var delta = target - current;
        return delta.Length() <= maxDistanceDelta ? target : current + delta.Normalized() * maxDistanceDelta;
    }

    // Ease-in and ease-out function
    public static float EaseInOut(float t)
    {
        return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
    }

    // Map a value from one range to another
    public static float MapRange(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        return toMin + (value - fromMin) * (toMax - toMin) / (fromMax - fromMin);
    }

    // Linear interpolation between two values
    public static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * Mathf.Clamp(t, 0, 1);
    }
    public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
    {
        return a + (b - a) * t;
    }

    public static float DistanceTo(this Vector2 a, Vector2 b)
    {
        return Mathf.Sqrt(Mathf.Pow(b.X - a.X, 2) + Mathf.Pow(b.Y - a.Y, 2));
    }

    public static Vector2 Normalized(this Vector2 vector)
    {
        float length = vector.Length();
        return length > 0 ? vector / length : Vector2.Zero;
    }

    public static (Vector2 leftPoint, Vector2 rightPoint) CalculateLeftRightPoints(Vector2 position, Vector2 lookAt, float distance)
    {
        // Step 1: Calculate the forward direction
        Vector2 forwardDirection = Normalized(lookAt - position);

        // Step 2: Calculate the left and right directions (rotated by ±90 degrees)
        Vector2 leftDirection = new Vector2(-forwardDirection.Y, forwardDirection.X);   // 90-degree rotation
        Vector2 rightDirection = new Vector2(forwardDirection.Y, -forwardDirection.X);  // -90-degree rotation

        // Step 3: Calculate the left and right points at the specified distance
        Vector2 leftPoint = position + leftDirection * distance;
        Vector2 rightPoint = position + rightDirection * distance;

        return (leftPoint, rightPoint);
    }

    private static readonly Random _random = new Random();

    // Get a random float between two values
    public static float RandomRange(float min, float max)
    {
        return (float)(_random.NextDouble() * (max - min) + min);
    }

    // Choose a random element from an array
    public static T ChooseRandom<T>(T[] array)
    {
        return array[_random.Next(array.Length)];
    }

    // Instantiates a scene by path
    public static T LoadScene<T>(string scenePath) where T : Node
    {
        var packedScene = GD.Load<PackedScene>(scenePath);
        return packedScene?.Instantiate<T>();
    }

    // Find a child node of a specific type in a recursive manner
    public static T FindNodeInChildren<T>(Node parentNode) where T : Node
    {
        foreach (Node child in parentNode.GetChildren())
        {
            if (child is T target)
                return target;

            var recursiveResult = FindNodeInChildren<T>(child);
            if (recursiveResult != null)
                return recursiveResult;
        }
        return null;
    }


}

