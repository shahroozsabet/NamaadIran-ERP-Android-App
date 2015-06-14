/*
 * Author: Shahrooz Sabet
 * Date: 20140628
 * */
#region using
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using NamaadMobile.Data;
using NamaadMobile.entity;
using NamaadMobile.Entity;
using NamaadMobile.SharedElement;
using NamaadMobile.Util;
using System;
using System.Collections.Generic;
using System.Linq;
#endregion
namespace NamaadMobile.aSQLiteManager
{
    [Activity(Label = "@string/SearchForm", Theme = "@style/SearchForm")]
    public class QueryViewer : NamaadMobile.SharedElement.NamaadFormBase, Android.Views.View.IOnClickListener
    {
        #region Define
        /// <summary>
        /// The tag For Debugging purpose
        /// </summary>
        private const string TAG = "NamaadMobile.aSQLiteManager.QueryViewer";
        private string _tvQ;
        private Context _cont;
        private NmdMobileDBAdapter _db;
        //Page offset
        private int offset = 0;	// default page offset
        private int limit;			// default page size

        protected string _order = "";
        private bool _increasing = false;
        // UI references.
        private Button bUp;
        private Button bDwn;
        private Button bFirst;
        private Button bLast;
        private Button bFilter;
        private TextView tvDB;
        private TableLayout _aTable;
        private TextView tvFromRec;
        private TextView tvToRec;
        private HorizontalScrollView hv;
        // Where clause for filter
        protected string _where = "";

        private bool _save;
        private bool logging;
        private bool _showTip = false;
        private int _maxWidth;

        private QueryResult res;
        private IList<string> FieldNames { get; set; }
        private IList<string> FieldDisplayNames { get; set; }
        private int _fontSize;
        private string sql;
        private bool _fieldMode = false;
        private string _tableDisplayName;
        private bool _logging;
        //EventHandler
        private OnGlobalLayoutListener ngl;
        #endregion
        #region Event
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            _logging = Prefs.getLogging(this);
            _cont = this;
            logging = Prefs.getLogging(_cont);
            SetContentView(Resource.Layout.query_viewer);
            LoginActionBar();
            _save = Prefs.getSaveSQL(_cont);
            limit = Prefs.getPageSize(_cont);

            tvDB = (TextView)FindViewById(Resource.Id.TableToView);
            bUp = (Button)this.FindViewById(Resource.Id.PgUp);
            bDwn = (Button)this.FindViewById(Resource.Id.PgDwn);
            bFilter = (Button)FindViewById(Resource.Id.btnFilter);
            bFirst = (Button)FindViewById(Resource.Id.btnFirst);
            bLast = (Button)FindViewById(Resource.Id.btnLast);
            tvFromRec = (TextView)FindViewById(Resource.Id.tvFromRec);
            tvToRec = (TextView)FindViewById(Resource.Id.tvToRec);
            hv = (HorizontalScrollView)FindViewById(Resource.Id.horizontalScrollView1);

            bUp.SetOnClickListener(this);
            bDwn.SetOnClickListener(this);
            bFirst.SetOnClickListener(this);
            bLast.SetOnClickListener(this);
            bFilter.SetOnClickListener(this);

            Bundle extras = Intent.Extras;
            _fontSize = Prefs.getFontSize(this);
            _fieldMode = false;

            if (extras != null)
            {
                //_cont = _tvQ.Context;
                FieldNames = extras.GetStringArrayList("FieldNames");
                FieldDisplayNames = extras.GetStringArrayList("FieldDisplayNames");
                _tvQ = extras.GetString("sql");
                _tableDisplayName = extras.GetString("TableDisplayName");
                tvDB.Text = GetString(Resource.String.DBTable) + " " + _tableDisplayName;
                //_db = null;//aSQLiteManager.database;
                //if (_db == null)
                //{
                // database not opened or closed if memory reclaimed
                //_dbPath = extras.GetString("db");
                //aSQLiteManager.database = new Database(_dbPath, _cont);
                //_db = aSQLiteManager.database;
                //}

                //if (!_db.isDatabase) { //TODO 3.4 NullPointerException here sometimes no database open - reopen in aSQLiteManager after onPause???
                //	Utils.logD("Not a database!", logging);
                //	Utils.showException(_dbPath + " is not a database!", _cont);
                //	finish();
                //} else {
                //	Utils.logD("Database open", logging);
                //}
                if (savedInstanceState != null)
                {
                    offset = savedInstanceState.GetInt("PageOffset", 0);
                    _where = savedInstanceState.GetString("WhereClause");
                    if (string.IsNullOrWhiteSpace(_where))
                        _where = "";
                    else
                    {
                        tvDB.Text = GetString(Resource.String.DBTable) + " " + _tableDisplayName + " (" + GetString(Resource.String.Filter) + ")";
                    }
                    _order = savedInstanceState.GetString("Order");
                    _increasing = savedInstanceState.GetBoolean("Increasing");
                    //if (savedInstanceState.GetBoolean("showTip"))
                    //{
                    //	//showTip(getText(R.string.Tip2), 2);
                    //}
                }
                //else ;
                //showTip(getText(R.string.Tip2), 2);
            }
            sql = _tvQ;
            _aTable = (TableLayout)FindViewById(Resource.Id.datagrid);
            if (!sql.Equals(""))
            {
                updateButtons(true);
                fillDataTableWithArgs();
                SetPagingHeader();
            }
        }
        /* (non-Javadoc)
 * @see android.view.View.OnClickListener#onClick(android.view.View)
 */
        public void OnClick(View v)
        {
            int key = v.Id;
            string sql = _tvQ;
            if (!sql.Equals(""))
                if (key == Resource.Id.btnFilter)
                {
                    Intent f = new Intent(_cont, typeof(FilterBuilder));
                    f.PutExtra("FILTER", _where);
                    f.PutStringArrayListExtra("FieldNames", FieldNames);
                    f.PutStringArrayListExtra("FieldDisplayNames", FieldDisplayNames);
                    StartActivityForResult(f, 2);
                }
                else if (key == Resource.Id.PgDwn)
                {
                    if (_aTable != null)
                    {
                        int childs = _aTable.ChildCount;
                        if (childs >= limit)
                        { // No more data on to display - no need to
                            // PgDwn
                            offset += limit;
                            string[] nn = { };
                            setTitles(_aTable, nn);
                            fillDataTableWithArgs();
                            childs = _aTable.ChildCount;
                            if (childs >= limit)
                                SetPageNo();
                            else
                            {
                                tvFromRec.Text = (offset + 1).ToString();
                                tvToRec.Text = (offset + childs).ToString();
                            }
                        }
                    }
                }
                else if (key == Resource.Id.PgUp)
                {
                    if (_aTable != null)
                    {
                        offset -= limit;
                        if (offset <= 0)
                            offset = 0;
                        SetPageNo();
                        fillDataTableWithArgs();
                    }
                }
                else if (key == Resource.Id.btnFirst)
                {
                    offset = 0;
                    fillDataTableWithArgs();
                    SetPageNo();
                }
                else if (key == Resource.Id.btnLast)
                {
                    try
                    {
                        using (_db = new NmdMobileDBAdapter(this))
                        {
                            _db.OpenOrCreateDatabase(((SharedEnviroment)ApplicationContext).DbNameClient);
                            int childs = _db.getNoOfRecordsSQLStatment(sql, _where);
                            offset = childs - limit;
                        }
                        fillDataTableWithArgs();
                        SetPageNo();
                    }
                    catch (Exception ex)
                    {
                        ExceptionHandler.toastMsg(this, ex.Message);
                        ExceptionHandler.logDActivity(ex.ToString(), _logging, TAG);
                    }
                }
        }
        protected override void OnRestart()
        {
            base.OnRestart();
            //if (aSQLiteManager.database == null)
            //	aSQLiteManager.database = new Database(_dbPath, _cont);
            //_db = aSQLiteManager.database;
        }
        protected override void OnSaveInstanceState(Bundle savedInstanceState)
        {
            // Save UI state changes to the savedInstanceState.
            // This bundle will be passed to onCreate if the process is
            // killed and restarted.
            savedInstanceState.PutBoolean("showTip", _showTip);
            savedInstanceState.PutInt("PageOffset", offset);
            savedInstanceState.PutString("WhereClause", _where);
            savedInstanceState.PutString("Order", _order);
            savedInstanceState.PutBoolean("Increasing", _increasing);
            base.OnSaveInstanceState(savedInstanceState);
        }
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
            // Return here if a activity is called with startActivityForResult
            if (data == null)
            {
                return;
            }
            if (requestCode == 2 && resultCode == Result.Ok)
            {
                // returned from FilterBuilder
                Bundle res = data.Extras;
                string filter = res.GetString("FILTER");
                if (filter == null)
                    filter = "";
                _where = filter;
                if (_where.Trim().Equals(""))
                {
                    tvDB.Text = GetString(Resource.String.DBTable) + " " + _tableDisplayName;
                }
                else
                {
                    tvDB.Text = GetString(Resource.String.DBTable) + " " + _tableDisplayName + " (" + GetString(Resource.String.Filter) + ")";
                }
                fillDataTableWithArgs();
                SetPagingHeader();
            }
        }
        protected override void OnResume()
        {
            base.OnResume();
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
            hv.ViewTreeObserver.RemoveGlobalOnLayoutListener(ngl);
            ngl.Dispose();
            ngl = null;
        }
        #endregion
        #region Function
        private void LoginActionBar()
        {
            ActionBar.SetDisplayHomeAsUpEnabled(false);
            SetActionBarTitle(GetString(Resource.String.SearchForm));
        }
        private void fillDataTableWithArgs()
        {
            string order = "";
            if (!_order.Equals(""))
            {
                order = " order by [" + _order;
                if (_increasing)
                    order += "] ASC";
                else
                    order += "] DESC ";
            }
            try
            {
                using (_db = new NmdMobileDBAdapter(this))
                {
                    _db.OpenOrCreateDatabase(((SharedEnviroment)ApplicationContext).DbNameClient);
                    _db.BeginTransaction();
                    res = _db.getSQLQueryPage(sql, offset, limit, _cont, FieldNames.ToArray(), _where, order);
                    _db.EndTransaction();
                }
                setTitles(_aTable, FieldDisplayNames.ToArray());
                appendRows(_aTable, res);
            }
            catch (Exception ex)
            {
                ExceptionHandler.toastMsg(this, ex.Message);
                ExceptionHandler.logDActivity(ex.ToString(), _logging, TAG);
            }
        }
        private void SetPagingHeader()
        {
            try
            {
                using (_db = new NmdMobileDBAdapter(this))
                {
                    _db.OpenOrCreateDatabase(((SharedEnviroment)ApplicationContext).DbNameClient);
                    int childs = _db.getNoOfRecordsSQLStatment(sql, _where);
                    tvDB.Text = tvDB.Text + "، " + GetString(Resource.String.RecCounts) + " " + childs;
                }
                SetPageNo();
            }
            catch (Exception ex)
            {
                ExceptionHandler.toastMsg(this, ex.Message);
                ExceptionHandler.logDActivity(ex.ToString(), _logging, TAG);
            }
        }
        private void SetPageNo()
        {
            tvFromRec.Text = (offset + 1).ToString();
            tvToRec.Text = (offset + limit).ToString();
        }
        /// <summary>
        /// Add a String[] as titles
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="titles">The titles.</param>
        private void setTitles(TableLayout table, string[] titles)
        {
            int rowSize = titles.Length;
            if (table == null)
                return;
            table.RemoveAllViews();
            TableRow row = new TableRow(this);
            row.SetBackgroundColor(Color.Blue);
            for (int i = 0; i < rowSize; i++)
            {
                TextView c = new TextView(this);
                c.TextSize = _fontSize;
                c.Typeface = Typeface.DefaultBold;
                c.SetTextColor(Color.White);
                string title = titles[i];
                if (!string.IsNullOrWhiteSpace(_order) && title.Equals(FieldDisplayNames[FieldNames.IndexOf(_order)]))
                    if (_increasing)
                        title += " ↓";
                    else
                        title += " ↑";
                c.Text = title;
                c.SetPadding(3, 5, 3, 5);
                if (!_fieldMode)
                {
                    c.Click += (sender, e) =>
                    {
                        string newOrder = c.Text.ToString();
                        if (newOrder.EndsWith("↑") || newOrder.EndsWith("↓"))
                            newOrder = newOrder.Substring(0, newOrder.Length - 2);
                        newOrder = FieldNames[FieldDisplayNames.IndexOf(newOrder)];
                        // if same field clicked twice reverse sorting
                        if (newOrder.Equals(_order))
                        {
                            _increasing = !_increasing;
                        }
                        else
                        {
                            _order = newOrder;
                            _increasing = true;
                        }
                        fillDataTableWithArgs();
                        updateData();
                    };
                }
                row.AddView(c);
            }
            TextView cS = new TextView(this);
            cS.TextSize = _fontSize;
            cS.Typeface = Typeface.DefaultBold;
            cS.SetPadding(5, 5, 5, 5);
            row.AddView(cS);
            table.AddView(row, new TableLayout.LayoutParams());
        }
        /// <summary>
        /// Update the data grid
        /// </summary>
        private void updateData()
        {
            try
            {
                fillDataTableWithArgs();
                updateButtons(true);
            }
            catch (Exception e)
            {
                ExceptionHandler.toastMsg(this, e.Message);
                ExceptionHandler.logDActivity(e.ToString(), _logging, TAG);
            }
        }
        /// <summary>
        /// If paging = true show paging buttons otherwise not
        /// </summary>
        /// <param name="paging">if set to <c>true</c> [paging].</param>
        private void updateButtons(bool paging)
        {
            if (paging)
            {
                bUp.Visibility = ViewStates.Visible;
                bDwn.Visibility = ViewStates.Visible;
                bFirst.Visibility = ViewStates.Visible;
                bLast.Visibility = ViewStates.Visible;
            }
            else
            {
                bUp.Visibility = ViewStates.Gone;
                bDwn.Visibility = ViewStates.Gone;
                bUp.Visibility = ViewStates.Gone;
                bDwn.Visibility = ViewStates.Gone;
            }
        }
        /// <summary>
        /// Add a String[][] list to the table layout as rows
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="data">The data.</param>
        private void appendRows(TableLayout table, QueryResult res)
        {
            string[][] data = res.Data.ToArray();
            if (data == null)
                return;
            _maxWidth = Prefs.getMaxWidth(_cont);
            int rowSize = data.Length;
            int colSize = (data.Length > 0) ? data[0].Length : 0;
            for (int i = 0; i < rowSize; i++)
            {
                int rowNo = i;
                TableRow row = new TableRow(this);
                //row.SetOnClickListener(new OnClickListener(delegate
                //	{
                //		// button 1 was clicked!
                //		//Utils.logD("OnClick: " + row.Id, logging);
                //	}));

                if (i % 2 == 1)
                    row.SetBackgroundColor(Color.DarkGray);
                for (int j = 0; j < colSize; j++)
                {
                    TextView c = new TextView(this);
                    c.Text = data[i][j];
                    c.SetPadding(5, 5, 5, 5);
                    if (_maxWidth > 0)
                        c.SetMaxWidth(_maxWidth);
                    c.Click += (sender, e) =>
                    {

                        // button 1 was clicked!
                        //Utils.logD("OnClick: " + c.Id, logging);
                        string text = c.Text;
                        ClipboardManager clipboard = (ClipboardManager)GetSystemService(ClipboardService);
                        clipboard.Text = text;
                        ExceptionHandler.toastMsg(_cont, GetText(Resource.String.CopiedToClipboard));
                        //Toast.MakeText(this, GetText(Resource.String.CopiedToClipboard), ToastLength.Long).Show();
                    };
                    row.AddView(c);
                    // Add a cell with "Select"
                    if (j == colSize - 1)
                    {
                        TextView cS = addSelectField(rowNo, res);
                        row.AddView(cS);
                    }
                }
                if (table != null)
                {
                    table.AddView(row, new TableLayout.LayoutParams()); //2.5 null pointer ex here
                }
                else
                {
                    //Utils.showMessage(_cont.getText(R.string.Error).toString(),
                    //		_cont.getText(R.string.StrangeErr).toString(), _cont);
                }

            }
        }
        /// <summary>
        /// Create a TextView with OnClickListener used to Select/edit the row
        /// </summary>
        /// <param name="rowNo">The rowId for the record taped on</param>
        /// <param name="aTable">if set to <c>true</c> [a table].The table name</param>
        /// <returns>A TextView with that start a record editor</returns>
        private TextView addSelectField(int rowNo, QueryResult res)
        {
            TextView c = new TextView(this);
            if (_maxWidth > 0)
                c.SetMaxWidth(_maxWidth);
            c.TextSize = _fontSize;
            int id;
            // change to long?
            id = rowNo;
            // If it is a table use rowid as id (always in first position) if now a
            // a view use rowNo
            c.Hint = rowNo.ToString();
            c.Id = id;
            c.SetPadding(5, 5, 5, 5);
            // TODO More efficient to make one OnClickListener and assign this to all
            // records edit field?
            c.Text = GetText(Resource.String.Select);
            c.Click += (sender, e) =>
            {
                try
                {
                    long rowid;
                    if (c.Hint != null)
                    {
                        rowid = long.Parse(c.Hint);
                        // Utils.logD("rowId in hint", _logging);
                    }
                    else
                        rowid = (long)rowNo - 1;
                    TableField[] rec = null;
                    //if (aTable)
                    //	rec = dBHelper.getRecord(_table, rowid);
                    rec = getRecord(res, rowid);
                    // TODO Handle View, comment by Shahrooz 20140219
                    // TODO OK to UI must be handled as a special case during update
                    /* Create new res intent */
                    Intent reply = new Intent();
                    //reply.PutExtra("quantity", quantity);
                    Bundle b = new Bundle();
                    b.PutParcelableArray("TableField[]", rec);
                    reply.PutExtras(b);
                    SetResult(Result.Ok, reply);
                    Finish();   // Close this activity
                }
                catch (Exception ex)
                {
                    ExceptionHandler.toastMsg(this, ex.Message);
                    ExceptionHandler.logDActivity(ex.ToString(), _logging, TAG);
                }
            };
            return c;
        }
        private static TableField[] getRecord(QueryResult res, long rowId)
        {
            int fields;
            TableField[] tfs = null;
            fields = res.Data[(int)rowId].Length;
            tfs = new TableField[fields];
            for (int j = 0; j < fields; j++)
            {
                TableField tf = new TableField();
                tf.setName(res.ColumnNames[j]);
                //tf.setDisplayName(res.ColumnDisplayNames[j]);
                tf.setUpdateable(false);
                //TODO Implement BLOB edit
                //is it a BLOB field turn edit off
                try
                {
                    //tf.setValue(cursor.GetString(j));
                    tf.setValue(res.Data[(int)rowId][j]);
                }
                catch (Exception)
                {
                    tf.setUpdateable(false);
                    tf.setValue("BLOB");

                }
                tfs[j] = tf;
            }
            return tfs;
        }
        #endregion
        private partial class OnGlobalLayoutListener : Java.Lang.Object, ViewTreeObserver.IOnGlobalLayoutListener
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
    }
}