using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace upfiles.common
{
    public static class UpFileValidation
    {
        //Content-Disposition: form-data; name="myfile1"; filename="Misc 002.jpg" 文件
        //Content-Disposition: form-data; name="myfile1"; 额外参数
        /// <summary>
        /// 获取及验证分界符
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="lengthLimit"></param>
        /// <returns></returns>
        public static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
        {
            string boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;
            //不存在分隔符
            if (string.IsNullOrWhiteSpace(boundary))
            {
                throw new InvalidDataException("不存在分隔符");
            }
            //判断分隔符长度是否过长
            if (boundary.Length > lengthLimit)
            {
                throw new InvalidDataException(
                    $"判断分隔符长度超过默认最大值{lengthLimit}");
            }

            return boundary;
        }

        public static bool IsMultipartContentType(string contentType)
        {
            return !string.IsNullOrEmpty(contentType)
                    && contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// 如果section是表单键值对类型的section，那么本方法返回true
        /// </summary>
        /// <param name="contentDisposition"></param>
        /// <returns></returns>
        public static bool IsFormData(ContentDispositionHeaderValue contentDisposition)
        {
            return contentDisposition != null
                    && contentDisposition.DispositionType.Equals("form-data")
                    && string.IsNullOrEmpty(contentDisposition.FileName.Value)
                    && string.IsNullOrEmpty(contentDisposition.FileNameStar.Value);
        }

        /// <summary>
        /// 如果section是上传文件类型的section，那么本方法返回true
        /// </summary>
        /// <param name="contentDisposition"></param>
        /// <returns></returns>
        public static bool IsFormFile(ContentDispositionHeaderValue contentDisposition)
        {
            return contentDisposition != null && contentDisposition.DispositionType.Equals("form-data")
                    && (!string.IsNullOrEmpty(contentDisposition.FileName.Value)
                    || !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value));
        }
        /// <summary>
        /// 获取name值
        /// </summary>
        /// <param name="disposition"></param>
        /// <returns></returns>
        public static string GetName(ContentDispositionHeaderValue disposition)
        {
            return disposition.Name.Value;
        }

        /// <summary>
        /// 获取filename值,文件参数有filename，额外参数没有filename
        /// </summary>
        /// <param name="disposition"></param>
        /// <returns></returns>
        public static string GetFileName(ContentDispositionHeaderValue disposition)
        {
            return disposition.FileName.Value;
        }
    }
}
