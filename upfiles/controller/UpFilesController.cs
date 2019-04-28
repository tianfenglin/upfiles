using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using upfiles.common;
using upfiles.config;

namespace upfiles.controller
{
    [Route("upfile")]
    public class UpFilesController : Controller
    {
        private IHostingEnvironment host;
        public UpFilesController(IHostingEnvironment _host)
        {
            host = _host;
        }
        /// <summary>
        /// 单个文件
        /// </summary>
        /// <returns></returns>
        [Route("upfile")]
        public string UpFile()
        {
            string filepath = Request.Form["filepath"];
            string filename = Request.Form["filename"];
            ResultHelper resultHelper = new ResultHelper();
            IFormFileCollection files = Request.Form.Files;
            string filename_new = "";
            //如果有文件名称则取给的名称
            if (filename != null)
            {
                filename_new = filename;
            }
            else
            {
                string newname = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                string[] fileform = files[0].FileName.Split('.');
                newname += "." + fileform[fileform.Length - 1];
                filename_new = newname;
            }
            //如果有文件路径则取给的路径
            string filepath_new = "";
            if (filepath != null)
            {
                filepath_new = host.ContentRootPath + "\\" + filepath;
            }
            else
            {
                filepath_new = host.ContentRootPath + "\\files";
            }
            //判断是否存在文件夹，不存在则创建
            if (!System.IO.Directory.Exists(filepath_new))
            {
                Directory.CreateDirectory(filepath_new);
            }
            string savePath = filepath_new + "\\" + filename_new;
            FileStream filestream = new FileStream(savePath, FileMode.Create); ;
            files[0].CopyTo(filestream);
            filestream.Flush();
            filestream.Close();
            filestream.Dispose();

            resultHelper.Msg = "文件上传成功";
            return resultHelper.GetResultInfo();
        }
        /// <summary>
        /// 多个文件
        /// </summary>
        /// <returns></returns>
        [Route("upfiles")]
        public string UpFiles()
        {
            ResultHelper resultHelper = new ResultHelper();
            string filepath = Request.Form["filepath"];
            string filename = Request.Form["filename"];
            IFormFileCollection files = Request.Form.Files;
            FileStream filestream = null;
            for (int i = 0; i < files.Count; i++)
            {
                string filename_new = "";
                //如果有文件名称则取给的名称
                if (filename != null)
                {
                    filename_new = filename;
                }
                else
                {
                    string newname = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    string[] fileform = files[i].FileName.Split('.');
                    newname += "." + fileform[fileform.Length - 1];
                    filename_new = newname;
                }
                //如果有文件路径则取给的路径
                string filepath_new = "";
                if (filepath != null)
                {
                    filepath_new = host.ContentRootPath + "\\" + filepath;
                }
                else
                {
                    filepath_new = host.ContentRootPath + "\\files";
                }
                //判断是否存在文件夹，不存在则创建
                if (!System.IO.Directory.Exists(filepath_new))
                {
                    Directory.CreateDirectory(filepath_new);
                }
                string savePath = filepath_new + "\\" + filename_new;
                filestream = new FileStream(savePath, FileMode.Create); ;
                files[i].CopyTo(filestream);
                filestream.Flush();
            }
            filestream.Close();
            filestream.Dispose();
            resultHelper.Msg = "文件上传成功";
            return resultHelper.GetResultInfo();
        }

        /// <summary>
        /// 断点上传文件 二进制文件
        /// </summary>
        /// <returns></returns>
        [Route("isbreakpointbybinary")]
        [BinaryFileFilter]
        public async Task<string> IsBreakPointByBinary()
        {
            ResultHelper resultHelper = new ResultHelper();
            string result = await Request.StreamFilesAsync(host.ContentRootPath);
            resultHelper.UseId = result;
            resultHelper.Msg = "上传成功";
            return resultHelper.GetResultInfo();

        }
        /// <summary>
        /// 断点上传文件 块文件
        /// </summary>
        /// <returns></returns>
        [Route("isbreakpointbyfile")]
        public string IsBreakPointByFile()
        {
            ResultHelper resultHelper = new ResultHelper();
            //切片MD5值   
            string fileMd5 = Request.Form["fileMd5"];
            //当前分片在上传分片中的顺序（从0开始）
            int chunk = Convert.ToInt32(Request.Form["chunk"]);
            //总分片数
            int chunks = Convert.ToInt32(Request.Form["chunks"]);
            //要创建的文件夹名字
            string name_wjj = fileMd5;
            //创建文件夹
            string path_fp_dz = host.ContentRootPath + "\\files\\" + name_wjj;
            //判断文件夹是否存在，不存在创建文件夹
            if (!System.IO.Directory.Exists(path_fp_dz))
            {
                Directory.CreateDirectory(path_fp_dz);
            }
            //每个分片的临时存储位置
            //每个分片用数字命名
            string path_fp = path_fp_dz + "\\" + chunk;
            //获取上传的文件分片
            IFormFileCollection files = Request.Form.Files;
            //保存上传的文件分片
            FileStream filestream = new FileStream(path_fp, FileMode.Create);
            files[0].CopyTo(filestream);
            resultHelper.Msg = "上传成功";
            return resultHelper.GetResultInfo();
        }
        /// <summary>
        ///  检测文件切片是否存在和完整性
        /// </summary>
        /// <returns>已存在：1 不存在及数据不完整：0</returns>
        public string Detection()
        {
            ResultHelper resultHelper = new ResultHelper();
            //切片MD5值
            string fileMd5 = Request.Form["fileMd5"];
            //切片索引
            string chunk = Request.Form["chunk"];
            //切片大小
            string chunkSize = Request.Form["chunkSize"];
            //文件夹路径                                                  
            string wjj_path = host.ContentRootPath + "\\files\\" + fileMd5;
            //文件路径
            string wj_path = wjj_path + "\\" + chunk;
            //判断文件夹和文件片段是否存在
            if (!System.IO.Directory.Exists(wjj_path) || !System.IO.File.Exists(wj_path))
            {
                resultHelper.Msg = "0";
            }
            else
            {
                //判断文件大小是否一样防止数据不完整
                long size = System.IO.File.OpenRead(wj_path).Length;
                if (chunkSize == size.ToString())
                {
                    resultHelper.Msg = "1";
                }
                else
                {
                    resultHelper.Msg = "0";
                }
            }
            return resultHelper.GetResultInfo();
        }

        /// <summary>
        /// 合并分片
        /// </summary>
        /// <returns></returns>
        [Route("mergebinary")]
        public string MergeBinary()
        {
            ResultHelper resultHelper = new ResultHelper();
            string filename = Request.Form["filename"];
            //是否采用文件原名称
            string isoldname = Request.Form["isoldname"];
            if (isoldname == "0")
            {
                string newname = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                string[] fileform = filename.Split('.');
                newname += "." + fileform[fileform.Length - 1];
                filename = newname;
            }
            string path_wj = host.ContentRootPath + "\\files\\" + filename;
            //切片MD5值
            string name_wjj = Request.Form["fileMd5"];
            string path_fp_dz = host.ContentRootPath + "\\files\\" + name_wjj;
            //获取所有的分片路径
            string[] file_fp = Directory.GetFiles(path_fp_dz);
            IComparer<string> paixu = new PaiXuIComparer();
            List<string> fenpian = new List<string>();
            fenpian.AddRange(file_fp);
            //排序文件，防止文件顺序错乱导致合并文件错误
            fenpian.Sort(paixu);
            FileStream fileout = null;
            FileStream write = null;
            try
            {
                //此处应大于等于传过来分片的大小，否则出现错误
                byte[] buffer = new byte[10 * 1024];
                fileout = new FileStream(path_wj, FileMode.Create);

                //读取分片字节的实际大小
                int readedLen = 0;
                for (int i = 0; i < fenpian.Count; i++)
                {
                    //读取某个分片
                    write = new FileStream(fenpian[i], FileMode.Open);
                    while ((readedLen = write.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        //把读取到的分片字节写入到要生成的文件中
                        fileout.Write(buffer, 0, readedLen);
                        fileout.Flush();
                    }
                    write.Close();
                    write.Dispose();

                }
                fileout.Close();
                fileout.Dispose();
            }
            catch (Exception e)
            {

                throw e;
            }
            finally
            {
                //释放资源，目的：防止文件被占用
                if (write != null)
                {
                    try
                    {
                        write.Close();
                        write.Dispose();
                    }
                    catch (Exception e)
                    {

                        throw e;
                    }
                    finally
                    {
                        write = null;
                    }
                }
                if (fileout != null)
                {
                    try
                    {
                        fileout.Close();
                        fileout.Dispose();
                    }
                    catch (Exception e)
                    {

                        throw e;
                    }
                    finally
                    {
                        fileout = null;
                    }
                }
                //删除文件
                for (int i = 0; i < fenpian.Count; i++)
                {
                    FileInfo info = new FileInfo(fenpian[i]);
                    info.Delete();
                }
                //删除目录
                Directory.Delete(path_fp_dz);

            }
            resultHelper.UseId = filename;
            resultHelper.Msg = "合并成功";
            return resultHelper.GetResultInfo();
        }
    }

}