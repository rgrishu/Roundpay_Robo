﻿using Microsoft.AspNetCore.Mvc;
using Roundpay_Robo.AppCode.DB;
using Roundpay_Robo.AppCode.Interfaces;
using Roundpay_Robo.AppCode.Model;
using Roundpay_Robo.AppCode.Model.ProcModel;
using System.Data;

namespace Roundpay_Robo.AppCode.DL
{
    public class ProcGetMasterSocialType : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetMasterSocialType(IDAL dal) => _dal = dal;
        public object Call(object obj) => throw new NotImplementedException();
        public object Call()
        {
            var _res = new ResponseStatus();
            try
            {
                DataTable dt = _dal.Get(GetName());
                if (dt.Rows.Count > 0)
                {
                    _res.CommonStr = Convert.ToString(dt.Rows[0]["_Ids"]);
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _res;
        }
        public string GetName() => @"declare @temp varchar(250)
                                        SET @temp = ''
                                    select @temp=@temp+ Cast(_ID as varchar) +',' from MASTER_SocialAlertType where _ISActive=1
                                    select SubString(@temp,0,LEN(@temp)) _Ids";
    }
}