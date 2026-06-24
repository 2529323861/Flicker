using System.Collections.Generic;
using Flicker.Core;
using Flicker.Core.Service;
using UnityEngine;

namespace Flicker.Veiw
{
    class Vibration
    {   
        /// <summary>
        /// 最大时间
        /// </summary>
        readonly public float maxTime;
        /// <summary>
        /// 最大振幅
        /// </summary>
        readonly public Vector3 amplitude;
        /// <summary>
        /// 当前时间
        /// </summary>
        public float nowTime;
        public Vibration(float maxTime, Vector3 amplitude)
        {
            this.maxTime = maxTime;
            this.amplitude = amplitude;
        }
    }
    public class VibrationMgr: Singleton<VibrationMgr>, VibrationMgrInterface
    {   
        /// <summary>
        /// 振动数组，每次进行振动时会遍历累加振幅
        /// </summary>
        private List<Vibration> vibrations = new List<Vibration>();
        /// <summary>
        /// 截断比例，当达到最大时间时，此时振动波函数振幅为最大振幅*此系数，默认2%，适当减小可以使得动画更加平滑
        /// </summary>
        public float cutoffRatio = 0.002f;

        public Vector3 cameraOffset;
        void Update()
        {   
            Vector3 offset = new Vector3(0,0);
            for (int i = vibrations.Count-1;i>=0;i--)
            {
                vibrations[i].nowTime += Time.deltaTime;
                float factor = Mathf.Sin(20*Mathf.PI*vibrations[i].nowTime) * Mathf.Exp(vibrations[i].nowTime * Mathf.Log(cutoffRatio)/vibrations[i].maxTime);
                offset.x += vibrations[i].amplitude.x * factor;
                offset.y += vibrations[i].amplitude.y * factor;
                offset.z += vibrations[i].amplitude.z * factor;
                if (vibrations[i].nowTime >= vibrations[i].maxTime)
                {
                    vibrations.RemoveAt(i);
                }
            }
            Camera.main.transform.localPosition = offset+cameraOffset;
        }
        public void AddVibration(float maxTime, Vector3 amplitude)
        {
            vibrations.Add(new Vibration(maxTime,amplitude));
        }
        public void RemoveAllVibrations()
        {
            vibrations.Clear();
        }
    }
}