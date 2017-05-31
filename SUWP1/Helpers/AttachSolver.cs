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
            var result = obj["message"].GetString();
            if (obj.ContainsKey("imagelist"))
            {
                var jaAttachList = obj["imagelist"].GetArray();
                var dictImgList = new Dictionary<int, string>();
                var numAttaches = jaAttachList.Count;
                for (var i = 0; i < numAttaches; i++)
                {
                    var tmpObj = obj["attachments"].GetObject()[jaAttachList[i].GetString()].GetObject();
                    // non-zero for images
                    if (int.Parse(tmpObj["isimage"].GetString()) != 0)
                    {
                        dictImgList.Add(int.Parse(tmpObj["aid"].GetString()), tmpObj["url"].GetString() + tmpObj["attachment"].GetString());
                    }
                }
                var strUrlImg = string.Join("\n", dictImgList);
                result = regAttach.Replace(result, (Match match) =>
                    "<img src=\"" + dictImgList[int.Parse(Regex.Match(match.Value, @"\d+").Value)] + "\" />");
            }
            return result;
        }

        public static bool regexTester(string reg, string input)
        {
            var regReg = new Regex(reg);
            return regReg.IsMatch(input);
        }
    }
}
