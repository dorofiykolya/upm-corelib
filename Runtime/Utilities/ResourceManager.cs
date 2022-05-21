using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using UnityEngine;

namespace Framework.Runtime.Utilities
{
  public class ResourceManager : IDisposable
  {
    private readonly ICoroutineProvider _coroutineProvider;
    private readonly Dictionary<string, ResourceResult> _map;

    public ResourceManager(ICoroutineProvider coroutineProvider)
    {
      _coroutineProvider = coroutineProvider;
      _map = new Dictionary<string, ResourceResult>();
    }

    public ResourceResult<T> Get<T>(string prefab) where T : UnityEngine.Object
    {
      ResourceResult result;
      if (!_map.TryGetValue(prefab, out result))
      {
        result = new ResourceResult<T>(_coroutineProvider, prefab);
        _map[prefab] = result;
      }

      return (ResourceResult<T>)result;
    }

    public void Dispose()
    {
      foreach (var value in _map.Values)
      {
        value.Dispose();
      }
    }

    public abstract class ResourceResult : IDisposable
    {
      public abstract float Progress { get; }
      public abstract bool IsCompleted { get; }
      public abstract bool IsError { get; }

      public abstract void Collect();

      public virtual void Dispose()
      {

      }
    }

    public class ResourceResult<T> : ResourceResult where T : UnityEngine.Object
    {
      private readonly ICoroutineProvider _coroutineProvider;
      private readonly string _path;
      private readonly Signal<ResourceResult<T>> _onResult;
      private T _result;
      private Coroutine _coroutine;
      private ResourceRequest _loadAsync;
      private bool _unload;
      private float _progress;
      private bool _isCompleted;
      private bool _isError;


      public ResourceResult(ICoroutineProvider coroutineProvider, string path)
      {
        _coroutineProvider = coroutineProvider;
        _path = path;
        _onResult = new Signal<ResourceResult<T>>(Lifetime.Eternal);
      }

      public override float Progress => _progress;
      public override bool IsCompleted => _isCompleted;
      public override bool IsError => _isError;
      public T Result { get { return _result; } }

      public override void Collect()
      {
        if (_result != null)
        {
          Resources.UnloadAsset(_result);
          _result = null;
        }

        _isCompleted = false;
        _isError = false;
      }

      public override void Dispose()
      {
        _unload = true;
        Collect();
      }

      public ResourceResult<T> LoadAsync(Lifetime lifetime, Action<ResourceResult<T>> onResult)
      {
        if (!lifetime.IsTerminated)
        {
          _onResult.Subscribe(lifetime, onResult);
        }
        if (IsCompleted)
        {
          FireOnResult();
        }
        else
        {
          if (_loadAsync == null)
          {
            _isCompleted = false;
            _isError = false;
            _unload = false;
            _loadAsync = Resources.LoadAsync<T>(_path);
            StopCoroutine();
            _coroutine = _coroutineProvider.StartCoroutine(LoadAsyncProcess());
          }
        }

        return this;
      }

      private IEnumerator LoadAsyncProcess()
      {
        while (!_loadAsync.isDone)
        {
          if (_unload)
          {
            _loadAsync = null;
            _result = null;
            _isCompleted = false;
            yield break;
          }
          _progress = _loadAsync.progress;
          yield return null;
        }

        _isCompleted = true;
        _result = _loadAsync.asset as T;
        _loadAsync = null;
        if (_result == null)
        {
          _isError = true;
        }
        FireOnResult();
      }

      private void StopCoroutine()
      {
        if (_coroutine != null)
        {
          _coroutineProvider.StopCoroutine(_coroutine);
        }
      }

      private void FireOnResult()
      {
        _onResult.Fire(this);
      }
    }
  }
}
