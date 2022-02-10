using UnityEngine;

public class CircleArc : MonoBehaviour
{
    public LineRenderer line;
    public float radius = 1f;
    [Min(0)] public float angle = 0f;
    [Range(0.1f, 45), Tooltip("Angular resolution in degrees")]
    public float resolution = 1f;
    public Color color = Color.black;

    private int currentNumSegments = 0;

    private void OnValidate()
    {
        Redraw();
    }

    public void Redraw()
    {
        if (line == null)
        {
            Debug.Log("No LineRenderer assigned");
            return;
        }

        if (line.startColor != color || line.endColor != color)
        {
            line.startColor = color;
            line.endColor = color;
        }

        //int maxNumSegments = Mathf.FloorToInt(360f / resolution);
        angle = Mathf.Clamp(angle, 0, 360);
        int newNumSegments = Mathf.FloorToInt(angle / resolution);

        if (newNumSegments != currentNumSegments)
        {
            line.positionCount = (newNumSegments == 0) ? 0 : newNumSegments + 1;
            currentNumSegments = newNumSegments;
            if (newNumSegments == 0)
            {
                return;
            }

            Vector3[] positions = new Vector3[line.positionCount];
            positions[0] = radius * Vector3.right;
            for (int i = 1; i < line.positionCount; i++)
            {
                float theta = i * resolution * Mathf.Deg2Rad;
                positions[i] = radius * new Vector3(Mathf.Cos(theta), Mathf.Sin(theta), 0);
            }
            line.SetPositions(positions);
        }
        line.loop = angle == 360;
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    float theta = angle * Mathf.Deg2Rad;
    //    Gizmos.DrawLine(Vector3.zero, radius * new Vector3(Mathf.Cos(theta), Mathf.Sin(theta), 0));
    //}
}
