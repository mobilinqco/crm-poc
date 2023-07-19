using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ACRM.mobile.Domain.Configuration.UserInterface;
using ACRM.mobile.Services.Contracts;

namespace ACRM.mobile.Services
{
    public class CacheService: ICacheService
    {
        private ConcurrentDictionary<string, object> _configurationCache = new ConcurrentDictionary<string, object>();
        private ConcurrentDictionary<string, Dictionary<string,Expand>> _expandCache = new ConcurrentDictionary<string, Dictionary<string, Expand>>();

        public CacheService()
        {
        }

        public void AddItem(CacheItemKeys itemKey, object itemValue)
        {
            if (_configurationCache.ContainsKey(itemKey.ToString()))
            {
                _configurationCache[itemKey.ToString()] = itemValue;
            }
            else
            {
                _configurationCache.TryAdd(itemKey.ToString(), itemValue);
            }

        }

        public object GetItem(CacheItemKeys itemKey)
        {
            if (_configurationCache.ContainsKey(itemKey.ToString()))
            {
                return _configurationCache[itemKey.ToString()];
            }
            return null;
        }

        public void ResetCache()
        {
            _configurationCache.Clear();
            _expandCache.Clear();
        }

        public void AddExpandItem(string expandName, string key, Expand expand)
        {
            if (_expandCache.ContainsKey(expandName))
            {
                if (_expandCache[expandName].ContainsKey(key))
                {
                    _expandCache[expandName][key] = expand;
                }
                else
                {
                    _expandCache[expandName].Add(key, expand);
                }
            }
            else
            {
                _expandCache.TryAdd(expandName, new Dictionary<string, Expand> { { key, expand }});
            }

        }

        public Expand GetExpandItem(string expandName, string key)
        {
            if (_expandCache.ContainsKey(expandName))
            {
                if (_expandCache[expandName].ContainsKey(key))
                {
                    return _expandCache[expandName][key];
                }
            }
            return null;
        }
    }
}
