using System;
using System.Collections.Generic;
using Flicker.Core;
using UnityEngine;

namespace Flicker.UI
{
    /// <summary>
    /// UI 堆栈管理器。
    /// 负责按层分类管理所有的 UIPanel 的开启、关闭、遮挡交互。
    /// 支持同层面板基于堆栈（LIFO）的开启关闭，与全屏暂停策略。
    /// </summary>
    public class UIManager : Singleton<UIManager>
    {
        // UI 的统一画布根节点
        [SerializeField] private Canvas _mainCanvas;

        // 面板工厂：这里使用预制体Resources同步加载为例，也可接Addressables
        private string PrefixPath = "UI/Panels/";

        // 按层维护的 Transform 节点，用于调整渲染层级
        private readonly Dictionary<UILayer, Transform> _layerRoots = new Dictionary<UILayer, Transform>();

        // 维护每层级已打开的面板列表（作为堆栈）
        private readonly Dictionary<UILayer, List<UIPanel>> _panelStacks = new Dictionary<UILayer, List<UIPanel>>();

        // 所有已实例化的面板缓存
        private readonly Dictionary<Type, UIPanel> _panelCache = new Dictionary<Type, UIPanel>();

        protected override void OnSingletonInit()
        {
            base.OnSingletonInit();
            // 在运行时如果没有指定 Canvas，尝试创建
            if (_mainCanvas == null)
            {
                var canvasGO = new GameObject("UICanvas");
                canvasGO.transform.SetParent(this.transform);
                _mainCanvas = canvasGO.AddComponent<Canvas>();
                _mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }

            // 初始化所有定义了的层级节点
            foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
            {
                string layerName = layer.ToString();
                Transform layerTransform = _mainCanvas.transform.Find(layerName);
                
                if (layerTransform == null)
                {
                    var layerGO = new GameObject(layerName);
                    layerGO.transform.SetParent(_mainCanvas.transform, false);
                    // 使用 RectTransform 沾满全屏
                    var rt = layerGO.AddComponent<RectTransform>();
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                    layerTransform = layerGO.transform;
                }

                _layerRoots[layer] = layerTransform;
                _panelStacks[layer] = new List<UIPanel>();
            }

            /** 闪白层：开机即建好并常驻，之后每次命中只是重播动画 */
            Open<ScreenFlash>();
        }

        /// <summary>
        /// 打开并推入面板到对应堆栈层。
        /// </summary>
        public T Open<T>(object args = null) where T : UIPanel
        {
            T panel = GetOrCreatePanel<T>();
            if (panel == null) return null;

            UILayer layer = panel.Layer;
            var stack = _panelStacks[layer];

            // 如果该面板已经在顶层，重新调一遍 OnOpen 刷新参数（或者忽略）
            if (stack.Count > 0 && stack[stack.Count - 1] == panel)
            {
                panel.OnOpen(args);
                return panel;
            }

            // 如果堆栈里包含它（但不在顶层），直接把它挪到顶层
            if (stack.Contains(panel))
            {
                stack.Remove(panel);
            }

            // 处理全屏遮挡逻辑：如果新面板是全屏，则暂停旧栈顶
            if (panel.IsFullScreen && stack.Count > 0)
            {
                // 可以暂停下面所有，也可以只暂停顶层，这里采用“暂停下面全部非全屏，直到遇到下一个全屏”的逻辑
                for (int i = stack.Count - 1; i >= 0; i--)
                {
                    stack[i].OnPause();
                    if (stack[i].IsFullScreen) break;
                }
            }

            // 置于最后渲染（最上层）
            panel.transform.SetAsLastSibling();
            stack.Add(panel);
            
            panel.IsOpened = true;
            panel.OnOpen(args);

            return panel;
        }

        /// <summary>
        /// 关闭指定类型的面板，并处理下层面板的恢复。
        /// </summary>
        public void Close<T>() where T : UIPanel
        {
            Close(typeof(T));
        }

        /// <summary>
        /// 关闭面板（供基类反射或内部使用）。
        /// </summary>
        public void Close(Type panelType)
        {
            if (!_panelCache.TryGetValue(panelType, out var panel) || !panel.IsOpened)
                return;

            UILayer layer = panel.Layer;
            var stack = _panelStacks[layer];

            if (!stack.Contains(panel)) return;

            bool isTop = (stack[stack.Count - 1] == panel);
            stack.Remove(panel);
            
            panel.IsOpened = false;
            panel.OnClose();

            // 如果关闭的正好是栈顶 的全屏 面板，我们需要恢复下面的面板
            if (isTop && panel.IsFullScreen && stack.Count > 0)
            {
                for (int i = stack.Count - 1; i >= 0; i--)
                {
                    stack[i].OnResume();
                    // 恢复直到遇到下一个全屏面板截止
                    if (stack[i].IsFullScreen) break;
                }
            }
        }

        /// <summary>
        /// 关闭某一层的所有面板
        /// </summary>
        public void CloseAll(UILayer layer)
        {
            var stack = _panelStacks[layer];
            for (int i = stack.Count - 1; i >= 0; i--)
            {
                var panel = stack[i];
                panel.IsOpened = false;
                panel.OnClose();
            }
            stack.Clear();
        }

        /// <summary>
        /// 获取已经实例化的面板
        /// </summary>
        public T GetPanel<T>() where T : UIPanel
        {
            if (_panelCache.TryGetValue(typeof(T), out var panel))
            {
                return panel as T;
            }
            return null;
        }

        private T GetOrCreatePanel<T>() where T : UIPanel
        {
            if (_panelCache.TryGetValue(typeof(T), out var existing))
            {
                return existing as T;
            }

            // 尝试在 Canvas 下寻找已存在的面板
            T panel = _mainCanvas.GetComponentInChildren<T>(true);
            if (panel != null)
            {
                _panelCache[typeof(T)] = panel;
                
                // 确保它在正确的层级下
                Transform correctLayerRoot = _layerRoots[panel.Layer];
                if (panel.transform.parent != correctLayerRoot)
                {
                    panel.transform.SetParent(correctLayerRoot, false);
                }
                
                return panel;
            }

            // 按照类名去 Resources 加载 Prefab
            string path = PrefixPath + typeof(T).Name;
            var prefab = Resources.Load<GameObject>(path);

            if (prefab == null)
            {
                Debug.LogWarning($"[UIManager] Cannot find UI Prefab at Resources/{path}. Creating empty panel dynamically.");
                var goDyn = new GameObject(typeof(T).Name);
                var rt = goDyn.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                
                panel = goDyn.AddComponent<T>();
                goDyn.transform.SetParent(_layerRoots[panel.Layer], false);
                
                _panelCache[typeof(T)] = panel;
                goDyn.SetActive(false);
                panel.IsOpened = false;
                return panel;
            }

            var go = Instantiate(prefab);
            panel = go.GetComponent<T>();

            if (panel == null)
            {
                Debug.LogError($"[UIManager] Prefab does not contain UIPanel component: {typeof(T).Name}");
                Destroy(go);
                return null;
            }

            // 挂载到对应的层级节点下
            go.transform.SetParent(_layerRoots[panel.Layer], false);
            go.name = typeof(T).Name;
            
            _panelCache[typeof(T)] = panel;

            // 初始隐藏
            go.SetActive(false);
            panel.IsOpened = false;

            return panel;
        }
    }
}
