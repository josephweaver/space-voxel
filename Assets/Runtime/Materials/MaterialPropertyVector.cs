// File: MaterialPropertyVector.cs
// Unity 2022/2023 compatible

using System;
using UnityEngine;

namespace SpaceVoxel.Materials
{
    /// <summary>
    /// Tier-agnostic normalized property vector. Values are in [0,1].
    /// Stored as a fixed array to keep it simple and deterministic.
    /// </summary>
    [Serializable]
    public struct MaterialPropertyVector
    {
        [SerializeField] private float[] _v; // length = 8

        public static MaterialPropertyVector CreateZero()
        {
            var vec = new MaterialPropertyVector { _v = new float[MaterialPropertyCount.Count] };
            return vec;
        }

        public float this[MaterialProperty p]
        {
            get => _v[(int)p];
            set => _v[(int)p] = value;
        }

        public float Get(int idx) => _v[idx];
        public void Set(int idx, float value) => _v[idx] = value;

        public void EnsureInit()
        {
            if (_v == null || _v.Length != MaterialPropertyCount.Count)
                _v = new float[MaterialPropertyCount.Count];
        }

        public void Clamp01()
        {
            EnsureInit();
            for (int i = 0; i < _v.Length; i++)
                _v[i] = Mathf.Clamp01(_v[i]);
        }

        public void Add(MaterialPropertyVector other)
        {
            EnsureInit();
            other.EnsureInit();
            for (int i = 0; i < _v.Length; i++)
                _v[i] += other._v[i];
        }

        public float Max()
        {
            EnsureInit();
            float m = _v[0];
            for (int i = 1; i < _v.Length; i++) m = Mathf.Max(m, _v[i]);
            return m;
        }

        public float Min()
        {
            EnsureInit();
            float m = _v[0];
            for (int i = 1; i < _v.Length; i++) m = Mathf.Min(m, _v[i]);
            return m;
        }

        public override string ToString()
        {
            EnsureInit();
            return $"S={_v[0]:0.00}, Tm={_v[1]:0.00}, Kt={_v[2]:0.00}, Ke={_v[3]:0.00}, Er={_v[4]:0.00}, Cr={_v[5]:0.00}, D={_v[6]:0.00}, Mf={_v[7]:0.00}";
        }
    }
}
