/*
 * Author: Shahrooz Sabet
 * Date: 20140628
 * */
#region using
using Android.App;
using System;
#endregion
namespace NamaadMobile.SharedElement
{
    [Application]
    public class SharedEnviroment : Application
    {
        public short OrgID { get; set; }
        public int SystemCode { get; set; }
        public int ActionCode { get; set; }
        public string ActionName { get; set; }
        public string ActionSource { get; set; }
        public string UserName { get; set; }
        public int UserCode { get; set; }
        public bool IsAdmin { get; set; }
        public string DbNameServer { get; set; }
        public string DbNameClient { get; set; }
        public bool Logging { get; set; }
        /// <summary>
        /// The tag For Debugging purpose
        /// </summary>
        public string TAG { get; set; }


        public SharedEnviroment(IntPtr handle, global::Android.Runtime.JniHandleOwnership transfer)
            : base(handle, transfer)
        {
        }

    }
}