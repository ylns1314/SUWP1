using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Resources.Core;

namespace SUWP1.Helpers
{
    /// <summary>
    /// A class used to generate urls for avatars by giving uid
    /// </summary>
    /// <remarks>
    /// Required length of uids are given in forms of reference to S1APIs.resw, which is 9 but actul uids are only 6 digits.
    /// Thus 3 zeors will be added in front for properly formatting.
    /// </remarks>
    static class AvatarFactory
    {
        private static ResourceLoader loader;
        private static string PREFIX;
        private static int LEN_UID;
        static AvatarFactory()
        {
            Debug.WriteLine(ResourceManager.Current.AllResourceMaps.Count);
            foreach(string key in ResourceManager.Current.AllResourceMaps.Keys)
                Debug.WriteLine(key);
            loader = ResourceLoader.GetForCurrentView("S1APIs");
            PREFIX = loader.GetString("PREFIX_AVATAR");
            LEN_UID = Int16.Parse(loader.GetString("LENGTH_UID"));
            Debug.WriteLine(getAvatar("125253", true));
        }
        
        /// <summary>
        /// The method generates the URL of avatar for the given uid.
        /// </summary>
        /// <param name="uid">The user id</param>
        /// <param name="middle">Whether return the middle size avatar's URL or not (will return the smaller one) </param>
        /// <returns>URL of avatar for the given uid</returns>
        public static string getAvatar(string uid, bool middle)
        {
            string result = uid;
            if (uid.Length < LEN_UID)
                result = uid.PadLeft(LEN_UID, '0');
            result = result.Substring(0, 3) + '/' + result.Substring(3, 2) + '/' + result.Substring(5, 2) + '/' + result.Substring(7, 2);
            if (middle)
                result += loader.GetString("SUFFIX_AVATAR_MIDDLE");
            else
                result += loader.GetString("SUFFIX_AVATAR_SMALL");
            return PREFIX + result;
        }
    }
}
