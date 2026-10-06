using UnityEngine;

namespace Flicker.Module.Prop
{
    public class NamelessHaloProp: BasePropStrategy
    {
        public NamelessHaloProp()
        {
            name = "无名光环";
            factoryId = "00006";
            /** 暂无美术资源，补齐 PropIcon/NamelessHalo.png 后改为 "NamelessHalo" */
            path = "Unknown";
            description = "与最远的一名敌人交换位置。";
        }
        /// <summary>
        /// 目标是否可作为换位对象。
        /// 跳跃或僵直中的角色位置由协程与状态机接管，换位会与其冲突，故排除
        /// </summary>
        private bool IsSwappable(GameCharacterController target)
        {
            if (target == gameCharacterController) return false;  //跳过自己
            if (target.teamTag == gameCharacterController.teamTag) return false;  //只找敌人

            var state = target.State;
            return state == GameCharacterState.Idle || state == GameCharacterState.Moving;
        }
        /// <summary>
        /// 需要自身可自由移动，且场上有可换位的敌人
        /// </summary>
        public override bool CanUse()
        {
            /** 自身跳跃或僵直中时位置由协程接管，换位会与状态机冲突 */
            var selfState = gameCharacterController.State;
            if (selfState != GameCharacterState.Idle && selfState != GameCharacterState.Moving) return false;

            foreach (var i in CharacterControllerManager.Instance.GetAll())
            {
                if (IsSwappable(i)) return true;
            }
            return false;
        }
        public override void OnUse()
        {
            Vector3 selfPosition = gameCharacterController.transform.localPosition;

            /** 找出距离最远的敌人 */
            GameCharacterController farthest = null;
            float maxDistance = 0f;
            foreach (var i in CharacterControllerManager.Instance.GetAll())
            {
                if (!IsSwappable(i)) continue;

                float distance = Vector3.Distance(selfPosition, i.transform.localPosition);
                if (farthest == null || distance > maxDistance)
                {
                    maxDistance = distance;
                    farthest = i;
                }
            }

            /** 交换位置与墙面，自身坐标须先缓存 */
            float selfY = selfPosition.y;
            bool selfWall = gameCharacterController.isRightWall;
            gameCharacterController.SetPosition(farthest.transform.localPosition.y, farthest.isRightWall);
            farthest.SetPosition(selfY, selfWall);
        }
    }
}
