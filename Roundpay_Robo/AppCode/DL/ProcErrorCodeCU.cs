﻿using Roundpay_Robo.AppCode.DB;
using Roundpay_Robo.AppCode.Interfaces;
using Roundpay_Robo.AppCode.Model;
using Roundpay_Robo.AppCode.Model.ProcModel;
using Roundpay_Robo.AppCode.StaticModel;
using RoundpayFinTech.AppCode.Model;
using RoundpayFinTech.AppCode.Model.ProcModel;
using System;
using System.Data.SqlClient;

namespace Roundpay_Robo.AppCode.DL
{
    public class ProcErrorCodeCU : IProcedure
    {
        private readonly IDAL _dal;
        public ProcErrorCodeCU(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (ErrorCodeDetailReq)obj;
            var res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            SqlParameter[] param = {
                new SqlParameter("@LT",req.LoginTypeID),
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@ID",req.Detail.EID),
                new SqlParameter("@Error",req.Detail.Error??""),
                new SqlParameter("@Code",req.Detail.Code??""),
                new SqlParameter("@Status",req.Detail.Status),
                new SqlParameter("@Type",req.Detail.ErrType),
                new SqlParameter("@IsDown",req.Detail.IsDown),
                new SqlParameter("@IsResend",req.Detail.IsResend),
                new SqlParameter("@IsCode",req.Detail.IsCode),                
                new SqlParameter("@IP",req.CommonStr??""),
                new SqlParameter("@Browser",req.CommonStr2??"")
            };

            try
            {
                var dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    res.Statuscode = Convert.ToInt16(dt.Rows[0][0]);
                    res.Msg = dt.Rows[0]["Msg"] is DBNull ? "" : dt.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = req.LoginTypeID,
                    UserId = req.LoginID
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return res;
        }
        public object Call() => throw new NotImplementedException();
        public string GetName() => "proc_ErrorCodeCU";
    }
}
