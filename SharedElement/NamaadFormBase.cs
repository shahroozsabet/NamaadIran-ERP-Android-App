/*
 * Author: Shahrooz Sabet
 * Date: 20140628
 * */
#region using
using Android.Animation;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
#endregion

namespace NamaadMobile.SharedElement
{
    public abstract class NamaadFormBase : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetActionBar();
        }

        /// <summary>
        /// Shows the form. 
        /// </summary>
        internal void ShowForm(Context ctx)
        {
            Intent i = new Intent(ctx, this.GetType());
            ctx.StartActivity(i);
        }

        private void SetActionBar()
        {
            Window.RequestFeature(WindowFeatures.ActionBar);
            AddActionBarFlag(ActionBarDisplayOptions.ShowCustom);
            ActionBar.SetCustomView(Resource.Layout.actionbar);
            ActionBar.SetDisplayHomeAsUpEnabled(true);
            //AddActionBarFlag(ActionBarDisplayOptions.HomeAsUp);
        }
        private void AddActionBarFlag(ActionBarDisplayOptions flag)
        {
            var bar = ActionBar;
            var change = bar.DisplayOptions ^ flag;
            ActionBar.SetDisplayOptions(change, flag);
        }
        public void SetActionBarTitle(string title)
        {
            ((TextView)FindViewById(Resource.Id.ActionBarTitle)).Text = title;
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                // Respond to the action bar's Up/Home button
                case Android.Resource.Id.Home:
                    Intent upIntent = new Intent(this, typeof(NmdMobileMain));
                    upIntent.PutExtra("UserCode", ((SharedEnviroment)ApplicationContext).UserCode);
                    upIntent.PutExtra("UserName", ((SharedEnviroment)ApplicationContext).UserName);
                    upIntent.PutExtra("IsAdmin", ((SharedEnviroment)ApplicationContext).IsAdmin);
                    upIntent.PutExtra("OrgID", ((SharedEnviroment)ApplicationContext).OrgID);
                    upIntent.PutExtra("SystemCode", ((SharedEnviroment)ApplicationContext).SystemCode);
                    upIntent.PutExtra("ActionCode", 0);
                    upIntent.AddFlags(ActivityFlags.NewTask);
                    upIntent.AddFlags(ActivityFlags.ClearTask);
                    NavUtils.NavigateUpTo(this, upIntent);
                    //StartActivity(upIntent);
                    Finish();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }
        #region AnimatorListenerAdapter implementation
        protected partial class ObjectAnimatorListenerAdapter : AnimatorListenerAdapter
        {
            private View parent;
            private bool show;

            public ObjectAnimatorListenerAdapter(View parent, bool show)
            {
                this.parent = parent;
                this.show = show;
            }

            public override void OnAnimationEnd(Animator animator)
            {
                parent.Visibility = show ? ViewStates.Visible : ViewStates.Gone;
            }
        }
        protected partial class ObjectAnimatorListenerAdapter2 : AnimatorListenerAdapter
        {
            private View parent;
            private bool show;

            public ObjectAnimatorListenerAdapter2(View parent, bool show)
            {
                this.parent = parent;
                this.show = show;
            }

            public override void OnAnimationEnd(Animator animator)
            {
                parent.Visibility = show ? ViewStates.Gone : ViewStates.Visible;
            }
        }
        #endregion

    }
}