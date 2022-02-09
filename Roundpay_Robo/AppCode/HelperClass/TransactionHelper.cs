using Roundpay_Robo.AppCode.Configuration;
using Roundpay_Robo.AppCode.DB;
using Roundpay_Robo.AppCode.Interfaces;
using Roundpay_Robo.AppCode.Model;
using Roundpay_Robo.AppCode.StaticModel;
using Roundpay_Robo.AppCode.WebRequest;
using Roundpay_Robo.AppCode.MiddleLayer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Roundpay_Robo;
using Roundpay_Robo.AppCode.DL;
using Roundpay_Robo.AppCode.Interfaces;
using Roundpay_Robo.AppCode.MiddleLayer;
using Roundpay_Robo.AppCode.Model;
using Roundpay_Robo.AppCode.Model.ProcModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Validators;
using RoundpayFinTech.AppCode.Model.ProcModel;

namespace Roundpay_Robo.AppCode.HelperClass
{
    public class TransactionHelper
    {
        private readonly IDAL _dal;
        private readonly ToDataSet _toDataSet;
        private readonly IErrorCodeMLParent _errCodeML;
        private readonly IHttpContextAccessor _accessor;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _env;
        public TransactionHelper(IDAL dal, IHttpContextAccessor accessor, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            _dal = dal;
            _toDataSet = ToDataSet.O;
            _errCodeML = new ErrorCodeML(_dal);
            _accessor = accessor;
            _env = env;
        }
        #region TransactionRelated

        public async Task<TransactionStatus> MatchResponse(int OID, RechargeAPIHit rechargeAPIHit, string AccountNoKey)
        {
            const string MethodName = "MatchResponse";
            var tstatus = new TransactionStatus
            {
                Status = RechargeRespType.PENDING,
                APIID = rechargeAPIHit.aPIDetail.ID,
                APIType = rechargeAPIHit.aPIDetail.APIType,
                APIOpCode = rechargeAPIHit.aPIDetail.APIOpCode,
                APIName = rechargeAPIHit.aPIDetail.Name,
                APICommType = rechargeAPIHit.aPIDetail.CommType,
                APIComAmt = rechargeAPIHit.aPIDetail.Comm,
                APIGroupCode = rechargeAPIHit.aPIDetail.GroupCode,
                ErrorCode = ErrorCodes.Request_Accpeted.ToString(),
                ErrorMsg = nameof(ErrorCodes.Request_Accpeted),
                SwitchingID = rechargeAPIHit.aPIDetail.SwitchingID
            };
            try
            {
                var IsOtherCondition = false;
                var OtherConditionMatchString = string.Empty;
                tstatus.IsResend = false;
                tstatus.IsReLoginLapu = false;
                var errorCodes = new ErrorCodeDetail();
                var ds = new DataSet();
                if (rechargeAPIHit.aPIDetail.ResponseTypeID == ResponseType.JSON)
                {
                    ds = _toDataSet.ReadDataFromJson(rechargeAPIHit.Response);
                }
                else
                {
                    #region MatchResponseFor String,CSV and Other
                    IsOtherCondition = true;
                    OtherConditionMatchString = rechargeAPIHit.Response ?? string.Empty;
                    #endregion
                }
                #region MatchResponseForXML&JSON
                var IsErrorCodeFound = false;
                var IsMessageFound = false;
                if (ds.Tables.Count > 0 && !IsOtherCondition)
                {
                    bool IsStatusFound = string.IsNullOrEmpty(rechargeAPIHit.aPIDetail.StatusName),

                        IsVendorIDFound = string.IsNullOrEmpty(rechargeAPIHit.aPIDetail.VendorID),
                        IsOperatorIDFound = string.IsNullOrEmpty(rechargeAPIHit.aPIDetail.LiveID),
                        IsRefKeyFound = string.IsNullOrEmpty(rechargeAPIHit.aPIDetail.RefferenceKey),
                        IsBalanceKeyFound = string.IsNullOrEmpty(rechargeAPIHit.aPIDetail.BalanceKey);
                   
                    foreach (DataTable tbl in ds.Tables)
                    {

                        if (!IsStatusFound)
                        {
                            var STn = CheckIfKeyExistsInDatatable(tbl, rechargeAPIHit.aPIDetail.StatusName);
                            if (STn.CommonBool)
                            {
                                IsStatusFound = true;
                                string StatusValue = tbl.Rows[0][STn.CommonStr].ToString().ToUpper();
                                if (!string.IsNullOrEmpty(rechargeAPIHit.aPIDetail.SuccessCode))
                                {
                                        //if (tstatus.Status == LapuFailCode.SessionExpired)
                                        //{
                                        //    tstatus.IsResend = true;

                                        //    return tstatus;
                                        //}
                                    var sucValArr = rechargeAPIHit.aPIDetail.SuccessCode.ToUpper().Split(',');
                                    tstatus.Status = sucValArr.Contains(StatusValue) ? RechargeRespType.SUCCESS : tstatus.Status;
                                    if (tstatus.Status == RechargeRespType.PENDING && !string.IsNullOrEmpty(rechargeAPIHit.aPIDetail.FailCode))
                                    {
                                        var failValArr = rechargeAPIHit.aPIDetail.FailCode.ToUpper().Split(',');
                                        tstatus.Status = failValArr.Contains(StatusValue) ? RechargeRespType.FAILED : tstatus.Status;
                                    }
                                    
                                }
                            }
                        }
                        if (!IsOperatorIDFound)
                        {
                            var LVd = CheckIfKeyExistsInDatatable(tbl, rechargeAPIHit.aPIDetail.LiveID);
                            if (LVd.CommonBool)
                            {
                                IsOperatorIDFound = true;
                                tstatus.OperatorID = tbl.Rows[0][LVd.CommonStr].ToString();
                            }
                        }
                        if (!IsVendorIDFound)
                        {
                            var VnD = CheckIfKeyExistsInDatatable(tbl, rechargeAPIHit.aPIDetail.VendorID);
                            if (VnD.CommonBool)
                            {
                                IsVendorIDFound = true;
                                tstatus.VendorID = tbl.Rows[0][VnD.CommonStr].ToString();
                            }
                        }
                        if (!IsErrorCodeFound && tstatus.Status != RechargeRespType.SUCCESS)
                        {
                            var ECD = CheckIfKeyExistsInDatatable(tbl, rechargeAPIHit.aPIDetail.ErrorCodeKey);
                            if (ECD.CommonBool)
                            {
                                IsErrorCodeFound = true;
                                tstatus.APIErrorCode = tbl.Rows[0][ECD.CommonStr].ToString();

                                if (tstatus.APIErrorCode == LapuFailCode.SessionExpired)
                                {
                                    tstatus.Status = RechargeRespType.PENDING;
                                    tstatus.IsResend = true;
                                    tstatus.IsReLoginLapu = true;
                                    return tstatus;
                                }
                            }
                        }
                        if (!IsMessageFound && tstatus.Status != RechargeRespType.SUCCESS)
                        {
                            var MGK = CheckIfKeyExistsInDatatable(tbl, rechargeAPIHit.aPIDetail.MsgKey);
                            if (MGK.CommonBool)
                            {
                                IsMessageFound = true;
                                tstatus.APIMsg = tbl.Rows[0][MGK.CommonStr].ToString();
                            }
                        }
                        var RFK = CheckIfKeyExistsInDatatable(tbl, rechargeAPIHit.aPIDetail.RefferenceKey);
                        if (!IsRefKeyFound && RFK.CommonBool)
                        {
                            IsRefKeyFound = true;
                            tstatus.RefferenceID = tbl.Rows[0][RFK.CommonStr].ToString();
                        }
                        if (!IsBalanceKeyFound && tstatus.Status == RechargeRespType.SUCCESS)
                        {
                            var BCK = CheckIfKeyExistsInDatatable(tbl, rechargeAPIHit.aPIDetail.BalanceKey);
                            if (BCK.CommonBool)
                            {
                                IsBalanceKeyFound = true;
                                tstatus.Balance = tbl.Rows[0][BCK.CommonStr].ToString();
                            }
                        }
                        if (IsStatusFound && IsVendorIDFound && IsOperatorIDFound && IsErrorCodeFound && IsMessageFound && IsRefKeyFound)
                            break;
                    }
                }


                if (tstatus.Status.In(RechargeRespType.PENDING, RechargeRespType.FAILED) && (ds.Tables.Count > 0 || rechargeAPIHit.aPIDetail.ResponseTypeID == ResponseType.Delimiter) && !IsOtherCondition)
                {
                    if (IsErrorCodeFound && (tstatus.APIErrorCode ?? string.Empty).Length > 0 && (tstatus.APIGroupCode ?? string.Empty).Length > 0)
                    {
                        var apiErrorCodes = _errCodeML.GetAPIErrorCode(new APIErrorCode { APICode = tstatus.APIErrorCode, GroupCode = tstatus.APIGroupCode });
                        if (tstatus.Status == RechargeRespType.FAILED)
                        {
                            apiErrorCodes.ECode = string.IsNullOrEmpty(apiErrorCodes.ECode) ? ErrorCodes.Unknown_Error.ToString() : apiErrorCodes.ECode;
                        }
                        if ((apiErrorCodes.ECode ?? string.Empty).Length > 0)
                        {
                            errorCodes = _errCodeML.Get(apiErrorCodes.ECode);
                            if (errorCodes.Status == RechargeRespType.FAILED && tstatus.Status == RechargeRespType.PENDING)
                            {
                                tstatus.Status = RechargeRespType.FAILED;
                            }
                        }
                    }
                    else if (IsMessageFound)
                    {
                        IsOtherCondition = true;
                        OtherConditionMatchString = tstatus.APIMsg;
                    }
                }
                if (IsOtherCondition && (OtherConditionMatchString ?? string.Empty).Trim().Length > 0)
                {
                    var apistatuscheck = new APISTATUSCHECK
                    {
                        Msg = Validate.O.ReplaceAllSpecials(OtherConditionMatchString).Trim()
                    };
                    var _proc = new ProcCheckTextResponse(_dal);
                    apistatuscheck = (APISTATUSCHECK)await _proc.Call(apistatuscheck).ConfigureAwait(false);
                    if (apistatuscheck.Statuscode == ErrorCodes.One)
                    {
                        tstatus.Status = apistatuscheck.Status.In(RechargeRespType.PENDING, RechargeRespType.FAILED, RechargeRespType.SUCCESS) ? apistatuscheck.Status : RechargeRespType.PENDING;
                        if (tstatus.Status == RechargeRespType.FAILED)
                        {
                            errorCodes = _errCodeML.Get(string.IsNullOrEmpty(apistatuscheck.ErrorCode) ? ErrorCodes.Unknown_Error.ToString() : apistatuscheck.ErrorCode);
                        }
                        else
                        {
                            errorCodes = _errCodeML.Get(string.IsNullOrEmpty(apistatuscheck.ErrorCode) ? ErrorCodes.Request_Accpeted.ToString() : apistatuscheck.ErrorCode);
                            if (errorCodes.Status == RechargeRespType.FAILED && tstatus.Status == RechargeRespType.PENDING)
                            {
                                tstatus.Status = RechargeRespType.FAILED;
                            }
                        }

                        if (errorCodes.ErrType == ErrType.BillFetch)
                        {
                            tstatus.Status = RechargeRespType.PENDING;
                        }
                        if (tstatus.Status == RechargeRespType.FAILED)
                        {
                            if (errorCodes.Status == 0)
                            {
                                apistatuscheck.OperatorID = string.IsNullOrEmpty(apistatuscheck.ErrorCode) ? (errorCodes.Error ?? nameof(ErrorCodes.Unknown_Error).Replace("_", " ")) : nameof(ErrorCodes.Unknown_Error).Replace("_", " ");
                                errorCodes.Code = apistatuscheck.ErrorCode = string.IsNullOrEmpty(apistatuscheck.ErrorCode) ? ErrorCodes.Unknown_Error.ToString() : apistatuscheck.ErrorCode;
                                errorCodes.Error = apistatuscheck.OperatorID;
                            }
                            else
                            {
                                apistatuscheck.OperatorID = errorCodes.Error;
                            }
                        }
                        tstatus.OperatorID = apistatuscheck.OperatorID;
                        tstatus.VendorID = apistatuscheck.VendorID;
                        tstatus.ErrorCode = errorCodes.Code;
                        tstatus.ErrorMsg = errorCodes.Error;
                    }
                }
                if (errorCodes.Status > 0)
                {
                    tstatus.ErrorCode = errorCodes.Code;
                    tstatus.ErrorMsg = (errorCodes.Error ?? string.Empty).Replace(Replacement.REPLACE, tstatus.APIMsg);
                    if (!string.IsNullOrEmpty(AccountNoKey))
                    {
                        tstatus.ErrorMsg = tstatus.ErrorMsg.Replace(Replacement.AccountKey, AccountNoKey);
                    }
                    else
                    {
                        tstatus.ErrorMsg = tstatus.ErrorMsg.Replace(Replacement.AccountKey, "Account/Mobile Number");
                    }
                    if (errorCodes.Status == RechargeRespType.FAILED)
                    {
                        tstatus.Status = RechargeRespType.FAILED;
                        tstatus.OperatorID = tstatus.ErrorMsg;

                        if (errorCodes.IsDown && rechargeAPIHit.aPIDetail.IsOpDownAllow)
                        {
                            await UpdateAPIDownStatus(OID, rechargeAPIHit.aPIDetail.ID).ConfigureAwait(false);
                            var errorLog = new ErrorLog
                            {
                                ClassName = GetType().Name,
                                FuncName = MethodName,
                                Error = "APIDown:" + rechargeAPIHit.aPIDetail.ID + ":" + rechargeAPIHit.aPIDetail.IsOpDownAllow,
                                LoginTypeID = LoginType.ApplicationUser,
                                UserId = OID
                            };
                            var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                        }
                        else if (errorCodes.IsResend)
                        {
                            tstatus.IsResend = true;
                            var errorLog = new ErrorLog
                            {
                                ClassName = GetType().Name,
                                FuncName = MethodName,
                                Error = "ResendCondFound:" + (errorCodes.Error ?? "") + "|" + rechargeAPIHit.aPIDetail.ID,
                                LoginTypeID = LoginType.ApplicationUser,
                                UserId = OID
                            };
                            var _ = new ProcPageErrorLog(_dal).Call(errorLog);
                        }
                    }
                    else if (errorCodes.Status == RechargeRespType.SUCCESS)
                    {
                        tstatus.Status = RechargeRespType.SUCCESS;
                    }
                }
                if (tstatus.Status == RechargeRespType.SUCCESS)
                {
                    tstatus.ErrorCode = ErrorCodes.Transaction_Successful.ToString();
                    tstatus.ErrorMsg = nameof(ErrorCodes.Transaction_Successful);

                }
                #endregion
            }
            catch (Exception ex)
            {
                var errorLog = new ErrorLog
                {
                    ClassName = GetType().Name,
                    FuncName = MethodName,
                    Error = ex.Message,
                    LoginTypeID = LoginType.ApplicationUser,
                    UserId = rechargeAPIHit.aPIDetail.ID
                };
                tstatus.Status = RechargeRespType.PENDING;
                var _ = new ProcPageErrorLog(_dal).Call(errorLog);
            }
            tstatus.ErrorCode = tstatus.ErrorCode ?? string.Empty;
            if (tstatus.Status == RechargeRespType.FAILED && (tstatus.ErrorCode.Equals(ErrorCodes.Request_Accpeted.ToString()) || string.IsNullOrEmpty(tstatus.ErrorCode)))
            {
                tstatus.ErrorCode = ErrorCodes.Unknown_Error.ToString();
                tstatus.ErrorMsg = nameof(ErrorCodes.Unknown_Error);
            }
            if (tstatus.Status == RechargeRespType.FAILED && !tstatus.IsResend && tstatus.ErrorCode.Equals(ErrorCodes.Unknown_Error.ToString()))
            {
                var errorCodes = _errCodeML.Get(ErrorCodes.Unknown_Error.ToString());
                tstatus.IsResend = errorCodes.IsResend;
                tstatus.ErrorMsg = errorCodes.Error;
                tstatus.OperatorID = errorCodes.Error;
            }

            if (tstatus.ErrorCode == ErrorCodes.Unknown_Error.ToString())
            {
                tstatus.ErrorCode = "158";
                tstatus.ErrorMsg = tstatus.APIMsg;
            }
            return tstatus;
        }
        public ResponseStatus CheckIfKeyExistsInDatatable(DataTable dt, string Key)
        {
            var res = new ResponseStatus();
            if (dt != null && !string.IsNullOrEmpty(Key))
            {
                if (Key.Contains("."))
                {
                    var root = Key.Split('.')[0];
                    res.CommonStr = Key.Split('.')[1];
                    if (dt.TableName == root)
                    {
                        res.CommonBool = dt.Columns.Contains(res.CommonStr);
                    }
                }
                else
                {
                    res.CommonStr = Key;
                    res.CommonBool = dt.Columns.Contains(Key);
                }
            }
            return res;
        }
        private async Task UpdateAPIDownStatus(int OID, int APIID)
        {
            var aPIDL = new APIDL(_dal);
            await aPIDL.DownAPI(OID, APIID).ConfigureAwait(false);
        }
        #endregion
    }
}
