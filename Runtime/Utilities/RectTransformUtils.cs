using UnityEngine;

namespace Framework.Runtime.Utilities
{
    public class RectTransformUtils
    {
        public static Rect GetScreenCoordinates(RectTransform transform)
        {
            Vector3[] worldCorners = new Vector3[4];
            transform.GetWorldCorners(worldCorners);
            Rect result = new Rect(
                worldCorners[0].x,
                worldCorners[0].y,
                worldCorners[2].x - worldCorners[0].x,
                worldCorners[2].y - worldCorners[0].y);
            return result;
        }
    }
}