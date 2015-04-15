/*
 * Author: Shahrooz Sabet
 * Date: 20140628
 * */
#region using
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

using NamaadMobile.Util;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
#endregion
namespace NamaadMobile.aSQLiteManager
{
    [Activity(Label = "@string/FilterBuilder", Theme = "@style/SearchForm")]
    public class FilterBuilder : NamaadMobile.SharedElement.NamaadFormBase, Android.Views.View.IOnClickListener
    {
        #region Define
        private bool _logging;
        private string _sql = "";
        //private String _displaySql = "";
        //private String _table = "";
        private EditText etValue;
        private EditText etSQL;
        private Spinner spField;
        private Spinner spQualifier;
        private Button btnAdd;
        private Button btnOk;
        private Button btnClear;
        private IList<string> FieldNames { get; set; }
        private IList<string> FieldDisplayNames { get; set; }
        #endregion
        #region Event
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            LoginActionBar();
            _logging = Prefs.getLogging(this);
            //if (TableViewer.dBHelper == null)
            //{
            //	//prevent not null
            //	//aSQLiteManager.database = new Database(_dbPath, _cont);
            //}
            SetContentView(Resource.Layout.filter_wizard);
            Bundle extras = Intent.Extras;
            if (extras != null)
            {
                //_table = extras.GetString("TABLE");
                FieldNames = extras.GetStringArrayList("FieldNames");
                FieldDisplayNames = extras.GetStringArrayList("FieldDisplayNames");
                _sql = toDisplaySQL(extras.GetString("FILTER"));
                if (_sql == null)
                    _sql = "";
                // we are editing an existing filter
            }
            setUpUI();
        }
        public void OnClick(View btn)
        {
            if (btn.Id == Resource.Id.FilterButtonOk)
            {
                // filter finished return it to TableViewer
                Intent inte = new Intent();
                inte.PutExtra("FILTER", toSQL(etSQL.EditableText.ToString()));
                SetResult(Result.Ok, inte);
                Finish();
            }
            else if (btn.Id == btnAdd.Id)
            {
                // add a new filter
                bool isNumber = isNumeric(etValue.EditableText.ToString());
                if (!_sql.Trim().Equals(""))
                    _sql += "\nand ";
                _sql += "(" + spField.SelectedItem.ToString();
                _sql += " " + spQualifier.SelectedItem.ToString();
                if (spQualifier.SelectedItem.ToString().Equals("شبیه"))
                    _sql += " '%" + etValue.EditableText.ToString() + "%')";
                else if (spQualifier.SelectedItem.ToString().Equals("درمجموعه"))
                    _sql += " (" + etValue.EditableText.ToString() + "))";
                else
                {
                    // Quote all non numbers
                    string arg = etValue.EditableText.ToString();
                    if (!isNumber)
                        arg = "'" + arg + "'";
                    _sql += " " + arg + ")";
                }
                etSQL.Text = _sql;
            }
            else if (btn.Id == Resource.Id.FilterButtonClear)
            {
                // clear existing filter
                _sql = "";
                etSQL.Text = "";
            }
        }
        #endregion
        #region Function
        private void LoginActionBar()
        {
            ActionBar.SetDisplayHomeAsUpEnabled(false);
            SetActionBarTitle(GetString(Resource.String.SearchForm));
        }
        /**
     * Bind all the user interface components
     */
        private void setUpUI()
        {
            etValue = (EditText)FindViewById(Resource.Id.FilterETValue);
            etSQL = (EditText)FindViewById(Resource.Id.FilterQuery);
            etSQL.Text = _sql;
            spField = (Spinner)FindViewById(Resource.Id.FilterSPField);
            string[] items = FieldDisplayNames.ToArray();//SearchForm.dBHelper.getFieldsNames(_table);
            ArrayAdapter<string> adField = new ArrayAdapter<string>(this,
                Android.Resource.Layout.SimpleSpinnerItem, items);
            adField.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spField.Adapter = adField;
            spQualifier = (Spinner)FindViewById(Resource.Id.FilterSPQualifier);
            string[] qualifiers = new string[] { "=", "!=", "<", ">", "شبیه", "درمجموعه" };
            ArrayAdapter<string> adQualifiers = new ArrayAdapter<string>(this,
            Android.Resource.Layout.SimpleSpinnerItem, qualifiers);
            adQualifiers.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spQualifier.Adapter = adQualifiers;
            btnAdd = (Button)FindViewById(Resource.Id.FilterBTNAdd);
            btnAdd.SetOnClickListener(this);
            btnOk = (Button)FindViewById(Resource.Id.FilterButtonOk);
            btnOk.SetOnClickListener(this);
            btnClear = (Button)FindViewById(Resource.Id.FilterButtonClear);
            btnClear.SetOnClickListener(this);
        }
        /**
 * Check if a String is a string representation of a number
 * @param str
 * @return
 */
        public static bool isNumeric(string str)
        {
            try
            {
                double d = Double.Parse(str);
            }
            catch (Exception nfe)
            {
                return false;
            }
            return true;
        }
        private string toSQL(string sql)
        {
            for (int i = 0; i < FieldDisplayNames.Count; i++)
            {
                sql = Regex.Replace(sql, FieldDisplayNames[i], FieldNames[i]);
            }
            sql = Regex.Replace(sql, "درمجموعه", "in");
            sql = Regex.Replace(sql, "شبیه", "like");
            return sql;
        }
        private string toDisplaySQL(string sql)
        {
            for (int i = 0; i < FieldNames.Count; i++)
            {
                sql = Regex.Replace(sql, FieldNames[i], FieldDisplayNames[i]);
            }
            sql = Regex.Replace(sql, "in", "درمجموعه");
            sql = Regex.Replace(sql, "like", "شبیه");
            return sql;
        }
        #endregion
    }
}