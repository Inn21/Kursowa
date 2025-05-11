#region

using UnityEngine;

#endregion

namespace Engine.Core.Utils.Extensions
{
    public static class InputExtension
    {
        public static bool HasAnyInputBegan()
        {
#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0)) return true;
#else
            for(int i = 0; i < Input.touchCount; i++)
            {
                if(Input.GetTouch(i).phase == TouchPhase.Began)
                {
                    return true;
                }
            }
#endif
            return false;
        }
    }
}