using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Windows.Data.Json;
using Windows.ApplicationModel.Resources;
using System.Diagnostics;

namespace SUWP1.Helpers
{
    static class AttachSolver
    {
        private static ResourceLoader loaderReg;
        private static string strRegAttach;
        private static Regex regAttach;

        static AttachSolver()
        {
            loaderReg = ResourceLoader.GetForCurrentView("Regexes");
            strRegAttach = loaderReg.GetString("REGEX_ATTACHMENTS");
            regAttach = new Regex(strRegAttach);
        }

        public static string solve(JsonObject obj)
        {
            string result = obj["message"].GetString();
            if (obj.ContainsKey("imagelist"))
            {
                JsonArray jaAttachList = obj["imagelist"].GetArray();
                Dictionary<int, string> dictImgList = new Dictionary<int, string>();
                int numAttaches = jaAttachList.Count;
                for (int i = 0; i < numAttaches; i++)
                {
                    JsonObject tmpObj = obj["attachments"].GetObject()[jaAttachList[i].GetString()].GetObject();
                    // non-zero for images
                    if (int.Parse(tmpObj["isimage"].GetString()) != 0)
                    {
                        dictImgList.Add(int.Parse(tmpObj["aid"].GetString()), tmpObj["url"].GetString() + tmpObj["attachment"].GetString());
                    }
                }
                string strUrlImg = string.Join("\n", dictImgList);
                result = regAttach.Replace(result, (Match match) =>
                    "<img src=\"" + dictImgList[int.Parse(Regex.Match(match.Value, @"\d+").Value)] + "\" />");
            }
            return result;
        }

        public static bool regexTester(string reg, string input)
        {
            Regex regReg = new Regex(reg);
            return regReg.IsMatch(input);
        }
    }
}
