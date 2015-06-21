/*
 * Author: Shahrooz Sabet
 * Date: 20150629
 * */
#region using
using System;
using Android.Widget;
#endregion
namespace NamaadMobile.Adapter
{
    internal class BMSViewHolder : Java.Lang.Object
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public EventHandler<CompoundButton.CheckedChangeEventArgs> CheckedChange { get; set; }
        public bool Enabled { get; set; }
        public bool Checked { get; set; }
        public EventHandler Click { get; set; }
        public bool IsSwitch { get; set; }
        public override string ToString()
        {
            return Text;
        }
    }
}