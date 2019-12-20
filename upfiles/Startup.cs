using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.Web;
using upfiles.config;

namespace upfiles
{
    public class Startup
    {
        public IConfiguration Configuration;
        public Startup(IConfiguration Configuration)
        {
            this.Configuration = Configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            //获取跨域配置的地址
            string[] corsoptions = Configuration.GetSection("AllowOrigin:Origin").Value.Split(',');
            //跨域设置
            services.AddCors(options => options.AddPolicy("CorsOptions",
            p => p.WithOrigins(corsoptions).AllowAnyMethod().AllowAnyHeader().AllowCredentials()));
            services.AddMvc(options =>
            {
                //自定义异常处理
                options.Filters.Add<MyExceptionFilter>();
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //启用跨域设置 必须在app.UseMvc之前
            app.UseCors("CorsOptions");
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=user}/{action=userpageview}/{id?}");
            });
            //使用NLog作为日志记录工具
            loggerFactory.AddNLog();
            //引入Nlog配置文件（可省略）。如果是其他名字非Nlog.config，则必须加上这一行代码。
            env.ConfigureNLog("Nlog.config.xml");
        }
    }
}
