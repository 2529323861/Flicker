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
            deck.Add("00002");
            deck.Add("00003");
            deck.Add("00004");
            deck.Add("00005");
            deck.Add("00006");
            deck.Add("00007");
            deck.Add("00008");
            deck.Add("00009");
            deck.Add("00010");
            deck.Add("00011");
            deck.Add("00012");
            deck.Add("00013");
            // deck.Add("00002");
            // deck.Add("00003");
        }
        /// <summary>
        /// 注入卡组，覆盖 Awake 中的默认卡组，并立即重建当前包。
        /// 传 null 或空集合时不覆盖，保留默认卡组（供调试直开等未提交卡组的场景使用）
        /// </summary>
        /// <param name="newDeck">要注入的卡组</param>
        public void SetDeck(IEnumerable<string> newDeck)
        {
            if (newDeck == null) return;
            List<string> incoming = new(newDeck);
            if (incoming.Count == 0) return;

            deck.Clear();
            deck.AddRange(incoming);
            ResetBag();
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