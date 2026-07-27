using UnityEngine;

namespace Flicker.Module
{
    public struct BaseBuffStrategyMetaData
    {
        /// <summary>
        /// buff唯一递增id，代替引用
        /// </summary>
        public int buffId;
        /// <summary>
        /// Buff名称
        /// </summary>
        public string name;
        /// <summary>
        /// 资源地址
        /// </summary>
        public string path;
        /// <summary>
        /// 描述
        /// </summary>
        public string description;
        /// <summary>
        /// 图标可见
        /// </summary>
        public bool iconVisible;
        /// <summary>
        /// 是否可叠层
        /// </summary>
        public bool isStackable;
        /// <summary>
        /// 当前层数
        /// </summary>
        public int stacks;
        /// <summary>
        /// 最大层数
        /// </summary>
        public int maxStacks;
    }
    /// <summary>
    /// buff基类，共管Buff容器管理
    /// </summary>
    public class BaseBuffStrategy
    {
        private static int buffIder = 0;
        public BaseBuffStrategy()
        {
            this.buffId = buffIder;
            buffIder++;
        }
        /// <summary>
        /// buff唯一id，代替引用跨层传递
        /// </summary>
        public int buffId {get;private set;}
        /// <summary>
        /// buff容器绑定
        /// </summary>
        protected BuffContainer buffContainer;
        /// <summary>
        /// 效果容器绑定
        /// </summary>
        protected EffectContainer effectContainer;
        /// <summary>
        /// Buff名称
        /// </summary>
        public string name = "default";
        /// <summary>
        /// 资源地址
        /// </summary>
        public string path = "Unknown";
        /// <summary>
        /// 描述
        /// </summary>
        public string description = "默认描述";
        /// <summary>
        /// 图标可见
        /// </summary>
        public bool iconVisible = true;
        /// <summary>
        /// 是否可叠层
        /// </summary>
        public bool isStackable = false;
        /// <summary>
        /// 当前层数
        /// </summary>
        public int stacks = 1;
        /// <summary>
        /// 最大层数
        /// </summary>
        public int maxStacks = 1;
        /// <summary>
        /// 添加时触发
        /// </summary>
        public virtual void OnAdd(){}
        /// <summary>
        /// 移除时触发
        /// </summary>
        public virtual void OnRemove(){}
        /// <summary>
        /// 叠层变化时触发
        /// </summary>
        public virtual void OnStacksChange(){}

        public void BindBuffContainer(BuffContainer buffContainer)
        {
            this.buffContainer = buffContainer;
        }
        public void BindEffectContainer(EffectContainer effectContainer)
        {
            this.effectContainer = effectContainer;
        }
        public BaseBuffStrategyMetaData GetMetaData()
        {
            BaseBuffStrategyMetaData evt = new BaseBuffStrategyMetaData();
            evt.buffId=buffId;
            evt.name=name;
            evt.path=path;
            evt.description=description;
            evt.iconVisible=iconVisible;
            evt.isStackable=isStackable;
            evt.stacks=stacks;
            evt.maxStacks=maxStacks;
            return evt;
        }
    }
}