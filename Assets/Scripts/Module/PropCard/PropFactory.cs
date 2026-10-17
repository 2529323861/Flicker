using System;
using System.Collections.Generic;
using Flicker.Core;
using Flicker.Module.Prop;
using Flicker.Module.UIEvent;

namespace Flicker.Module
{
    public class PropFactory: Singleton<PropFactory>
    {
        /// <summary>
        /// 道具元信息缓存，null = 尚未构建。构建一次后复用
        /// </summary>
        List<BasePropStrategyMetaData> metaDataCache;
        private Dictionary<string, Func<BasePropStrategy>> props = new();
        /// <summary>
        /// 黑名单：不参与大厅卡组构筑的道具。暂时魔法数字写在这里，后续改为数据层配置
        /// </summary>
        private static readonly HashSet<string> pvpBlacklist = new(){"00000"};
        protected override void OnSingletonInit()
        {
            RegisterAllProps();
        }
        public void Init()
        {
            EventBus.Subscribe<PropPoolRequestEvent>(PropPoolRequestHandler);
        }
        public void RegisterAllProps()
        {
            props = new();
            metaDataCache = null;
            RegisterProp("00000",()=>new BasePropStrategy());
            RegisterProp("00001",()=>new RushGelProp());
            RegisterProp("00002",()=>new EmergenceSporeProp());
            RegisterProp("00003",()=>new ReverseFlowHourglassProp());
            RegisterProp("00004",()=>new NodeviationArrowProp());
            RegisterProp("00005",()=>new CompensationVoucherProp());
            RegisterProp("00006",()=>new NamelessHaloProp());
            RegisterProp("00007",()=>new PulseClockProp());
            RegisterProp("00008",()=>new StaticShellProp());
            RegisterProp("00009",()=>new LightBoneProp());
            RegisterProp("00010",()=>new LeadStabilizerProp());
            RegisterProp("00011",()=>new IdleShaftProp());
            RegisterProp("00012",()=>new WindBellProp());
            RegisterProp("00013",()=>new AnchorClawProp());

        }
        public void RegisterProp(string id, Func<BasePropStrategy> func)
        {
            props.Add(id,func);
            /** 注册表变动，元信息缓存作废 */
            metaDataCache = null;
        }
        public BasePropStrategy GetProp(string id)
        {
            if (props.TryGetValue(id, out var func))
            {
                return func.Invoke();
            }
            else
            {
                return GetProp("00000");
            }
        }
        public void PropPoolRequestHandler(PropPoolRequestEvent propPoolRequestEvent)
        {
            if (metaDataCache == null)
            {
                metaDataCache = new();
                foreach (var pair in props)
                {
                    if (pvpBlacklist.Contains(pair.Key)) continue;
                    metaDataCache.Add(pair.Value.Invoke().GetMetaData());
                }
                /** 字典遍历顺序无契约，按 id 升序固定下来 */
                metaDataCache.Sort((a,b)=>string.CompareOrdinal(a.factoryId,b.factoryId));
            }
            EventBus.Publish<PropPoolEvent>(new(){metaDatas=metaDataCache});
        }
        /// <summary>
        /// 校验一份 PvP 卡组是否合法：长度、不重复、工厂中存在、不在黑名单
        /// </summary>
        /// <param name="deck">待校验的卡组</param>
        /// <param name="reason">不合法时的原因，合法时为 null</param>
        /// <returns>是否合法</returns>
        public bool IsLegalDeck(List<string> deck, out string reason)
        {
            reason = null;
            if (deck == null || deck.Count != 7)
            {
                reason = $"卡组长度必须为7，实际为{deck?.Count}";
                return false;
            }

            HashSet<string> seen = new();
            foreach (var id in deck)
            {
                if (id == null || !props.ContainsKey(id))
                {
                    reason = $"道具不存在：{id}";
                    return false;
                }
                if (pvpBlacklist.Contains(id))
                {
                    reason = $"道具不可用：{id}";
                    return false;
                }
                if (!seen.Add(id))
                {
                    reason = $"道具重复：{id}";
                    return false;
                }
            }
            return true;
        }
    }
}