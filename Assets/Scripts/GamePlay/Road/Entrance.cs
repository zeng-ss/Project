using UnityEngine;

namespace CasualGame.CoffeeAway
{
    public class Entrance : AttachPoolPrefabBase
    {
        public void SetPos(Vector3 worldPos, Vector3 roadDir, bool isStart)
        {
            if (roadDir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(roadDir, Vector3.up);

            if (transform.localRotation == Quaternion.identity)
            {
                transform.position = worldPos - roadDir;
            }
            else
            {
                transform.position = worldPos;
            }
        }
    }
}