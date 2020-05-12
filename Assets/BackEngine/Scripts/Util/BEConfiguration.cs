using UnityEngine;

namespace BE.Util
{
    public class BEConfiguration : ScriptableObject
    {
        public string appSecret;
        public BERegion region = BERegion.US;

        public string getEndPoint()
        {
            switch (region)
            {
                case BERegion.US:
                    return "https://backengine-server.herokuapp.com/api";
                default:
                    return "https://backengine-server.herokuapp.com/api";
            }
        }
    }
}
