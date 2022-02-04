using Roundpay_Robo.AppCode.DB;
using Roundpay_Robo.AppCode.Interfaces;
using Roundpay_Robo.AppCode.Model;
using Roundpay_Robo.AppCode.StaticModel;
using Roundpay_Robo.AppCode.Model;
using Roundpay_Robo.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Roundpay_Robo.AppCode.DL
{
    public class ProcGetRoleForReferral : IProcedure
    {
        private readonly IDAL _dal;
        public ProcGetRoleForReferral(IDAL dal) => _dal = dal;
        public string GetName() => "proc_GetRoleForReferral";
        public object Call(object obj)
        {
            var _userID = obj;
            var _roleMaster = new List<RoleMaster>();

            SqlParameter[] param =
            {
                new SqlParameter("@UserID",_userID)
            };
            try
            {
                DataTable dt = _dal.GetByProcedure(GetName(), param);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        //_id , _Role ,_ind
                        var _rm = new RoleMaster
                        {
                            ID = Convert.ToInt32(row["_id"]),
                            Role = row["_Role"] is DBNull ? "" : row["_Role"].ToString(),
                            Ind = Convert.ToInt32(row["_ind"])
                        };
                        _roleMaster.Add(_rm);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = "Call",
                    Error = ex.Message,
                    LoginTypeID = 1,
                    UserId = 1
                };
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            return _roleMaster;
        }

        public object Call() => throw new NotImplementedException();


    }
}