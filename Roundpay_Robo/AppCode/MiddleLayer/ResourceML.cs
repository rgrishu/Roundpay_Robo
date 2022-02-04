using Roundpay_Robo.AppCode;
using Roundpay_Robo.AppCode.Configuration;
using Roundpay_Robo.AppCode.DB;
using Roundpay_Robo.AppCode.Interfaces;
using Roundpay_Robo.AppCode.Model;
using Roundpay_Robo.AppCode.StaticModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Roundpay_Robo.AppCode.DL;
using Roundpay_Robo.AppCode.Interfaces;
using Roundpay_Robo.AppCode.Model;
using Roundpay_Robo.AppCode.Model.ProcModel;
using Roundpay_Robo.AppCode.StaticModel;
using Roundpay_Robo.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Validators;

namespace Roundpay_Robo.AppCode.MiddleLayer
{
    public class ResourceML : IResourceML, IBannerML
    {
        IHttpContextAccessor _accessor;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _env;
        private readonly IRequestInfo _rinfo;
        private readonly IDAL _dal;
        private readonly IConnectionConfiguration _c;
        public ResourceML(IHttpContextAccessor accessor, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            _accessor = accessor;
            _env = env;
            _rinfo = new RequestInfo(_accessor, _env);
            _c = new ConnectionConfiguration(_accessor, _env);
            _dal = new DAL(_c.GetConnectionString());
        }
     

    }
}
