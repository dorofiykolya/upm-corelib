﻿using System;
using Common;

namespace Framework.Runtime.Utilities
{
  public class ValueSubscriber<T> where T : IEquatable<T>
  {
    public static implicit operator T(ValueSubscriber<T> value)
    {
      return value.Current;
    }


    private readonly Signal<ValueSubscriber<T>> _onChange;
    private T _current;
    private T _prev;

    public ValueSubscriber(Lifetime lifetime, T defaultValue = default(T))
    {
      _onChange = new Signal<ValueSubscriber<T>>(lifetime);
      _current = _prev = defaultValue;
    }

    public virtual T Current
    {
      get { return GetValue(); }
      set
      {
        SetValue(value);
      }
    }

    public T Prev
    {
      get { return _prev; }
    }

    public void ForceFire()
    {
      _onChange.Fire(this);
    }
    
    public virtual void SubscribeOnChange(Lifetime lifetime, Action<ValueSubscriber<T>> listener)
    {
      _onChange.Subscribe(lifetime, listener);
    }

    protected virtual void SetValue(T value)
    {
      var last = _current;
      if (!ReferenceEquals(last, value) && (ReferenceEquals(last, null) || !last.Equals(value)))
      {
        _prev = last;
        _current = value;
        _onChange.Fire(this);
      }
    }

    protected virtual T GetValue()
    {
      return _current;
    }
  }
}
