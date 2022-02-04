
using Roundpay_Robo.AppCode.Model;
using Microsoft.AspNetCore.Mvc.Rendering;
using Roundpay_Robo.AppCode.Model.ProcModel;
using System.Collections.Generic;
using Roundpay_Robo.AppCode.Interfaces;
using Roundpay_Robo.AppCode.Interfaces;

namespace Roundpay_Robo.Models
{
    public class Reg
    {
        public string Input { get; set; }
        public bool IsError { get; set; }
        public string Token { get; set; }
    }
    public class UserRegModel : Reg
    {
        public UserInfo userInfo { get; set; }
        public UserRoleSlab roleSlab { get; set; }
        public UserBalnace userBalnace { get; set; }
        public SelectList DMRModelSelect { get; set; }
    }
    public class EmpRegModel : Reg
    {
        public IResponseStatus EmpDetail { get; set; }
        public List<RoleMaster> Roles { get; set; }
    }
    public class UserListModel
    {
        public SelectList  selectListItems{ get; set; }
        public UserBalnace userBalnace { get; set; }
        public UserList userList { get; set; }
        public bool IsAdmin { get; set; }
        public string SelfMobileNo { get; set; }
        public int? RowCount { get; set; }
        public PegeSetting PegeSetting { get; set; }

    }
}
