using System;
using System.Collections.Generic;

namespace Framework.Runtime.Utilities
{
    public abstract class Persist<T> : Persist where T : Persist, new()
    {
        protected Persist()
        {
        }

        public new T this[string key]
        {
            get
            {
                T result = (T)GetPersistance(key);
                if (result == null)
                {
                    result = new T();
                    Initialize(result, key, Provider, this);
                    SetPersistance(key, result);
                }

                return result;
            }
        }
    }

    public class Persist
    {
        public static readonly Type[] AvailableTypes =
            { typeof(int), typeof(string), typeof(float), typeof(long), typeof(bool) };

        public static string ConcatPath(string fullPath, string key)
        {
            return fullPath + '/' + key;
        }

        private readonly Dictionary<string, Persist> _paths = new Dictionary<string, Persist>();
        private bool _initialized;
        private string _key;
        private IPersistProvider _persistProvider;
        private Persist _parent;
        private string _fullPath;
        private int? _intCache;
        private long? _longCache;
        private float? _floatCache;
        private string _stringCache;
        private bool _stringHasCache;

        public Persist()
        {
        }

        public Persist(IPersistProvider persistProvider)
        {
            Initialize(this, null, persistProvider, null);
        }

        protected static Persist Initialize(Persist persistence, string key, IPersistProvider persistProvider,
            Persist parent)
        {
            if (persistence._initialized) throw new InvalidOperationException("Persist have initialized");
            persistence._key = key;
            persistence._persistProvider = persistProvider;
            persistence._parent = parent;
            if (parent != null)
            {
                persistence._fullPath = ConcatPath(parent._fullPath, key);
            }
            else
            {
                persistence._fullPath = key;
            }

            persistence._initialized = true;
            return persistence;
        }

        protected Persist GetPersistance(string key)
        {
            Persist result;
            _paths.TryGetValue(key, out result);
            return result;
        }

        protected void SetPersistance(string key, Persist value)
        {
            _paths[key] = value;
        }

        public IPersistProvider Provider
        {
            get { return _persistProvider; }
        }

        public Persist this[string key]
        {
            get
            {
                Persist result;
                if (!_paths.TryGetValue(key, out result))
                {
                    _paths[key] = result = Initialize(new Persist(), key, _persistProvider, this);
                }

                return result;
            }
        }

        public string FullPath
        {
            get { return _fullPath; }
        }

        public Persist Parent
        {
            get { return _parent; }
        }

        public string Key
        {
            get { return _key; }
        }

        public void SetDefaultValue(object value)
        {
            if (value is int)
            {
                if (!_persistProvider.HasKey(_fullPath))
                {
                    _intCache = (int)value;
                }
                else
                {
                    _intCache = _persistProvider.GetInt(_fullPath);
                }
            }
            else if (value is bool)
            {
                if (!_persistProvider.HasKey(_fullPath))
                {
                    _intCache = ((bool)value ? 1 : 0);
                }
                else
                {
                    _intCache = _persistProvider.GetInt(_fullPath);
                }
            }
            else if (value is float)
            {
                if (!_persistProvider.HasKey(_fullPath))
                {
                    _floatCache = (float)value;
                }
                else
                {
                    _floatCache = _persistProvider.GetFloat(_fullPath);
                }
            }
            else if (value is string)
            {
                if (!_persistProvider.HasKey(_fullPath))
                {
                    _stringCache = (string)value;
                    _stringHasCache = true;
                }
                else
                {
                    _stringHasCache = true;
                    _stringCache = _persistProvider.GetString(_fullPath);
                }
            }
            else if (value is long)
            {
                if (!_persistProvider.HasKey(GetRightLongPath(_fullPath)))
                {
                    _longCache = (long)value;
                }
                else
                {
                    int right = _persistProvider.GetInt(GetRightLongPath(_fullPath));
                    int left = _persistProvider.GetInt(GetLeftLongPath(_fullPath));
                    _longCache = CombineToLong((uint)left, (uint)right);
                }
            }
            else
            {
                throw new ArgumentException(
                    "type not supported: " + (value != null ? value.GetType().FullName : "null"));
            }
        }

        public T GetValue<T>()
        {
            var type = typeof(T);
            return (T)GetValue(type);
        }

        public object GetValue(Type type)
        {
            if (type == typeof(int))
            {
                return IntValue;
            }

            if (type == typeof(bool))
            {
                return BoolValue;
            }

            if (type == typeof(float))
            {
                return FloatValue;
            }

            if (type == typeof(string))
            {
                return StringValue;
            }

            if (type == typeof(long))
            {
                return LongValue;
            }

            throw new ArgumentException("type not supported: " + type.FullName);
        }

        public void SetValue(object value)
        {
            if (value == null)
            {
                IntValue = 0;
                BoolValue = false;
                FloatValue = 0;
                StringValue = null;
                LongValue = 0;
            }
            else
            {
                var type = value.GetType();
                if (type == typeof(int))
                {
                    IntValue = (int)value;
                }
                else if (type == typeof(bool))
                {
                    BoolValue = (bool)value;
                }
                else if (type == typeof(float))
                {
                    FloatValue = (float)value;
                }
                else if (type == typeof(string))
                {
                    StringValue = (string)value;
                }
                else if (type == typeof(long))
                {
                    LongValue = (long)value;
                }
                else
                {
                    throw new ArgumentException("type not supported: " + type.FullName);
                }
            }
        }

        public long LongValue
        {
            get
            {
                if (_longCache.HasValue) return _longCache.Value;
                uint left = (uint)_persistProvider.GetInt(GetLeftLongPath(_fullPath));
                uint right = (uint)_persistProvider.GetInt(GetRightLongPath(_fullPath));
                return CombineToLong(left, right);
            }
            set
            {
                _longCache = value;
                var left = GetLeft(value);
                var right = GetRight(value);
                var leftInt = (int)left;
                var rightInt = (int)right;
                _persistProvider.SetInt(GetLeftLongPath(_fullPath), leftInt);
                _persistProvider.SetInt(GetRightLongPath(_fullPath), rightInt);
                _persistProvider.Save();
            }
        }

        public bool BoolValue
        {
            get
            {
                if (_intCache.HasValue) return _intCache.Value != 0;
                return _persistProvider.GetInt(_fullPath) != 0;
            }
            set
            {
                _intCache = value ? 1 : 0;
                _persistProvider.SetInt(_fullPath, value ? 1 : 0);
                _persistProvider.Save();
            }
        }

        public int IntValue
        {
            get
            {
                if (_intCache.HasValue) return _intCache.Value;
                return _persistProvider.GetInt(_fullPath);
            }
            set
            {
                _intCache = value;
                _persistProvider.SetInt(_fullPath, value);
                _persistProvider.Save();
            }
        }

        public float FloatValue
        {
            get
            {
                if (_floatCache.HasValue) return _floatCache.Value;
                return _persistProvider.GetFloat(_fullPath);
            }
            set
            {
                _floatCache = value;
                _persistProvider.SetFloat(_fullPath, value);
                _persistProvider.Save();
            }
        }

        public string StringValue
        {
            get
            {
                if (_stringHasCache) return _stringCache;
                return _persistProvider.GetString(_fullPath);
            }
            set
            {
                _stringCache = value;
                _stringHasCache = true;
                _persistProvider.SetString(_fullPath, value);
                _persistProvider.Save();
            }
        }

        public void ClearCache()
        {
            _floatCache = null;
            _intCache = null;
            _longCache = null;
            _stringCache = null;
            _stringHasCache = false;
        }

        private static string GetLeftLongPath(string path)
        {
            return path + "@32";
        }

        private static string GetRightLongPath(string path)
        {
            return path + "@0";
        }

        private static uint GetLeft(long value)
        {
            ulong newValue = ((ulong)value >> 32);
            return (uint)newValue;
        }

        private static uint GetRight(long value)
        {
            ulong newValue = 0xFFFFFFFF & (ulong)value;
            return (uint)(newValue);
        }

        private static long CombineToLong(uint left, uint right)
        {
            return (long)(((ulong)left << 32) | (ulong)right);
        }
    }
}