using UnityEngine;
using System.Collections;

namespace BE.Models
{

    public class BELog
    {
        public string deviceId;
        public string deviceModel;
        public string os;
        public string language;

        public BELog()
        {
            deviceId = SystemInfo.deviceUniqueIdentifier;
            deviceModel = SystemInfo.deviceModel;
            os = SystemInfo.operatingSystem;
            language = Application.systemLanguage.ToString();
        }

    }
}
