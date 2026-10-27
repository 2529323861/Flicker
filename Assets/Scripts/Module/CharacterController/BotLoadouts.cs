using System.Collections.Generic;
using UnityEngine;

namespace Flicker.Module
{
    /// <summary>
    /// 人机携带的预制卡组。
    ///
    /// 只收录 AI 会主动使用的道具（MockPropPolicy 中意图非 None 的），
    /// 且每套至少有一个能覆盖 Chase，避免 AI 在追击阶段空手。
    /// 每套三个，不允许重复，与 PropContainer.EquippedPropCount 对应。
    /// </summary>
    public static class BotLoadouts
    {
        private static readonly string[][] pools =
        {
            /** 追击：疾行凝胶、无名光环、逆流沙漏 —— 全程都有位移与补 CD 手段 */
            new[] { "00001", "00006", "00003" },

            /** 进攻：羽化孢子、不偏矢、静电甲壳 —— 三件都在起跳前叠满 */
            new[] { "00002", "00004", "00008" },

            /** 游走：脉冲发条、借风铃、疾行凝胶 —— 以脱离与再进场为主 */
            new[] { "00007", "00012", "00001" },

            /** 消耗：轻质骨、补偿券、逆流沙漏 —— 靠缩短跳跃 CD 提高出手频率 */
            new[] { "00009", "00005", "00003" },
        };

        /// <summary>
        /// 随机取一套卡组，返回新的列表，调用方可自由修改
        /// </summary>
        public static List<string> PickRandom()
        {
            return new List<string>(pools[Random.Range(0, pools.Length)]);
        }
    }
}
