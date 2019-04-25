using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace upfiles.common
{
    public static class SteamBinaryFile
    {
        private static readonly FormOptions _FormOptions = new FormOptions();

        public static async Task<string> StreamFilesAsync(this HttpRequest request, string directory)
        {
            string result = "0";
            if (!UpFileValidation.IsMultipartContentType(request.ContentType))
            {
                throw new Exception($"Expected a multipart request, but got {request.ContentType}");
            }

            //用于累加请求中所有表单url编码的键值对数量。
            KeyValueAccumulator formAccumulator = new KeyValueAccumulator();
            //获取请求的ContentType并model化
            MediaTypeHeaderValue contentType = MediaTypeHeaderValue.Parse(request.ContentType);
            //表单默认的分界线最大长度
            int boundaryLength = _FormOptions.MultipartBoundaryLengthLimit;
            //验证并得到分界线
            var boundary = UpFileValidation.GetBoundary(contentType, boundaryLength);
            var reader = new MultipartReader(boundary, request.Body);

            var section = await reader.ReadNextSectionAsync();//用于读取Http请求中的第一个section数据
            while (section != null)
            {
                ContentDispositionHeaderValue content;
                var isHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out content);

                if (isHeader)
                {
                    //文件数据处理
                    if (UpFileValidation.IsFormFile(content))
                    {
                        Dictionary<string, StringValues> dic = formAccumulator.GetResults();
                        string filepath = dic["filepath"];
                        string fileMd5 = dic["fileMd5"];
                        string chunk = dic["chunk"];
                        string chunks = dic["chunks"];
                        string save_filepath = directory + "\\" + filepath + "\\" + fileMd5;
                        if (!Directory.Exists(save_filepath))
                        {
                            Directory.CreateDirectory(save_filepath);
                        }

                        var fileName = UpFileValidation.GetFileName(content);
                        //这个是每一次从Http请求的section中读出文件数据的大小，单位是Byte即字节，这里设置为1024的意思是，
                        //每次从Http请求的section数据流中读取出1024字节的数据到服务器内存中，然后写入下面targetFileStream的文件流中
                        //，可以根据服务器的内存大小调整这个值。这样就避免了一次加载所有上传文件的数据到服务器内存中，导致服务器崩溃。
                        var loadBufferBytes = 10 * 1024 * 1024;

                        using (var targetFileStream = System.IO.File.Create(save_filepath + "\\" + chunk))
                        {
                            await section.Body.CopyToAsync(targetFileStream, loadBufferBytes);
                            result = "1";
                        }

                    }
                    //额外表单参数处理
                    else if (UpFileValidation.IsFormData(content))
                    {
                        // 这里不要限制键名的长度，因为多部分头的长度限制已经生效。
                        var key = HeaderUtilities.RemoveQuotes(content.Name);
                        var encoding = GetEncoding(section);
                        using (var streamReader = new StreamReader(
                            section.Body,
                            encoding,
                            detectEncodingFromByteOrderMarks: true,
                            bufferSize: 1024,
                            leaveOpen: true))
                        {
                            // 获取键的值
                            var value = await streamReader.ReadToEndAsync();
                            //如果值为undefined，则替换为空
                            if (String.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
                            {
                                value = String.Empty;
                            }
                            formAccumulator.Append(key.Value, value);
                            //参数键值对不能大于默认数量，否则异常
                            if (formAccumulator.ValueCount > _FormOptions.ValueCountLimit)
                            {
                                throw new InvalidDataException($"键值对参数最大不能超过{ _FormOptions.ValueCountLimit}");
                            }
                        }
                    }
                }
                //用于读取Http请求中的下一个section数据
                section = await reader.ReadNextSectionAsync();
            }
            return result;
        }
        private static Encoding GetEncoding(MultipartSection section)
        {
            MediaTypeHeaderValue mediaType;
            var hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out mediaType);
            //采用UTF8编码
            if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding))
            {
                return Encoding.UTF8;
            }
            return mediaType.Encoding;
        }
    }
}
