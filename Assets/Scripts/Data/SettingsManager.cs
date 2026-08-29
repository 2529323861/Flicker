using System;
using System.Collections.Generic;
using Flicker.Core;
using UnityEngine;

namespace Flicker.Data
{
    public class SettingsManager: Singleton<SettingsManager>
    {
        public void init()
        {
            RegistrySetting("player_name", ()=>{return GetRandomName();});
        }
        private Dictionary<string, Func<object>> defaultPairs=new();
        public void RegistrySetting<T>(string key, Func<T> defaultValue){
            Func<object> boxing = ()=>{return defaultValue.Invoke();};
            defaultPairs.Add(key, boxing);
        }
        public T GetSetting<T>(string key)
        {
            if (!defaultPairs.ContainsKey(key)) return default; //注册过才可以获取

            object defaultValue =  defaultPairs.GetValueOrDefault(key).Invoke();
            object value = null;
            if (defaultValue.GetType() == typeof(int))
            {                
                value = PlayerPrefs.GetInt(key, (int)defaultValue);
                PlayerPrefs.SetInt(key,(int)value);
                
            }
            if (defaultValue.GetType()==typeof(float))
            {
                value = PlayerPrefs.GetFloat(key, (float)defaultValue);
                PlayerPrefs.SetFloat(key,(float)value);
            }
            if (defaultValue.GetType()==typeof(string))
            {
                value = PlayerPrefs.GetString(key, (string)defaultValue);
                PlayerPrefs.SetString(key,(string)value);
            }
            PlayerPrefs.Save();
            return (T)value;
        }
        public void SetSetting<T>(string key, T value)
        {
            if (!defaultPairs.ContainsKey(key)) return; //注册过才可以设置
            if (typeof(T) == typeof(int))
            {                
                PlayerPrefs.SetInt(key,(int)(object)value);
                
            }
            if (typeof(T) == typeof(float))
            {
                PlayerPrefs.SetFloat(key,(float)(object)value);
            }
            if (typeof(T) == typeof(string))
            {
                PlayerPrefs.SetString(key,(string)(object)value);
            }
            PlayerPrefs.Save();
        }
        //——————————————————————————————————————————————————————————————————
        private List<string> adj = new(){"勤劳","悠闲","暴富","贪吃","迷糊","幸运","贫穷","低调","佛系","秃头","熬夜","摆烂","躺平"};
        private List<string> veg = new(){"西兰花","大白菜","卷心菜","花椰菜","胡萝卜","白萝卜","土豆","红薯", "芋头", "山药"};
        public string GetRandomName()
        {
            return adj[UnityEngine.Random.Range(0,adj.Count)] + "的" + veg[UnityEngine.Random.Range(0,veg.Count)];
        }
    }
}