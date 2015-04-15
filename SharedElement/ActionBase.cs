/*
 * Author: Shahrooz Sabet
 * Date: 20140628
 * */
using System;
namespace NamaadMobile.SharedElement
{
    public class ActionBase
    {
        public short OrgID { get; set; }
        public int SystemCode { get; set; }
        public int ActionCode { get; set; }
        public int ActionType { get; set; }
        public string ActionName { get; set; }
        public string ActionSource { get; set; }
        public string ActionArgument { get; set; }
        public int ParentCode { get; set; }
        public string DbNameServer { get; set; }
        public string DbNameClient { get; set; }
    }
}