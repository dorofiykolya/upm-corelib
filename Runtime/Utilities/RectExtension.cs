using System;
using UnityEngine;

namespace Framework.Runtime.Utilities
{
  public static class RectExtension
  {
    public static Rect Edit(this Rect rect, Func<Rect, Rect> action)
    {
      rect = action(rect);
      return rect;
    }
  }
}