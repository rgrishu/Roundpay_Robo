using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Roundpay_Robo.AppCode.Interfaces;
using Roundpay_Robo.AppCode.Model;
using Roundpay_Robo.AppCode.Model.ProcModel;
using System.Data.SqlClient;
using System.Data;
using Roundpay_Robo.AppCode.DB;
using Roundpay_Robo.AppCode.Model.App;
using Roundpay_Robo.AppCode.Model;
using Roundpay_Robo.AppCode.StaticModel;
using Roundpay_Robo.AppCode.Interfaces;

namespace Roundpay_Robo.AppCode.DL
{
    public class ProcProjectValidate : IProcedure
    {
        private readonly IDAL _dal;
        public ProcProjectValidate(IDAL dal) => _dal = dal;
        public object Call(object obj)
        {
            var req = (Authentication)obj;
            DataTable dt = new DataTable();
            dt.Columns.Add("ServiceId", typeof(int));
            dt.Columns.Add("IsActive", typeof(bool));
            dt.Columns.Add("IsVisible", typeof(bool));
            foreach (var item in req.Services)
            {
                var nRow = dt.NewRow();
                nRow["ServiceId"] = item.ID;
                nRow["IsActive"] = item.IsActive == "True" ? true : false;
                nRow["IsVisible"] = item.IsVisible == "True" ? true : false;
                dt.Rows.Add(nRow);
            }

            var _res = new ResponseStatus
            {
                Statuscode = ErrorCodes.Minus1,
                Msg = ErrorCodes.TempError
            };
            try
            {
                SqlParameter[] param = {
                new SqlParameter("@VTType",dt)
            };
                DataTable data = _dal.GetByProcedure(GetName(), param);
                if (data.Rows.Count > 0)
                {
                    _res.Statuscode = Convert.ToInt32(data.Rows[0][0]);
                    _res.Msg = data.Rows[0]["Msg"].ToString();
                }
            }
            catch (Exception ex)
            {

            }
            return _res;
        }
        public object Call() => throw new NotImplementedException();

        public string GetName() => "Proc_ProjectValidate";
    }
}
