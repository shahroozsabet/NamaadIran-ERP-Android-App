/*
 * Author: Shahrooz Sabet
 * Date: 20140628
 * Updated:20150901
 * */
#region using
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Mono.Data.Sqlite;
using NamaadMobile.aSQLiteManager;
using NamaadMobile.Adapter;
using NamaadMobile.Data;
using NamaadMobile.Entity;
using NamaadMobile.SharedElement;
using NamaadMobile.Util;
using NamaadMobile.WebService;
using Object = Java.Lang.Object;
#endregion

namespace NamaadMobile
{
    /// <summary>
    /// TODO: Can use Time picker
    /// </summary>
    [Activity(Label = "@string/Sal1050")]
    public class Sal1050 : NamaadFormBase
    {
        #region Define
        public static NmdMobileDBAdapter dbHelper;
        /// <summary>
        /// The tag For Debugging purpose
        /// </summary>
        private const string _table = "WebGoodSalPrice";
        // UI references.
        private Button btnCreate;
        private Button btnSave;
        private Button btnBarCodeScanner;
        private ListView listView;
        private TextView mTVLookUpCustomPriceTotal;
        private TextView tvPosCustNo;
        private TextView tvPosPriceType;
        private TextView tvCurrentDate;
        private TextView tvSerial;
        private HorizontalScrollView hv;
        //private TextView mTVLookUpCustomQuantityTotal;
        //private ListView listViewQuantity;
        private IList<IParcelable> listItem = new List<IParcelable>();

        private Sal1050Adapter adapter;

        private int serialDprt;
        private int seq = 0;
        //EventHandler
        private OnGlobalLayoutListener ngl;
        #endregion
        #region Event
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            ((SharedEnviroment)ApplicationContext).Logging = Pref.getLogging(this);
            ((SharedEnviroment)ApplicationContext).TAG = "NamaadMobile.Sal1050";
            SetActionBarTitle(((SharedEnviroment)ApplicationContext).ActionName);
            SetContentView(Resource.Layout.sal1050);
            btnCreate = (Button)FindViewById(Resource.Id.btnCreate);
            btnSave = (Button)FindViewById(Resource.Id.btnSave);
            btnBarCodeScanner = (Button)FindViewById(Resource.Id.btnBarCodeScanner);
            tvPosCustNo = ((TextView)FindViewById(Resource.Id.tvPosCustNo));
            tvPosPriceType = ((TextView)FindViewById(Resource.Id.tvPosPriceType));
            tvCurrentDate = ((TextView)FindViewById(Resource.Id.tvCurrentDate));
            tvSerial = ((TextView)FindViewById(Resource.Id.tvSerial));
            hv = (HorizontalScrollView)FindViewById(Resource.Id.horizontalScrollView1);
            try
            {
                using (dbHelper = new NmdMobileDBAdapter(this))
                {
                    dbHelper.OpenOrCreateDatabase(((SharedEnviroment)ApplicationContext).ActionArgument);
                    using (SqliteDataReader reader = dbHelper.ExecuteReader("Select CrmDprt, PosCustNo, PosPriceType, CurrentDate From WebSalCommon"))
                        while (reader.Read())
                        {
                            tvPosCustNo.Text = reader["PosCustNo"].ToString();
                            tvPosPriceType.Text = reader["PosPriceType"].ToString();
                            //mTVCurrentDate_FactorForooshGah.Text = reader["CurrentDate"].ToString();
                            serialDprt = int.Parse(reader["CrmDprt"].ToString());
                        }
                    object obj = dbHelper.ExecuteScalar("SELECT Max(FormNo) As MaxFormNo FROM WebSalPerformaDtl");
                    if (obj != DBNull.Value)
                        tvSerial.Text = (int.Parse(obj.ToString()) + 1).ToString();

                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("SQLite error\r\nno such table:"))
                    ExceptionHandler.toastMsg(this, GetString(Resource.String.error_reRefresh_Sal1050));
                else
                    ExceptionHandler.toastMsg(this, e.Message);
                btnCreate.Enabled = false;
                btnSave.Enabled = false;
                ExceptionHandler.logDActivity(e.ToString(), ((SharedEnviroment)ApplicationContext).Logging, ((SharedEnviroment)ApplicationContext).TAG);
            }
            tvCurrentDate.Text = GetFormDate().ToString();
            mTVLookUpCustomPriceTotal = (TextView)FindViewById(Resource.Id.tVLookUpCustomPriceTotal);
            //mTVLookUpCustomQuantityTotal = (TextView)FindViewById(Resource.Id.tVLookUpCustomQuantityTotal);

            listView = (ListView)FindViewById(Resource.Id.lVFactorForooshGah);
            //var listItemCon = LastNonConfigurationInstance as IList<IParcelable>;
            //if (listItemCon != null)
            //{
            //	listItem = listItemCon;
            //}
            adapter = new Sal1050Adapter(this, listItem);// Using cursor is not feasible since we use Mono SQlite library.
            listView.Adapter = adapter;
            listView.TextFilterEnabled = true;
            // Tell the list view to show one checked/activated item at a time.
            listView.ChoiceMode = ChoiceMode.Single;

            RegisterForContextMenu(listView);

            btnSave = (Button)FindViewById(Resource.Id.btnSave);
        }
        protected override void OnResume()
        {
            base.OnResume();
            btnSave.Click += btnSave_Click;
            btnCreate.Click += btnCreate_Click;
            btnBarCodeScanner.Click += btnBarCodeScanner_Click;
            ngl = new OnGlobalLayoutListener(delegate
            {
                hv.FullScroll(FocusSearchDirection.Right);
            }
            );
            hv.ViewTreeObserver.AddOnGlobalLayoutListener(ngl);
        }
        protected override void OnPause()
        {
            base.OnPause();
            btnSave.Click -= btnSave_Click;
            btnCreate.Click -= btnCreate_Click;
            btnBarCodeScanner.Click -= btnBarCodeScanner_Click;
            hv.ViewTreeObserver.RemoveGlobalOnLayoutListener(ngl);
            ngl.Dispose();
            ngl = null;
        }
        public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo info)
        {
            base.OnCreateContextMenu(menu, v, info);
            menu.SetHeaderTitle(((Sal1050EntityWebGoodSalPrice)listItem.ElementAt(((AdapterView.AdapterContextMenuInfo)info).Position)).FarsiDesc);
            menu.SetHeaderIcon(Android.Resource.Drawable.IcMenuDelete);
            //menu.Add(0, 0, 0, GetString(Resource.String.Update));
            menu.Add(0, 1, 0, GetString(Resource.String.Delete));
        }
        public override bool OnContextItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case 0:
                    ContextItemClicked(item.TitleFormatted.ToString());
                    break;
                case 1:
                    DeleteItem(item);
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }
        //private void listViewItemSelected(object sender, AdapterView.ItemClickEventArgs e)
        //{
        //	// Make the newly clicked item the currently selected one.
        //	listView.SetItemChecked(e.Position, true);
        //}
        /// <summary>
        /// Called when the activity receives a results. Catch result
        /// </summary>
        /// <param name="requestCode">The integer request code originally
        /// supplied to startActivityForResult(), allowing you to identify
        /// who this result came from.</param>
        /// <param name="resultCode">The integer result code returned by 
        /// the child activity through its setResult().</param>
        /// <param name="data">An Intent, which can return result data to the caller 
        /// (various data can be attached to Intent "extras").</param>
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (resultCode == Result.Ok)
            {
                IParcelable[] tblFieldArr = null;
                try
                {
                    if (requestCode == 0)
                    {
                        tblFieldArr = data.GetParcelableArrayExtra("TableField[]");
                    }
                    else if (requestCode == 1)
                    {
                        using (dbHelper = new NmdMobileDBAdapter(this))
                        {
                            dbHelper.OpenOrCreateDatabase(((SharedEnviroment)ApplicationContext).ActionArgument);
                            string barcode = data.GetStringExtra("barcode");
                            long rowID = (long)dbHelper.ExecuteScalar("Select rowid From " + _table + " Where ItemCode Like '%" + barcode + "%'");
                            tblFieldArr = dbHelper.getRecord(_table, rowID);
                        }
                    }
                    QuantityDialogBuilder(WebGoodSalPriceBuilder(tblFieldArr));
                }
                catch (Exception e)
                {
                    ExceptionHandler.toastMsg(this, e.Message);
                    ExceptionHandler.logDActivity(e.ToString(), ((SharedEnviroment)ApplicationContext).Logging, ((SharedEnviroment)ApplicationContext).TAG);
                }
            }
        }
        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutParcelableArrayList("listItem", listItem);
            outState.PutString("mTVLookUpCustomPriceTotal", mTVLookUpCustomPriceTotal.Text);
            outState.PutString("formNo", tvSerial.Text);
            outState.PutInt("seq", seq);
            base.OnSaveInstanceState(outState);
        }
        protected override void OnRestoreInstanceState(Bundle savedState)
        {
            base.OnRestoreInstanceState(savedState);
            listItem = savedState.GetParcelableArrayList("listItem").Cast<IParcelable>().ToList(); //as IList<IParcelable>; 
            adapter = new Sal1050Adapter(this, listItem);// Using cursor is not feasible since we use Mono SQlite library.
            listView.Adapter = adapter;
            mTVLookUpCustomPriceTotal.Text = savedState.GetString("mTVLookUpCustomPriceTotal");
            tvSerial.Text = savedState.GetString("formNo");
            seq = savedState.GetInt("seq");
        }
        //public override Java.Lang.Object OnRetainNonConfigurationInstance()
        //{
        //	base.OnRetainNonConfigurationInstance();
        //	return (Java.Lang.Object)listItem;//InValidCast Exception
        //}
        #endregion
        #region Function
        private void btnBarCodeScanner_Click(object sender, EventArgs e)
        {
            // start the scanner
            StartActivityForResult(typeof(ScanActivity), 1);
        }
        private void btnCreate_Click(object sender, EventArgs e)
        {
            /* Start new Activity that returns a result */

            Intent intent = new Intent(this, typeof(TableViewer));

            intent.PutExtra("Table", _table);
            intent.PutExtra("type", Types.TABLE);

            string tableDisplayName = GetString(Resource.String.WebGoodSalPrice);
            intent.PutExtra("TableDisplayName", tableDisplayName);
            IList<string> fieldNames = new List<string>();
            fieldNames.Add("Cost1");
            fieldNames.Add("PriceID");
            fieldNames.Add("PriceType");
            fieldNames.Add("Unit");
            fieldNames.Add("Price");
            fieldNames.Add("FarsiDesc");
            fieldNames.Add("ItemCode");
            IList<string> fieldDisplayNames = new List<string>();
            fieldDisplayNames.Add(GetString(Resource.String.tblLblCost1));
            fieldDisplayNames.Add(GetString(Resource.String.tblLblPriceID));
            fieldDisplayNames.Add(GetString(Resource.String.tblLblPriceType));
            fieldDisplayNames.Add(GetString(Resource.String.tblLblUnit));
            fieldDisplayNames.Add(GetString(Resource.String.tblLblPrice));
            fieldDisplayNames.Add(GetString(Resource.String.tblLblFarsiDesc));
            fieldDisplayNames.Add(GetString(Resource.String.tblLblItemCode));
            intent.PutStringArrayListExtra("FieldNames", fieldNames);
            intent.PutStringArrayListExtra("FieldDisplayNames", fieldDisplayNames);
            StartActivityForResult(intent, 0);
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(WSManager));

            string sql = "Select FarsiDesc,ItemCode From WebGoodSalPrice";
            intent.PutExtra("sql", sql);

            string tableDisplayName = GetString(Resource.String.WebGoodSalPrice);
            intent.PutExtra("TableDisplayName", tableDisplayName);
            IList<string> fieldNames = new List<string>();
            fieldNames.Add("FarsiDesc");
            fieldNames.Add("ItemCode");
            IList<string> fieldDisplayNames = new List<string>();
            fieldDisplayNames.Add(GetString(Resource.String.tblLblFarsiDesc));
            fieldDisplayNames.Add(GetString(Resource.String.tblLblItemCode));
            intent.PutStringArrayListExtra("FieldNames", fieldNames);
            intent.PutStringArrayListExtra("FieldDisplayNames", fieldDisplayNames);
            StartActivityForResult(intent, 0);
        }
        void ContextItemClicked(string item)
        {
            Console.WriteLine(item + " Option Menuitem Clicked");
            var t = Toast.MakeText(this, "Options Menu '" + item + "' Clicked", ToastLength.Short);
            t.Show();
        }
        private void ToDB(Sal1050EntityWebGoodSalPrice res)
        {
            try
            {
                using (dbHelper = new NmdMobileDBAdapter(this))
                {
                    dbHelper.OpenOrCreateDatabase(((SharedEnviroment)ApplicationContext).ActionArgument);
                    using (DataTable dt = dbHelper.ExecuteSQL(WebGoodSalPriceDTSB(res).ToString()))
                    {
                        dbHelper.CopyToDB(dt, "WebSalPerformaDtl", null, null);
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionHandler.toastMsg(this, e.Message);
                ExceptionHandler.logDActivity(e.ToString(), ((SharedEnviroment)ApplicationContext).Logging, ((SharedEnviroment)ApplicationContext).TAG);
            }
        }
        private static int GetFormDate()
        {
            PersianCalendar pc = new PersianCalendar();
            DateTime thisDate = DateTime.Now;
            int formDate = int.Parse(pc.GetYear(thisDate).ToString() + pc.GetMonth(thisDate).ToString() + pc.GetDayOfMonth(thisDate).ToString());
            return formDate;
        }
        private void DeleteItem(IMenuItem item)
        {
            AdapterView.AdapterContextMenuInfo info = (AdapterView.AdapterContextMenuInfo)item.MenuInfo;
            try
            {
                Sal1050EntityWebGoodSalPrice element = (Sal1050EntityWebGoodSalPrice)listItem.ElementAt(info.Position);
                mTVLookUpCustomPriceTotal.Text = (double.Parse(mTVLookUpCustomPriceTotal.Text) - element.Price * element.Quantity).ToString();
                listItem.RemoveAt(info.Position);
                using (dbHelper = new NmdMobileDBAdapter(this))
                {
                    dbHelper.OpenOrCreateDatabase(((SharedEnviroment)ApplicationContext).ActionArgument);
                    dbHelper.ExecuteNonQuery("Delete From WebSalPerformaDtl Where FormNo=" + tvSerial.Text + " And Seq =" + (info.Position + 1) + " And SerialDprt=" + serialDprt + " And FormDate=" + tvCurrentDate.Text);
                }
                //adapter.RemoveItem(element);
                //dBHelper.ExecuteNonQuery("Update WebSalPerformaDtl Set Seq=" + --seq + " Where FormNo=" + mTV_Serial_FactorForooshGah.Text + " And Seq =" + (info.Position + 2) + " And SerialDprt=" + serialDprt + " And FormDate=" + mTVCurrentDate_FactorForooshGah.Text);
            }
            catch (Exception e)
            {
                ExceptionHandler.toastMsg(this, e.Message);
                ExceptionHandler.logDActivity(e.ToString(), ((SharedEnviroment)ApplicationContext).Logging, ((SharedEnviroment)ApplicationContext).TAG);
            }
            adapter.NotifyDataSetChanged();
        }
        private void QuantityDialogBuilder(Sal1050EntityWebGoodSalPrice sal1050EntityWebGoodSalePrice)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            View line_editor = LayoutInflater.Inflate(Resource.Layout.sal1050_line_editor, null);
            builder.SetView(line_editor);
            AlertDialog ad = builder.Create();
            ad.SetTitle(sal1050EntityWebGoodSalePrice.FarsiDesc);
            ad.SetIcon(Android.Resource.Drawable.IcDialogAlert);
            //ad.SetMessage("Alert message...");
            EditText etQuantity = (EditText)line_editor.FindViewById(Resource.Id.eTQuantity);
            TextView mTVUnitQuantityForm = ((TextView)line_editor.FindViewById(Resource.Id.tVUnitQuantityForm));
            mTVUnitQuantityForm.Text = sal1050EntityWebGoodSalePrice.Unit;
            double quantity = 1;
            ad.SetButton(GetString(Resource.String.OK), (s, e) =>
            {
                string str = etQuantity.Text.Trim();
                if (!string.IsNullOrWhiteSpace(str) && double.Parse(str) > 0)
                    quantity = double.Parse(str);
                sal1050EntityWebGoodSalePrice.Quantity = quantity;
                AddItem(sal1050EntityWebGoodSalePrice);
            });
            ad.Show();
        }
        private Sal1050EntityWebGoodSalPrice WebGoodSalPriceBuilder(IParcelable[] tblFieldArr)
        {
            Sal1050EntityWebGoodSalPrice sal1050EntityWebGoodSalePrice = new Sal1050EntityWebGoodSalPrice();
            foreach (TableField field in tblFieldArr)
            {
                switch (field.getName())
                {
                    case "Cost1":
                        sal1050EntityWebGoodSalePrice.Cost1 = double.Parse(field.getValue());
                        break;
                    case "FarsiDesc":
                        sal1050EntityWebGoodSalePrice.FarsiDesc = field.getValue();
                        break;
                    case "ItemCode":
                        sal1050EntityWebGoodSalePrice.ItemCode = field.getValue();
                        break;
                    case "Price":
                        sal1050EntityWebGoodSalePrice.Price = double.Parse(field.getValue());
                        break;
                    case "PriceID":
                        sal1050EntityWebGoodSalePrice.PriceID = int.Parse(field.getValue());
                        break;
                    case "PriceType":
                        sal1050EntityWebGoodSalePrice.PriceType = int.Parse(field.getValue());
                        break;
                    case "Unit":
                        sal1050EntityWebGoodSalePrice.Unit = field.getValue();
                        break;
                }
            }
            return sal1050EntityWebGoodSalePrice;
        }
        private void AddItem(Sal1050EntityWebGoodSalPrice sal1050EntityWebGoodSalePrice)
        {
            listItem.Add(sal1050EntityWebGoodSalePrice);
            //adapter.AddItem(sal1050EntityWebGoodSalePrice);
            adapter.NotifyDataSetChanged();
            //mTVLookUpCustomQuantityTotal.Text = (int.Parse(mTVLookUpCustomQuantityTotal.Text) + 1).ToString();
            mTVLookUpCustomPriceTotal.Text = (double.Parse(mTVLookUpCustomPriceTotal.Text) + sal1050EntityWebGoodSalePrice.Price * sal1050EntityWebGoodSalePrice.Quantity).ToString();
            ToDB(sal1050EntityWebGoodSalePrice);
        }
        #endregion
        #region PrivateHelper
        private StringBuilder WebGoodSalPriceDTSB(Sal1050EntityWebGoodSalPrice res)
        {
            int formDate = int.Parse(tvCurrentDate.Text);
            StringBuilder sb = new StringBuilder();
            sb.Append("Select Cast(");
            sb.Append(serialDprt); sb.Append(" As int) As SerialDprt, Cast(");
            sb.Append(int.Parse(tvSerial.Text)); sb.Append(" As int) As FormNo, Cast(");
            sb.Append(formDate); sb.Append(" As int) As FormDate, Cast(");
            sb.Append(++seq); sb.Append(" As int) As Seq, Cast(");
            sb.Append("0 As int) As PreviousSeq,'");
            sb.Append(res.ItemCode); sb.Append("' As ItemCode, Cast(");
            sb.Append("0 As int) As BatchNo, Cast(");
            sb.Append("0 As int) As ServiceCode, Cast(");
            sb.Append(tvPosCustNo.Text); sb.Append(" As int) As PersonNo, Cast(");
            sb.Append(res.PriceID); sb.Append(" As int) As PriceId, Cast(");
            sb.Append(res.PriceType); sb.Append(" As smallint) As PriceType, Cast(");
            sb.Append(res.Price); sb.Append(" As float) As Price, Cast(");
            sb.Append(res.Quantity); sb.Append(" As float) As Qty1, Cast(");
            sb.Append("1 As float) As Qty2, Cast(");
            sb.Append("0 As float) As UsedQty1, Cast(");
            sb.Append("0 As float) As UsedQty2, Cast(");
            sb.Append("0 As float) As IncrsDecrs1, Cast(");
            sb.Append("0 As float) As IncrsDecrs2, Cast(");
            sb.Append("0 As float) As IncrsDecrs3, Cast(");
            sb.Append("0 As float) As IncrsDecrs4, Cast(");
            sb.Append("0 As float) As UsedIncrsDecrs1, Cast(");
            sb.Append("0 As float) As UsedIncrsDecrs2, Cast(");
            sb.Append("0 As float) As UsedIncrsDecrs3, Cast(");
            sb.Append("0 As float) As UsedIncrsDecrs4, Cast(");
            sb.Append("0 As float) As HdrDiscount, Cast(");
            sb.Append("0 As float) As Discount, Cast(");
            sb.Append(res.Cost1); sb.Append(" As float) As Cost1, Cast(");
            sb.Append("0 As float) As Cost2, Cast(");
            sb.Append("0 As float) As Cost3, Cast(");
            sb.Append("0 As float) As Cost4, Cast(");
            sb.Append("0 As float) As Cost5, Cast(");
            sb.Append("0 As float) As Cost6, Cast(");
            sb.Append("0 As float) As Cost7, Cast(");
            sb.Append("0 As float) As Cost8, Cast(");
            sb.Append("0 As float) As Amount, Cast(");
            sb.Append("0 As float) As NetAmount, Cast(");
            sb.Append("0 As float) As CrdtAmount, Cast(");
            sb.Append("'' As varchar(250)) As Description, Cast(");
            sb.Append(formDate); sb.Append(" As int) As CreateDate, Cast(");
            sb.Append(((SharedEnviroment)ApplicationContext).UserCode); sb.Append(" As int) As CreateUser, Cast(");
            sb.Append(formDate); sb.Append(" As int) As ChangeDate, Cast(");
            sb.Append(((SharedEnviroment)ApplicationContext).UserCode); sb.Append(" As int) As ChangeUser, Cast(");
            sb.Append("1 As int) As TableDataVersion");

            return sb;
        }
        private partial class OnGlobalLayoutListener : Object, ViewTreeObserver.IOnGlobalLayoutListener
        {
            private Action on_global_layout;
            public OnGlobalLayoutListener(Action onGlobalLayout)
            {
                on_global_layout = onGlobalLayout;
            }
            public void OnGlobalLayout()
            {
                on_global_layout();
            }
        }
        #endregion
    }
}