using System;
using System.Collections.Generic;
using Flicker.Core;
using Newtonsoft.Json;
using UnityEngine;

namespace Flicker.Data
{
    public class SettingsManager: Singleton<SettingsManager>
    {
        public void init()
        {
            RegistrySetting("playerName", ()=>{return GetRandomName();});
            /** 默认卡组：固定的 7 个道具 id，序列化为 json 字符串存储。
             *  默认值函数每次 GetSetting 都会被调用一次（结果被已存值覆盖丢弃），
             *  因此这里必须是纯函数，不能随机、不能写盘 */
            RegistrySetting("pvpDeck", ()=>JsonConvert.SerializeObject(new string[]{
                "00001","00002","00003","00004","00005","00006","00007"
            }));
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
        /// <summary>
        /// 将已注册的字段恢复为默认值（默认值由注册时的委托重新生成）
        /// </summary>
        /// <param name="key">已注册的字段名</param>
        public void ResetSetting(string key)
        {
            if (!defaultPairs.ContainsKey(key)) return; //注册过才可以重置

            object defaultValue = defaultPairs.GetValueOrDefault(key).Invoke();
            if (defaultValue.GetType() == typeof(int))
            {
                PlayerPrefs.SetInt(key,(int)defaultValue);
            }
            if (defaultValue.GetType() == typeof(float))
            {
                PlayerPrefs.SetFloat(key,(float)defaultValue);
            }
            if (defaultValue.GetType() == typeof(string))
            {
                PlayerPrefs.SetString(key,(string)defaultValue);
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