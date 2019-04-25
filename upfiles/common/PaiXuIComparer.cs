using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace upfiles.common
{
    public class PaiXuIComparer : IComparer<string>
    {
        /// <summary>
        /// 对文件进行排序
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public int Compare(string a, string b)
        {
            string[] str1 = a.Split('\\');
            string[] str2 = b.Split('\\');
            if (int.Parse(str1[str1.Length - 1]) > int.Parse(str2[str2.Length - 1]))
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }
    }
}
