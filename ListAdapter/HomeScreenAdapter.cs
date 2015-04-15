/*
 * Author: Shahrooz Sabet
 * Date: 20140628
 * */
#region using
using Android.App;
using Android.Views;
using Android.Widget;
using NamaadMobile.SharedElement;
using System.Collections.Generic;
#endregion
namespace NamaadMobile.Adapter
{
    public class HomeScreenAdapter : BaseAdapter<ActionBase>
    {
        List<ActionBase> items;
        Activity context;
        public HomeScreenAdapter(Activity context, List<ActionBase> items)
            : base()
        {
            this.context = context;
            this.items = items;
        }
        public override long GetItemId(int position)
        {
            return position;
        }
        public override ActionBase this[int position]
        {
            get { return items[position]; }
        }
        public override int Count
        {
            get { return items.Count; }
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = items[position];

            View view = convertView;
            if (view == null)
                view = context.LayoutInflater.Inflate(Resource.Layout.home_screen_custom_view, null);
            TextView tv = (TextView)view.FindViewById<TextView>(Resource.Id.Text1);
            tv.Text = item.ActionName;
            if (item.ActionType == 1)
            {
                tv.SetCompoundDrawablesWithIntrinsicBounds(0, 0, Resource.Drawable.ic_menu_programs2, 0);
            }
            else if (item.ActionType == 2)
            {
                tv.SetCompoundDrawablesWithIntrinsicBounds(0, 0, Resource.Drawable.ic_menu_star, 0);
            }
            else
            {
                tv.SetCompoundDrawablesWithIntrinsicBounds(0, 0, Resource.Drawable.ic_test_icon, 0);
            }

            return view;
        }
    }
}