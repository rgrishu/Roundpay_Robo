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
using RoundpayFinTech.AppCode.StaticModel;

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
        #region Ranjana
        public void CreateWebsiteDirectory(int WID, string _FolderType)
        {
            //For Website Folder
            if (_FolderType.Equals(FolderType.Website))
            {
                string _Path = DOCType.WebsiteFolderPath.Replace("{0}", WID.ToString());
                string _PathBanner = DOCType.BannerSitePath.Replace("{0}", WID.ToString());
                string _PathApp = DOCType.BannerUserPath.Replace("{0}", WID.ToString());
                string _PathPopup = DOCType.PopupPath.Replace("{0}", WID.ToString());
                string _PathTheme = DOCType.ThemePath.Replace("{0}", WID.ToString());


                if (!(Directory.Exists(_Path)))
                {
                    Directory.CreateDirectory(_Path);
                    Directory.CreateDirectory(_PathBanner);
                    Directory.CreateDirectory(_PathApp);
                    Directory.CreateDirectory(_PathPopup);
                    Directory.CreateDirectory(_PathTheme);

                }
                string _SFileName = "";
                string _DFileName = "";
                string _TFileName = "";

                string[] _FilePath = Directory.GetFiles(DOCType.DefaultFolderPath.Replace("{0}", "0"));
                foreach (var _Files in _FilePath)
                {
                    _SFileName = Path.GetFileName(_Files);
                    if (_SFileName != "Noimage.png")
                    {
                        _DFileName = _Path + "/" + _SFileName;
                    }
                    else
                    {
                        _DFileName = _PathPopup + "/" + _SFileName;
                    }
                    if (!File.Exists(_DFileName))
                    {
                        File.Copy(_Files, _DFileName);
                    }
                }
                string[] _FilePathTheme = Directory.GetFiles(DOCType.DefaultFolderTheme.Replace("{0}", "1"));
                foreach (var _FilesTheme in _FilePathTheme)
                {
                    _TFileName = Path.GetFileName(_FilesTheme);
                    _DFileName = _PathTheme + "/" + _TFileName;
                    if (!File.Exists(_DFileName))
                    {
                        File.Copy(_FilesTheme, _DFileName);
                    }
                }


            }
        }
        #endregion
        public StringBuilder GetLogoURL(int WID)
        {
            StringBuilder sb = new StringBuilder();
            var crf = _rinfo.GetCurrentReqInfo();
            sb.Append(crf.Scheme);
            sb.Append("://");
            sb.Append(crf.Host);
            if (crf.Port > 0)
            {
                sb.Append(":");
                sb.Append(crf.Port);
            }
            sb.Append("/");
            sb.AppendFormat(DOCType.LogoSuffix, WID);
            return sb;
        }

    }
}
