/*
 * Author: Shahrooz Sabet
 * Date:20150701
 * */
#region using
using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Widget;
#endregion
namespace NamaadMobile.Adapter
{
    internal class BMSViewHolderAdapter : BaseAdapter<BMSViewHolder>
    {
        readonly Activity owner;
        List<BMSViewHolder> items;
        public BMSViewHolderAdapter(Activity owner, List<BMSViewHolder> items)
            : base()
        {
            this.owner = owner;
            this.items = items;
        }
        public override long GetItemId(int position)
        {
            return position;
        }
        public override BMSViewHolder this[int position]
        {
            get { return items[position]; }
        }
        public override int Count
        {
            get { return items.Count; }
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            BMSViewHolder item = items[position];
            View view = owner.LayoutInflater.Inflate(item.IsSwitch ? Resource.Layout.list_item_switch : Resource.Layout.bms_editable_device, null);
            if (item.IsSwitch)
            {
                Switch bmsDevice = view.FindViewById<Switch>(Resource.Id.swDyn);
                bmsDevice.Tag = item.Id;
                bmsDevice.Text = item.ToString();
                bmsDevice.Enabled = item.Enabled;
                bmsDevice.Checked = item.Checked;
                if (bmsDevice.Enabled) bmsDevice.CheckedChange += item.CheckedChange;
            }
            else
            {
                EditText etDyn = view.FindViewById<EditText>(Resource.Id.etLabledNumberButton);
                TextView tvDyn = view.FindViewById<TextView>(Resource.Id.tvLabledNumberButton);
                tvDyn.Text = item.ToString();
                Button btnEditableDevice = view.FindViewById<Button>(Resource.Id.btnLabledNumberButton);
                btnEditableDevice.Tag = item.Id;
                btnEditableDevice.Click += item.Click;
            }
            return view;
        }
    }
}