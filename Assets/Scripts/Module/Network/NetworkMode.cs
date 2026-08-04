using Mirror;

namespace Flicker.Module.Network
{
    /// <summary>
    /// 网络模式判断工具，基于全局静态状态，Awake 阶段即可用（不依赖 Mirror 回调时序）。
    /// </summary>
    public static class NetworkMode
    {
        /// <summary>
        /// 是否处于多人模式（联网中，含服务端/客户端/Host）。
        /// 单机模式（未来肉鸽）为 false。
        /// </summary>
        public static bool IsMultiplay =>
            NetworkClient.active || NetworkServer.active;

        /// <summary>
        /// 是否服务端（含 Host）。
        /// </summary>
        public static bool IsServer => NetworkServer.active;

        /// <summary>
        /// 是否客户端空壳（多人 + 非服务端）。
        /// 此状态下逻辑层组件应空转（不执行逻辑）。
        /// </summary>
        public static bool IsClientShell =>
            NetworkClient.active && !NetworkServer.active;

        /// <summary>
        /// 是否本机应运行逻辑（服务端，或单机模式）。
        /// 等价于：!IsClientShell
        /// </summary>
        public static bool IsLogicActive =>
            IsServer || !IsMultiplay;
    }
}
