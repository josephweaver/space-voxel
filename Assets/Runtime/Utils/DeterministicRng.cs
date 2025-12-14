// File: DeterministicRng.cs
// Unity 2022/2023 compatible
//
// Deterministic PRNG intended for gameplay generation.
// Not cryptographically secure.

using System;

namespace SpaceVoxel.Materials
{
    public struct DeterministicRng
    {
        private uint _state;

        public DeterministicRng(uint seed)
        {
            _state = seed != 0 ? seed : 0xA341316Cu;
        }

        // xorshift32
        private uint NextU32()
        {
            uint x = _state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            _state = x;
            return x;
        }

        public float Next01()
        {
            // 24-bit mantissa -> [0,1)
            uint x = NextU32();
            return (x & 0x00FFFFFFu) / 16777216f;
        }

        public float Tri01()
        {
            // (u1 + u2) / 2
            return (Next01() + Next01()) * 0.5f;
        }

        public int NextInt(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive) return minInclusive;
            uint range = (uint)(maxExclusive - minInclusive);
            uint x = NextU32();
            return (int)(minInclusive + (x % range));
        }

        public uint NextU32Public() => NextU32();

        public static uint HashCombine(uint a, uint b)
        {
            // A simple 32-bit mix. Stable and cheap.
            uint x = a + 0x9E3779B9u + (b << 6) + (b >> 2);
            x ^= x >> 16;
            x *= 0x7FEB352Du;
            x ^= x >> 15;
            x *= 0x846CA68Bu;
            x ^= x >> 16;
            return x;
        }

        public static uint HashStringToU32(string s)
        {
            if (string.IsNullOrEmpty(s)) return 0u;
            unchecked
            {
                uint h = 2166136261u; // FNV-1a
                for (int i = 0; i < s.Length; i++)
                {
                    h ^= s[i];
                    h *= 16777619u;
                }
                return h;
            }
        }
    }
}
