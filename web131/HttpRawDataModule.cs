using System;
using System.Web;

namespace web131
{
    public class HttpRawDataModule : IHttpModule
    {
        /// <summary>
        /// 您将需要在您网站的 web.config 文件中配置此模块，
        /// 并向 IIS 注册此模块，然后才能使用。有关详细信息，
        /// 请参见下面的链接: http://go.microsoft.com/?linkid=8101007
        /// </summary>
        #region IHttpModule Members

        public void Dispose()
        {
            //此处放置清除代码。

        }

        public void Init(HttpApplication context)
        {
            //绑定事件，在对此请求处理过程全部结束后进行过滤操作
            
            //context.ReleaseRequestState += new EventHandler(context_ReleaseRequestState);
            context.AcquireRequestState += new EventHandler(context_ReleaseRequestState);
            context.BeginRequest+=new  EventHandler(context_BeginRequest); 
             
        }

        #endregion

        /// <summary>  
        /// 开始处理请求事件  
        /// </summary>  
        /// <param name="sender"></param>  
        /// <param name="args"></param>  
        private void context_BeginRequest(object sender, EventArgs args)
        {
            //获取HttpApplication  
            HttpApplication application = sender as HttpApplication;
            //获取用户请求的URL  
            string oldUrl = application.Request.RawUrl;
            //如果请求中存在“BookDetail”字符进行处理请求  
            if (oldUrl.IndexOf("BookDetail") > 0)
            {
                //截取BookDetail之前位置的字符串  
                string newUrl = oldUrl.Substring(0, oldUrl.IndexOf("BookDetail"));
                //请求的新字符串为"BookDetail.aspx?bid=XXXXXXXX"  
                newUrl = newUrl + "BookDetail.aspx?bid=" + oldUrl.Substring(oldUrl.LastIndexOf("_") + 1,
                    (oldUrl.IndexOf(".") - oldUrl.LastIndexOf("_") - 1));
                //将请求中的URL进行重写  
                //application.Context.RewritePath(newUrl);
                application.Context.RewritePath("Default.aspx");
            }


        }  

        /// <summary>
        /// 对此HTTP请求处理的过程全部结束
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void context_ReleaseRequestState(object sender, EventArgs e)
        {
            HttpApplication application = (HttpApplication)sender;

            //这里需要针对ASPX页面进行拦截，测试发现如果不这么做，Wap 访问站点图片容易显示为X，奇怪
            string[] temp = application.Request.CurrentExecutionFilePath.Split('.');
            string pageT = temp[temp.Length - 1].ToLower();
            if (temp.Length > 0 && (pageT == "aspx" || pageT == "htm"))
            {
                //装配过滤器
                string usr = "";
                if (application.Context.Session != null)
                {
                    if (application.Context.Session["usr"] != null)
                    {
                        usr = application.Context.Session["usr"].ToString();
                    }
                }
                application.Response.Filter = new RawFilter(application.Response.Filter, usr);

                //绑定过滤器事件
                RawFilter filter = (RawFilter)application.Response.Filter;
                filter.OnRawDataRecordedEvent += new EventHandler<RawDataEventArgs>(filter_OnRawDataRecordedEvent);
            }
        }

        /// <summary>
        /// 当原始数据采集到以后，入库 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void filter_OnRawDataRecordedEvent(object sender, RawDataEventArgs e)
        {
            string allcode = e.SourceCode;
            //WapSite.SiteDataClass wapdata = new WapSite.SiteDataClass();
            //wapdata.WriteRawDataLog(allcode);
        }

        public void OnLogRequest(Object source, EventArgs e)
        {
            //可以在此放置自定义日志记录逻辑
        }
    }
}
