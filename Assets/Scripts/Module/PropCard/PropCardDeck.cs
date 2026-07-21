using System.Collections.Generic;
using UnityEngine;

namespace Flicker.Module
{
    /// <summary>
    /// 卡组管理器，同时支持7bag获取卡牌
    /// </summary>
    public class PropCardDeck : MonoBehaviour
    {
        /// <summary>
        /// 卡组
        /// </summary>
        private List<string> deck = new();
        /// <summary>
        /// 当前包
        /// </summary>
        private List<string> nowBag = new();
        void Awake()
        {
            /** 暂时令deck为固定对象 */
            deck.Add("00001");
            // deck.Add("00002");
            // deck.Add("00003");
        }
        /// <summary>
        /// 重置当前包
        /// </summary>
        public void ResetBag()
        {
            nowBag = new();
            foreach (var i in deck)
            {
                nowBag.Add(i);
            }
            for (int i = 0; i < deck.Count*2; i++)
            {
                int p = Random.Range(0,deck.Count);
                int q = Random.Range(0,deck.Count);
                var temp = nowBag[p];
                nowBag[p] = nowBag[q];
                nowBag[q] = temp;
            }
        }
        /// <summary>
        /// 获取道具
        /// </summary>
        /// <returns></returns>
        public BasePropStrategy GetProp()
        {
            if (nowBag.Count==0) ResetBag();
            if (nowBag.Count==0) return null;
            var result = nowBag[0];
            nowBag.RemoveAt(0);
            return PropFactory.Instance.GetProp(result);
        }
    }
}