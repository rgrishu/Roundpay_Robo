using Microsoft.AspNetCore.Mvc;
using Roundpay_Robo.AppCode.DB;
using Roundpay_Robo.AppCode.Interfaces;
using Roundpay_Robo.AppCode.Model;
using Roundpay_Robo.AppCode.Model.ProcModel;
using System.Data;
using System.Data.SqlClient;

namespace Roundpay_Robo.AppCode.DL
{
    public class ProcGetEmailSettingByFormat : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetEmailSettingByFormat(IDAL dal) => _dal = dal;
        public string GetName() => "proc_GetEmailSettingByFormat";
        public object Call(object obj)
        {
            CommonReq req = (CommonReq)obj;
            SqlParameter[] param = {
                new SqlParameter("@LoginID",req.LoginID),
                new SqlParameter("@FormatID",req.CommonInt),
                new SqlParameter("@WID",req.CommonInt2),
                new SqlParameter("@isSendFailed",req.CommonBool),
            };
            var _res = new EmailSettingswithFormat();
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    _res.Statuscode = Convert.ToInt32(dt.Rows[0][0]);
                    _res.Msg = Convert.ToString(dt.Rows[0]["Msg"]);
                    if ((dt.Rows[0][0] is DBNull ? -1 : Convert.ToInt32(dt.Rows[0][0])) == 1)
                    {
                        _res.ID = Convert.ToInt32(dt.Rows[0]["_ID"]);
                        _res.FormatID = dt.Rows[0]["_FormatID"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_FormatID"]);
                        _res.Subject = Convert.ToString(dt.Rows[0]["_Subject"]);
                        _res.EmailTemplate = Convert.ToString(dt.Rows[0]["_EmailTemplate"]);
                        _res.IsEnableEmail = dt.Rows[0]["_IsEnableEmail"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsEnableEmail"]);
                        _res.FromEmail = Convert.ToString(dt.Rows[0]["_FromEmail"]);
                        _res.HostName = Convert.ToString(dt.Rows[0]["_HostName"]);
                        _res.Port = dt.Rows[0]["_Port"] is DBNull ? 0 : Convert.ToInt32(dt.Rows[0]["_Port"]);
                        _res.MailUserID = Convert.ToString(dt.Rows[0]["_MailUserID"]);
                        _res.Password = Convert.ToString(dt.Rows[0]["_Password"]);
                        _res.IsSSL = dt.Rows[0]["_IsSSL"] is DBNull ? false : Convert.ToBoolean(dt.Rows[0]["_IsSSL"]);
                        _res.SaleEmail = Convert.ToString(dt.Rows[0]["_SaleMailID"]);
                    }
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
            return _res;
        }
        public object Call() => throw new NotImplementedException();
    }
}
