using UnityEngine;

[ExecuteInEditMode]
public class CelestialBody : MonoBehaviour
{
    // Quantities of motion
    public Vector3 Position
    {
        get => transform.position;
        set => transform.position = value;
    }
    [HideInInspector] public Vector3 Velocity { get; set; } = Vector3.zero;
    public float Mass { get; set; } = 1f;

    // Visual size
    //[Min(0)] public float maxRadius = 100f;
    public float radius = 1f;
    //public float Radius {
    //    get { return _radius; }
    //    set {
    //        _radius = Mathf.Min(value, maxRadius);
    //        transform.localScale = 2 * _radius * Vector3.one;
    //    }
    //}

    // Rotation of the body about its own axis
    public float RotationPeriod { get; set; } = 1f;
    //public float RotationalAngularMomentum
    //{
    //    get
    //    {
    //        float momentOfInertia = 0.4f * Mass * Radius * Radius;
    //        float angularSpeed = 2 * Mathf.PI / RotationPeriod;
    //        return momentOfInertia * angularSpeed;
    //    }
    //}

    public void SetRadius(float radius)
    {
        this.radius = radius;
        transform.localScale = 2 * this.radius * Vector3.one;
    }

    public void SetScale(Vector3 scale)
    {
        transform.localScale = scale;
    }

    public void Translate(Vector3 displacement)
    {
        Position += displacement;
    }

    public void IncrementVelocity(Vector3 deltaVelocity)
    {
        Velocity += deltaVelocity;
    }

    public void IncrementRotation(Vector3 deltaRotation)
    {
        transform.Rotate(deltaRotation);
    }

    public void SetRotation(Vector3 rotation)
    {
        transform.rotation = Quaternion.Euler(rotation);
    }
}
