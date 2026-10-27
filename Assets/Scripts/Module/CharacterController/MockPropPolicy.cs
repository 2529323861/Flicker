namespace Flicker.Module
{
    using UnityEngine;

    /// <summary>
    /// AI 的行动意图。用于把「AI 想做什么」与「道具能做什么」对上
    ///
    /// 不设「缺资源」一类：补资源道具的时机完全由特判决定（缺冷却、被禁跳），
    /// 它们该归到追、逃还是攻，取决于补上资源后想做什么，因此按用途分散归类
    /// </summary>
    [System.Flags]
    public enum PropIntent
    {
        None    = 0,
        /// <summary>想接近对手</summary>
        Chase   = 1 << 0,
        /// <summary>想远离或规避</summary>
        Escape  = 1 << 1,
        /// <summary>即将起跳，希望强化或加速本次攻击</summary>
        Attack  = 1 << 2,
    }

    /// <summary>
    /// 道具到 AI 意图的映射表。
    ///
    /// 为什么放在 AI 侧而不是 BasePropStrategy：
    /// 当前道具组是封闭集合（PvP 固定 13 个），不必为「新增道具自动获得 AI 行为」付代价。
    /// 若将来道具组开放，这张表应整体迁移到道具侧，决策逻辑本身不用改。
    ///
    /// 判定规则：只读描述的第一句（收益句），忽略代价。
    /// 因此交换类道具 AI 也会用，并且会吃到代价。这是刻意接受的：
    /// 练习模式不需要完美 AI，而「每个道具都写一套时机判断」的成本远高于收益。
    ///
    /// Attack 的额外要求：必须在起跳前使用才有意义。
    /// 因此只收录「修饰下一次跳跃」或「持续数秒且围绕跳跃结算」的道具，
    /// 它们的收益全都发生在被弹反或命中之后，而非使用瞬间。
    ///
    /// 返回 None 的道具，AI 永不主动使用。
    /// </summary>
    public static class MockPropPolicy
    {
        public static PropIntent IntentOf(string factoryId)
        {
            return factoryId switch
            {
                "00001" => PropIntent.Chase | PropIntent.Escape,     // 疾行凝胶：移速 +150%
                "00002" => PropIntent.Attack,                        // 羽化孢子：滞空 -80%，飞跃更快
                "00003" => PropIntent.Chase | PropIntent.Escape,     // 逆流沙漏：清零跳跃冷却，追与逃都用得上
                "00004" => PropIntent.Attack,                        // 不偏矢：弹反不改变方向
                "00005" => PropIntent.Attack,                        // 补偿券：未命中返还跳跃冷却
                "00006" => PropIntent.Chase,                         // 无名光环：与最远敌人换位
                "00007" => PropIntent.Escape | PropIntent.Chase,     // 脉冲发条：冲刺，期间免伤
                "00008" => PropIntent.Attack,                        // 静电甲壳：被弹反时对方僵直更重
                "00009" => PropIntent.Attack,                        // 轻质骨：跳跃冷却上限 -40%，服务进攻
                "00012" => PropIntent.Escape | PropIntent.Attack,    // 借风铃：立刻起跳 + 解除僵直

                /** 以下不用：收益与代价同在第一句，或时机需要推理 */
                // 00010 铅制稳定器：纯防御，没有对应意图
                // 00011 空转轴：清完道具 CD 需要后续配合
                // 00013 锚爪：折返语义与进攻矛盾

                _ => PropIntent.None,
            };
        }

        /// <summary>
        /// 第二级过滤：这道具在当前局面下值不值得用。
        ///
        /// 与 IntentOf 的关系是串联而非并列：
        ///   IntentOf 粗筛「这个意图下哪些道具能用」，
        ///   ConditionOf 精筛「在这批候选里，此刻用是否划算」。
        ///
        /// 补资源类道具的时机完全由这里承担，它们本身就是「需要时才用」的道具。
        /// 返回 false 表示此刻不该用，AI 会跳过它继续看下一个候选。
        /// 未列出的道具一律通过。
        /// </summary>
        public static bool ConditionOf(string factoryId, MockController owner)
        {
            GameCharacterController target = owner.Target;

            return factoryId switch
            {
                /** 移速加成：贴脸时加速没有意义，留到需要拉开或追上时再开 */
                "00001" => Mathf.Abs(owner.DeltaY) > 2f,

                /** 清零跳跃冷却：只在真的跳不了时才用，否则白占一个冷却位 */
                "00003" => !owner.CanJump,

                /** 冲刺期间免伤：对手正在滞空时价值最高，其余时候只是一次位移 */
                "00007" => target != null && target.State == GameCharacterState.JumpHanging,

                /** 跳跃冷却上限 -40%：缺冷却才有意义 */
                "00009" => !owner.CanJump,

                /** 立刻起跳并解除僵直：僵直中才用。
                 *  否则本次跳跃不造成攻击，相当于主动放弃一次进攻 */
                "00012" => owner.IsJumpBlocked,

                _ => true,
            };
        }
    }

    /// <summary>
    /// 禁止跳跃的 buff 白名单。
    ///
    /// 机制上跳跃被禁止是经 JumpingProhibitionEffect 在 BeforeJumpEvent 取消实现的，
    /// 没有任何查询入口，因此 AI 只能反向维护一份「已知会拦跳跃的 buff」清单。
    /// 新增此类 buff 时必须在 Blockers 里补一行，否则 AI 会持续尝试无效跳跃。
    /// </summary>
    public static class MockJumpBlockPolicy
    {
        private static readonly System.Type[] Blockers =
        {
            typeof(Buff.StiffnessBuff),   // 僵直：无法跳跃且减速
        };

        public static bool IsBlocked(BuffContainer buffContainer)
        {
            if (buffContainer == null) return false;
            foreach (var blocker in Blockers)
            {
                if (buffContainer.HasBuff(blocker)) return true;
            }
            return false;
        }
    }
}
