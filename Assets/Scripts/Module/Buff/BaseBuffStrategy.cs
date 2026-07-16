using UnityEngine;

namespace Flicker.Module
{
    /// <summary>
    /// buff基类，共管Buff容器管理
    /// </summary>
    public class BaseBuffStrategy
    {

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
        public string iconPath = "invisible";
        /// <summary>
        /// 描述
        /// </summary>
        public string description = "默认描述";
        /// <summary>
        /// 图标不可见
        /// </summary>
        public bool iconInvisible = true;
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
    }
}