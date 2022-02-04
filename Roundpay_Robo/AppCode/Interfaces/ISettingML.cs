using Roundpay_Robo.AppCode.Interfaces;
using Roundpay_Robo.AppCode.Model;
using Roundpay_Robo.AppCode.Model;
using Roundpay_Robo.AppCode.Model.ProcModel;
using System.Collections.Generic;

namespace Roundpay_Robo.AppCode.Interfaces
{
    public interface ISettingML
    {
        IEnumerable<RoleMaster> GetRoleForReferral(int _userID);
    }
}
