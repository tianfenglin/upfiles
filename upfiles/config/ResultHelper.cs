using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace upfiles.config
{
    public class ResultHelper
    {
        private bool success = true;
        private string useId = "";
        private object msg = "";
        /// <summary>
        /// 默认true
        /// </summary>
        public bool Success
        {
            get { return success; }
            set { success = value; }
        }
        /// <summary>
        /// 返回需要使用的id
        /// </summary>
        public string UseId
        {
            get { return useId; }
            set { useId = value; }
        }
        /// <summary>
        /// 返回消息内容
        /// </summary>
        public object Msg
        {
            get { return msg; }
            set { msg = value; }
        }
        /// <summary>
        /// 获取返回的内容字符串
        /// </summary>
        /// <returns></returns>
        public string GetResultInfo()
        {

            JsonSerializerSettings jSetting = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),//首字母小写
                NullValueHandling = NullValueHandling.Ignore,  //过滤null
                DateFormatString = "yyyy-MM-dd HH:mm:ss" //设置时间
            };
            string result = JsonConvert.SerializeObject(this, jSetting);
            return result;
        }
    }
}
