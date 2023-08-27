using UnityEngine;
using ValheimTooler.Core;
using ValheimTooler.Utils;

namespace ValheimTooler.Models.Mono
{
    class PinnedObject : MonoBehaviour
    {
        public Minimap.PinData pin;

        public void Init(string aName)
        {
            pin = Minimap.instance.AddPin(base.transform.position, Minimap.PinType.Icon3, aName, ConfigManager.s_permanentPins.Value, false);
            ZLog.Log(string.Format("Tracking: {0} at {1} {2} {3}", new object[]
            {
                aName,
                base.transform.position.x,
                base.transform.position.y,
                base.transform.position.z
            }));
        }

        private void OnDestroy()
        {
            bool flag = pin != null && Minimap.instance != null && !ConfigManager.s_permanentPins.Value;
            if (flag)
            {
                Minimap.instance.RemovePin(pin);
                ZLog.Log(string.Format("Removing: {0} at {1} {2} {3}", new object[]
                {
                    pin.m_name,
                    base.transform.position.x,
                    base.transform.position.y,
                    base.transform.position.z
                }));
            }
        }
    }
}
