using System.Collections;
using UnityEngine;

namespace Framework.Runtime.Utilities
{
  public interface ICoroutineProvider
  {
    Coroutine StartCoroutine(IEnumerator enumerator);
    void StopCoroutine(Coroutine coroutine);
  }
}
