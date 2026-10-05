using System;
using System.Collections.Generic;
using Flicker.Core;
using Flicker.Module.Prop;

namespace Flicker.Module
{
    public class PropFactory: Singleton<PropFactory>
    {
        private Dictionary<string, Func<BasePropStrategy>> props = new();
        protected override void OnSingletonInit()
        {
            RegisterAllProps();
        }
        public void RegisterAllProps()
        {
            props = new();
            RegisterProp("00000",()=>new BasePropStrategy());
            RegisterProp("00001",()=>new RushGelProp());
            RegisterProp("00002",()=>new EmergenceSporeProp());
            RegisterProp("00003",()=>new ReverseFlowHourglassProp());
            RegisterProp("00004",()=>new NodeviationArrowProp());
            RegisterProp("00005",()=>new CompensationVoucherProp());
            RegisterProp("00006",()=>new NamelessHaloProp());

        }
        public void RegisterProp(string id, Func<BasePropStrategy> func)
        {
            props.Add(id,func);
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
    }
}