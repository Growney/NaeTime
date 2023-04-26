using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Core.Security.Core
{
    public class ClaimsCollection : IReadOnlyDictionary<string, string>
    {
        private readonly ConcurrentDictionary<string, Claim> _structure = new ConcurrentDictionary<string, Claim>();

        public string this[string key]
        {
            get
            {
                var claim = _structure[key];
                return claim.Value;
            }
        }

        public IEnumerable<string> Keys => _structure.Keys;

        public IEnumerable<string> Values
        {
            get
            {
                foreach (var kvp in _structure)
                {
                    yield return kvp.Value.Value;
                }
            }
        }

        public int Count => _structure.Count;

        public ClaimsCollection(IEnumerable<Claim> claims)
        {
            foreach (var claim in claims)
            {
                _structure.AddOrUpdate(claim.Type, claim, (key, current) =>
                {
                    return claim;
                });
            }
        }

        public bool ContainsKey(string key)
        {
            return _structure.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            foreach (var kvp in _structure)
            {
                yield return new KeyValuePair<string, string>(kvp.Key, kvp.Value.Value);
            }
        }

        public bool TryGetValue(string key, out string value)
        {
            if (_structure.TryGetValue(key, out var claim))
            {
                value = claim.Value;
                return true;
            }
            else
            {
                value = string.Empty;
                return false;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<Claim> GetClaims()
        {
            return _structure.Values;
        }

        public static ClaimsCollection CreateCollection(params KeyValuePair<string, object>[] values)
        {
            var claims = new List<Claim>();
            foreach (var val in values)
            {
                claims.Add(new Claim(val.Key, val.Value.ToString()));
            }
            return new ClaimsCollection(claims);
        }
    }
}
