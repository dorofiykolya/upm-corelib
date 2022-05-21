using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Framework.Runtime.Utilities.Components
{
  [ExecuteInEditMode]
  public class FitOrthographicComponent : MonoBehaviour
  {
#pragma warning disable 649
    [SerializeField]
    private Camera _camera;
    [SerializeField]
    private Transform[] _target;
    [SerializeField]
    private float _width = 10.24f;
    [SerializeField]
    private float _height = 10.24f;
    [SerializeField]
    private float _scalePercentDelata = 1f;

    [SerializeField]
    private PersentOffset _longSizeOffsetPercent;
    [SerializeField]
    private PersentOffset _shortSizeOffsetPercent;
#pragma warning restore 649

    private float _orthographicSize;
    private int _scaledPixelWidth;
    private int _scaledPixelHeight;
    private float _heightPercent;

    private void Awake()
    {
      Resize(_width, _height);
    }

    private void Update()
    {
      if (_camera == null || _target == null || _target.Length == 0) return;

      var orthographicSize = _camera.orthographicSize;
      var scaledPixelWidth = _camera.scaledPixelWidth;
      var scaledPixelHeight = _camera.scaledPixelHeight;
      if (Math.Abs(orthographicSize - _orthographicSize) > float.Epsilon || scaledPixelWidth != _scaledPixelWidth ||
          scaledPixelHeight != _scaledPixelHeight)
      {
        _orthographicSize = orthographicSize;
        _scaledPixelWidth = scaledPixelWidth;
        _scaledPixelHeight = scaledPixelHeight;
        Resize(_width, _height);
      }
    }

    public void Resize(float width, float height)
    {
      _width = width;
      _height = height;
      Resize();
    }

    private float GetOrietationSize(int orientation)
    {
      return orientation == 0 ? _width : _height;
    }

    private PersentOffset GetOffset(bool longest)
    {
      return longest ? _longSizeOffsetPercent : _shortSizeOffsetPercent;
    }

    public void Resize()
    {
      if (_target == null || _target.Length == 0) return;

      List<float> size = new float[2] { _width, _height }.ToList();

      float width = size.Min();
      float height = size.Min();

      List<float> worldScreen = new float[2].ToList();

      worldScreen[1] = _camera.orthographicSize * 2f;
      worldScreen[0] = worldScreen[1] / _camera.scaledPixelHeight * _camera.scaledPixelWidth;

      var orientation = worldScreen.IndexOf(worldScreen.Max());

      var widthOffset = GetOffset(orientation == 0);
      var heightOffset = GetOffset(orientation == 1);

      List<float> conteinerSize = new float[2].ToList();

      conteinerSize[1] = worldScreen[1] * (1 - heightOffset.Begin - heightOffset.End);
      conteinerSize[0] = worldScreen[0] * (1 - widthOffset.Begin - widthOffset.End);

      _heightPercent = conteinerSize.Min() / size.Min();

      Vector3 imgScale = new Vector3(1f, 1f, 1f);

      Vector2 ratio = new Vector2(_width / _height, _height / _width);
      if ((conteinerSize[0] / width) < (conteinerSize[1] / height))
      {
        imgScale.x = Mathf.Min(conteinerSize[0] / width, conteinerSize[1] / Mathf.Max(height, conteinerSize[1] / _heightPercent)) + _scalePercentDelata;
        imgScale.y = imgScale.x * ratio.y;
      }
      else
      {
        imgScale.y = conteinerSize[1] / Mathf.Max(height, conteinerSize[1] / _heightPercent) + _scalePercentDelata;
        imgScale.x = imgScale.y * ratio.x;
      }

      foreach (var target in _target)
      {
        target.transform.localPosition = new Vector3((widthOffset.Begin - widthOffset.End) * worldScreen[0] / 2, -(heightOffset.Begin - heightOffset.End) * worldScreen[1] / 2, target.transform.localPosition.z);
        target.localScale = imgScale;
      }
    }

    private void OnDrawGizmos()
    {
      if (_target == null || _target.Length == 0) return;

      foreach (var target in _target)
      {
        var lastColor = Gizmos.color;
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(target.position, new Vector3(_width * target.localScale.x, _height * target.localScale.y, target.position.y));
        Gizmos.color = lastColor;
      }
    }

    [Serializable]
    public class PersentOffset
    {
      public float Begin;
      public float End;
    }
  }
}
