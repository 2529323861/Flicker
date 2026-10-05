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
        /// 需要场上有敌人才能使用
        /// </summary>
        public override bool CanUse()
        {
            foreach (var i in CharacterControllerManager.Instance.GetAll())
            {
                if (i == gameCharacterController) continue;  //跳过自己
                if (i.teamTag == gameCharacterController.teamTag) continue;  //只找敌人
                return true;
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
                if (i == gameCharacterController) continue;  //跳过自己
                if (i.teamTag == gameCharacterController.teamTag) continue;  //只找敌人

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
