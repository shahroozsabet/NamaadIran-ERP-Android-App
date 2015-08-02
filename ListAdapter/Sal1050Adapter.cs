/*
 * Author: Shahrooz Sabet
 * Date: 20140628
 * Updated:20150901
 * */
#region using
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using NamaadMobile.Entity;
using System.Collections.Generic;
#endregion
namespace NamaadMobile.Adapter
{

    /// <summary>
    /// To work efficiently the adapter implemented here uses two techniques:
    /// - It reuses the convertView passed to getView() to avoid inflating View when it is not necessary
    /// - It uses the ViewHolder pattern to avoid calling findViewById() when it is not necessary
    /// The ViewHolder pattern consists in storing a data structure in the tag of the view returned by
    /// getView(). This data structures contains references to the views we want to bind data to, thus
    /// avoiding calls to findViewById() every time getView() is invoked.
    /// TODO: Page Up/Down
    /// </summary>
    internal class Sal1050Adapter : BaseAdapter<IParcelable>
    {
        public IList<IParcelable> Orig_items { get; set; }
        public IList<IParcelable> Items { get; set; }
        private readonly LayoutInflater mInflater;

        public Sal1050Adapter(Activity context, IList<IParcelable> items)
            : base()
        {
            Orig_items = items;
            Items = items;
            // Cache the LayoutInflate to avoid asking for a new one each time.
            mInflater = LayoutInflater.From(context);
        }
        public override long GetItemId(int position)
        {
            return position;
        }
        public override IParcelable this[int position] => Items[position];

        /// <summary>
        /// Custom Basic Filtering Function, Filters the webgoodsalprice.
        /// </summary>
        /// <param name="constraint">The constraint.</param>
        public void FilterWebGoodSalPrice(string constraint)
        {
            // Create new empty list to add matched elements to
            List<IParcelable> filtered = new List<IParcelable>();

            // examine each element to build filtered list
            // remember to always use your original items list
            foreach (Sal1050EntityWebGoodSalPrice item in this.Orig_items)
            {
                string name = item.ToString();
                if (name.Contains(constraint))
                {
                    filtered.Add(item);
                }
            }
            //set new (filterd) current list of items
            this.Items = filtered;

            //notify ListView to Rebuild
            NotifyDataSetChanged();
        }
        public override Java.Lang.Object GetItem(int position)
        {
            return (Java.Lang.Object)Items[position];
        }
        public override int Count => Items?.Count ?? 0;

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            // A ViewHolder keeps references to children views to avoid unnecessary calls
            // to findViewById() on each row.
            ViewHolder holder;
            Sal1050EntityWebGoodSalPrice item = (Sal1050EntityWebGoodSalPrice)Items[position];

            if (convertView == null)
            {
                convertView = mInflater.Inflate(Resource.Layout.lookup_good_sal_price_custom_view, null);

                // Creates a ViewHolder and store references to children views
                // we want to bind data to.
                holder = new ViewHolder
                {
                    TVLookUpCustomCost1 = convertView.FindViewById<TextView>(Resource.Id.tVLookUpCustomCost1),
                    TVLookUpCustomFarsiDesc = convertView.FindViewById<TextView>(Resource.Id.tVLookUpCustomFarsiDesc),
                    TVLookUpCustomItemCode = convertView.FindViewById<TextView>(Resource.Id.tVLookUpCustomItemCode),
                    TVLookUpCustomPrice = convertView.FindViewById<TextView>(Resource.Id.tVLookUpCustomPrice),
                    TVLookUpCustomUnit = convertView.FindViewById<TextView>(Resource.Id.tVLookUpCustomUnit),
                    TVLookUpCustomRowNo = convertView.FindViewById<TextView>(Resource.Id.tVLookUpCustomRowNo),
                    TVLookUpCustomQuantity = convertView.FindViewById<TextView>(Resource.Id.tVLookUpCustomQuantity)
                };

                convertView.Tag = holder;
            }
            else
            {
                // Get the ViewHolder back to get fast access to the TextView
                // and the ImageView.
                holder = (ViewHolder)convertView.Tag;
            }

            // Bind the data efficiently with the holder.
            holder.TVLookUpCustomItemCode.Text = item.ItemCode;
            holder.TVLookUpCustomFarsiDesc.Text = item.FarsiDesc;
            holder.TVLookUpCustomPrice.Text = item.Price.ToString();
            holder.TVLookUpCustomUnit.Text = item.Unit;
            holder.TVLookUpCustomCost1.Text = item.Cost1.ToString();
            holder.TVLookUpCustomRowNo.Text = (position + 1).ToString();
            holder.TVLookUpCustomQuantity.Text = item.Quantity.ToString();

            return convertView;
        }
        /// <summary>
        /// TODO: Recycle Rows via ViewHolder, manual from Android programing toturial pdf, Common ware, page 42
        /// </summary>
        private class ViewHolder : Java.Lang.Object
        {
            public TextView TVLookUpCustomCost1 { get; set; }
            public TextView TVLookUpCustomFarsiDesc { get; set; }
            public TextView TVLookUpCustomItemCode { get; set; }
            public TextView TVLookUpCustomPrice { get; set; }
            public TextView TVLookUpCustomUnit { get; set; }
            public TextView TVLookUpCustomRowNo { get; set; }
            public TextView TVLookUpCustomQuantity { get; set; }
        }
        public void ClearItems()
        {
            Orig_items.Clear();
            Items.Clear();
            NotifyDataSetChanged();
        }

        /// <summary>
        /// Adds the item. 
        /// Note:is not tested yet.
        /// </summary>
        /// <param name="res">The resource.</param>
        public void AddItem(Sal1050EntityWebGoodSalPrice res)
        {
            Orig_items.Add(res);
            Items.Add(res);
            NotifyDataSetChanged();
        }
        /// <summary>
        /// Removes the item.
        /// Note:is not tested yet.
        /// </summary>
        /// <param name="res">The resource.</param>
        public void RemoveItem(Sal1050EntityWebGoodSalPrice res)
        {
            Orig_items.Remove(res);
            Items.Remove(res);
            NotifyDataSetChanged();
        }
    }
}