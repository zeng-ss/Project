// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("KWmAvSnlqQAbxheBwPkKGiJesjI/Fx3n2n9mWYzsQG2cF+E7WSUtF8FCTENzwUJJQcFCQkPxP3Z95v47m0Z2fd9+e3zCoFnc0Xuv6k43dLA6zkM5NuVXlf+WnTV4BYwAK7G6gAzZvUIFd1B/EEJ2Ib4KixVP1Esvc8FCYXNORUppxQvFtE5CQkJGQ0BtDTL8U9iKrPFhTbjTVD3zqI+iBj2p7HSgScerf7pBdzcoLnF73WTFwE7rdk8soaNkwpibzCrww433M8uMnIcqY/LaDUgJHSE/5tXBcm9KmvmCVa0s04lo8JogVpmh1qHWpUAQPACTL644YuWRqBZi+H12TXWgyUfNYxRC14hMWB7nmiLSzPtOMP9MhI7IPesZ162e8kFAQkNC");
        private static int[] order = new int[] { 12,5,5,3,6,7,12,10,9,12,11,11,13,13,14 };
        private static int key = 67;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
