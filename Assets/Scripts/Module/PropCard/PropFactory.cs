using System;
using System.Collections.Generic;
using Flicker.Core;

namespace Flicker.Module
{
    public class PropFactory: Singleton<PropFactory>
    {
        private Dictionary<string, Func<BasePropStrategy>> props = new();
        protected override void OnSingletonInit()
        {
            RegisterProps();
        }
        public void RegisterProps()
        {
            props = new();

        }
        
    }
}