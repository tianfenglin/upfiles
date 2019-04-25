using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace upfiles.config
{
    /// <summary>
    /// 异常处理
    /// </summary>
    public class MyExceptionFilter : ExceptionFilterAttribute
    {
        private Logger logger = LogManager.GetCurrentClassLogger();
        public override void OnException(ExceptionContext context)
        {
            base.OnException(context);
            Exception ex = context.Exception;
            logger.Error(ex);
            string msg = ex.Message;
            if (ex.GetType() == typeof(SqlException))//数据库相关错误
            {
                msg = "错误代码：000001";
            }
            else if (ex.GetType() == typeof(DivideByZeroException))//尝试除以0
            {
                msg = "错误代码：000002";
            }
            else if (ex.GetType() == typeof(NullReferenceException))//null异常
            {
                msg = "错误代码：000004";
            }
            ResultHelper resultHelper = new ResultHelper();
            resultHelper.Success = false;
            resultHelper.Msg = msg;
            string result = resultHelper.GetResultInfo();
            context.Result = new ContentResult() { Content = result };
            context.ExceptionHandled = true;
            context.HttpContext.Response.Clear();
            context.HttpContext.Response.ContentType = "text/html; charset=utf-8";
            context.HttpContext.Response.StatusCode = 500;
            context.ExceptionHandled = true;
        }
    }
}
