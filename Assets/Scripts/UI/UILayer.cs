namespace Flicker.UI
{
    /// <summary>
    /// UI 面板的层级划分。
    /// 层级越高，渲染在越上方。
    /// </summary>
    public enum UILayer
    {
        Background = 0,  // 背景图层（世界 Sprite 之下）
        HUD = 100,       // 游戏内 HUD（血条、金币等常驻信息），覆盖于世界 Sprite 之上
        Normal = 200,    // 普通面板（背包、角色属性等），全屏时遮挡下层
        Popup = 300,     // 弹窗层（确认框、提示等）
        Guide = 400,     // 新手引导层
        Loading = 500,   // 加载遮罩或顶层覆盖
    }
}
