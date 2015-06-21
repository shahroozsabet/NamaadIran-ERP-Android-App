/*
 * Author: Shahrooz Sabet
 * Date: 20140628
 * Updated:20150628
 * */
#region using
using Android.Views;
using Android.Widget;
using NamaadMobile.SharedElement;
#endregion
namespace NamaadMobile.Adapter
{
    internal class HomeScreenAdapter : BaseAdapter<ActionBase>
    {
        readonly NmdMobileMain owner;
        public HomeScreenAdapter(NmdMobileMain owner)
            : base()
        {
            this.owner = owner;
        }
        public override long GetItemId(int position)
        {
            return position;
        }
        public override ActionBase this[int position]
        {
            get { return owner.name2class[position]; }
        }
        public override int Count
        {
            get { return owner.name2class.Count; }
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            ActionBase item = owner.name2class[position];

            if (convertView == null)
                convertView = owner.LayoutInflater.Inflate(Resource.Layout.home_screen_custom_view, null);
            TextView tv = convertView.FindViewById<TextView>(Resource.Id.tvHomeScreen);
            tv.Text = item.ActionName;
            ImageView img = convertView.FindViewById<ImageView>(Resource.Id.imgHomeScreen);
            switch (item.ActionType)
            {
                case 1:
                    tv.SetCompoundDrawablesWithIntrinsicBounds(0, 0, Resource.Drawable.ic_menu_programs2, 0);
                    if (item.SystemCode == 3)
                        img.SetImageResource(Resource.Drawable.ic_bms);
                    else if (item.SystemCode == 2)
                        img.SetImageResource(Resource.Drawable.ic_sal);
                    break;
                case 2:
                    tv.SetCompoundDrawablesWithIntrinsicBounds(0, 0, Resource.Drawable.ic_menu_star, 0);
                    if (item.SystemCode == 2)
                    {
                        img.SetImageResource(item.ActionCode == 1050
                            ? Resource.Drawable.ic_sal1050
                            : Resource.Drawable.ic_sal);
                    }
                    else if (item.SystemCode == 3)
                    {
                        if (item.ActionCode == 1050)
                            img.SetImageResource(Resource.Drawable.ic_bms_bulb);
                        else if (item.ActionCode == 1100)
                            img.SetImageResource(Resource.Drawable.ic_bms_energy);
                        else if (item.ActionCode == 1150)
                            img.SetImageResource(Resource.Drawable.ic_bms_health);
                        else if (item.ActionCode == 1200)
                            img.SetImageResource(Resource.Drawable.ic_bms_sec);
                        else if (item.ActionCode == 1250)
                            img.SetImageResource(Resource.Drawable.ic_bms_env);
                        else
                            img.SetImageResource(Resource.Drawable.ic_bms);
                    }
                    break;
                default:
                    tv.SetCompoundDrawablesWithIntrinsicBounds(0, 0, Resource.Drawable.ic_test_icon, 0);
                    break;
            }

            return convertView;
        }
    }
}