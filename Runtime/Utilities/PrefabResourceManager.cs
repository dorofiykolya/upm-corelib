﻿using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework.Runtime.Utilities
{
  public class PrefabResourceManager : IDisposable
  {
    protected readonly ICoroutineProvider _coroutineProvider;
    private readonly Dictionary<string, ResourceResult> _map;

    public PrefabResourceManager(ICoroutineProvider coroutineProvider, Lifetime lifetime)
    {
      _coroutineProvider = coroutineProvider;
      _map = new Dictionary<string, ResourceResult>();
    }

    public ResourceResult GetPrefab(string prefab)
    {
      ResourceResult result;
      
      if (!_map.TryGetValue(prefab, out result))
      {
        result = InstantiateResourceResult(prefab);
        _map[prefab] = result;
      }
      
      return result;
    }

    protected virtual ResourceResult InstantiateResourceResult(string prefab)
    {
      return new ResourceResult(_coroutineProvider, prefab);
    }

    public void Collect()
    {
      foreach (var resource in _map.Values)
      {
        resource.Collect();
      }
    }

    public void Dispose()
    {
      foreach (var value in _map.Values)
      {
        value.Dispose();
      }
    }

    public struct ResourceLoadProgress
    {
      public string PrefabPath;
      public float Progress;
    }

    public class ResourceResult : IDisposable
    {
      protected string _prefab;
      
      private readonly List<GameObject> _instances;
      private readonly Stack<GameObject> _pool;
      private readonly ICoroutineProvider _coroutineProvider;
      private readonly Signal<ResourceResult> _onResult;
      private GameObject _prefabResult;
      private Coroutine _coroutine;
      private ResourceRequest _loadAsync;
      private bool _unload;
      private float _progress;

      public ResourceResult(ICoroutineProvider coroutineProvider, string prefab)
      {
        _instances = new List<GameObject>();
        _pool = new Stack<GameObject>();
        _coroutineProvider = coroutineProvider;
        _prefab = prefab;
        _onResult = new Signal<ResourceResult>(Lifetime.Eternal);
      }

      public float Progress { get { return _progress; } }
      public bool IsCompleted { get; private set; }
      public bool IsError { get; private set; }
      public int Instances { get { return _instances.Count; } }
      public GameObject Prefab { get { return _prefabResult; } }
      public string PrefabName => _prefab;

      public T Instantiate<T>() where T : Component
      {
        T instance;
        if (_pool.Count != 0)
        {
          instance = _pool.Pop().GetComponent<T>();
        }
        else
        {
          var prefab = GetPrefab();
          if (prefab == null)
          {
            throw new InvalidOperationException($"The Object you want to instantiate is null., prefab path: {_prefab}");
          }
          instance = Object.Instantiate(prefab).GetComponent<T>();
        }
        _instances.Add(instance.gameObject);
        return instance;
      }

      public GameObject Instantiate()
      {
        GameObject instance;
        if (_pool.Count != 0)
        {
          instance = _pool.Pop();
        }
        else
        {
          var prefab = GetPrefab();
          if (prefab == null)
          {
            throw new InvalidOperationException($"The Object you want to instantiate is null., prefab path: {_prefab}");
          }
          instance = Object.Instantiate(prefab);
        }
        _instances.Add(instance);
        return instance;
      }
      
      public T Instantiate<T>(Transform transform) where T : Component
      {
        return (T)Instantiate(typeof(T), transform);
      }

      public Component Instantiate(Type type, Transform transform)
      {
        Component instance;
        if (_pool.Count != 0)
        {
          instance = _pool.Pop().GetComponent(type);
          instance.transform.SetParent(transform, false);
        }
        else
        {
          var prefab = GetPrefab();
          if (prefab == null)
          {
            throw new InvalidOperationException($"The Object you want to instantiate is null., prefab path: {_prefab}");
          }
          instance = ((GameObject)Object.Instantiate((Object) prefab, transform, false)).GetComponent(type);
        }
        _instances.Add(instance.gameObject);
        return instance;
      }

      public GameObject Instantiate(Transform transform)
      {
        GameObject instance;
        if (_pool.Count != 0)
        {
          instance = _pool.Pop();
          instance.transform.SetParent(transform, false);
        }
        else
        {
          var prefab = GetPrefab();
          if (prefab == null)
          {
            throw new InvalidOperationException($"The Object you want to instantiate is null., prefab path: {_prefab}");
          }
          instance = Object.Instantiate(prefab, transform, false);
        }
        _instances.Add(instance.gameObject);
        return instance;
      }

      public void Collect()
      {
        foreach (var gameObject in _pool)
        {
          if (gameObject != null)
          {
            Object.Destroy(gameObject);
          }
        }
        _pool.Clear();
      }

      public bool IsInstantiated(GameObject value)
      {
        return _instances.Contains(value);
      }

      public bool IsReleased(GameObject gameObject)
      {
        return _pool.Contains(gameObject);
      }

      public void Release(GameObject value)
      {
        if (_unload) return;
        if (value == null)
        {
          throw new NullReferenceException("value can't bee null:" + _prefab);
        }
        if (_pool.Contains(value))
        {
          throw new ArgumentException("pool contains this gameObject: " + _prefab);
        }
        if (!_instances.Contains(value))
        {
          throw new ArgumentException("gameObject not in instance list: " + _prefab + ", value in pool: " + (_pool.Contains(value)));
        }
        _instances.Remove(value);

        _pool.Push(value);
      }

      public void Release(Component value)
      {
        Release(value.gameObject);
      }

      public void Dispose()
      {
        _unload = true;
        if (_instances.Count != 0)
        {
          foreach (var gameObject in _instances)
          {
            Object.Destroy(gameObject);
          }
          _instances.Clear();
        }
        if (_pool.Count != 0)
        {
          foreach (var gameObject in _pool)
          {
            Object.Destroy(gameObject);
          }
          _pool.Clear();
        }
      }

      public ResourceResult Load()
      {
        var prefab = GetPrefab();
        if (prefab != null)
        {
          IsCompleted = true;
        }
        else
        {
          IsError = true;
        }

        return this;
      }

      public virtual ResourceResult LoadAsync(Lifetime lifetime, Action<ResourceResult> onResult)
      {
        SubscribeResult(lifetime, onResult);
        
        if (IsCompleted)
        {
          FireOnResult();
        }
        else
        {
          if (_loadAsync == null)
          {
            IsCompleted = false;
            IsError = false;
            _unload = false;
            _loadAsync = Resources.LoadAsync<GameObject>(_prefab);
            StopCoroutine();
            _coroutine = _coroutineProvider.StartCoroutine(LoadAsyncProcess());
          }
        }

        return this;
      }

      protected void SubscribeResult(Lifetime lifetime, Action<ResourceResult> onResult)
      {
        if (!lifetime.IsTerminated)
        {
          _onResult.Subscribe(lifetime, onResult);
        }
      }

      private IEnumerator LoadAsyncProcess()
      {        
        while (!_loadAsync.isDone)
        {
          if (_unload)
          {
            _loadAsync = null;
            
            Debug.Log("ResourceResult.LoadAsyncProcess set null to prefab");
            _prefabResult = null;
            IsCompleted = false;
            yield break;
          }
          _progress = _loadAsync.progress;
          yield return null;
        }
        
        HandleComplete(_loadAsync.asset as GameObject);
        
        _loadAsync = null;
        if (_prefabResult == null)
        {
          HandleError();
          
        }
        FireOnResult();
      }

      protected void HandleComplete(GameObject prefab)
      {
        IsCompleted = true;
        _prefabResult = prefab;
        
        Debug.Log($"ResourceResult.HandleComplete {Prefab}");
      }

      protected void HandleError()
      {
        IsError = true;
      }

      private void StopCoroutine()
      {
        if (_coroutine != null)
        {
          _coroutineProvider.StopCoroutine(_coroutine);
        }
      }

     protected void FireOnResult()
      {
        _onResult.Fire(this);
      }

      private GameObject GetPrefab()
      {
        return _prefabResult ?? (_prefabResult = Resources.Load<GameObject>(_prefab));
      }

      public override string ToString()
      {
        return $"[Resource Name {PrefabName}, Prefab {Prefab}, Completed {IsCompleted}, Error {IsError}]";
      }
    }
  }
}