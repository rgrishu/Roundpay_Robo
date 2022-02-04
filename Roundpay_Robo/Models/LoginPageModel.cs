﻿using Roundpay_Robo.AppCode.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Roundpay_Robo.Models
{
    public class LoginPageModel
    {
        public int WID { get; set; }
        public int PageID { get; set; }
        public int ThemeID { get; set; }
        public string Host { get; set; }
        public string AppName { get; set; }
        public string CustomerCareMobileNos { get; set; }
        public string CustomerPhoneNos { get; set; }
        public IEnumerable<BannerImage> BGServiceImgURLs { get; set; }
        public ReferralRoleMaster referralRoleMaster { get; set; }
    }
}
