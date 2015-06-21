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
    public class TableViewer : NamaadFormBase, View.IOnClickListener
    {
        #region Define
        /// <summary>
        /// The tag For Debugging purpose
        /// </summary>
        private const string TAG = "NamaadMobile.aSQLiteManager.TableViewer";
        private string _table;
        private Context _cont;
        //Page offset
        private int offset = 0;				// default page offset
        private int limit = 15; 			// default page size
        //private bool _updateTable;
        // UI references.
        private Button bUp;
        private Button bDwn;
        private Button bFirst;
        private Button bLast;
        private Button bFilter;
        private TextView tvDB;
        private TableLayout _aHeadings;
        private TableLayout _aTable;
        private TableRow _trHeadings;
        private TableRow _trLastRow;
        private string _tableDisplayName;
        private TextView tvFromRec;
        private TextView tvToRec;
        private HorizontalScrollView hv;

        private int sourceType;
        // Where clause for filter
        protected string _where = "";

        private bool _logging;
        private int _fontSize;
        protected string _order = "";
        private bool _increasing = false;
        private bool _canInsertInView = false;
        private bool _canUpdateView = false;
        private bool _canDeleteView = false;
        private bool _isOnlySelectable = true;
        private IList<string> FieldNames { get; set; }
        private IList<string> FieldDisplayNames { get; set; }

        private Record[] _data;
        private bool _fieldMode = false;
        private bool _showTip = false;
        private int _maxWidth;
        private bool resize = false;
        private bool measured = false;

        private static NmdMobileDBAdapter dbHelper;
        //EventHandler
        private OnGlobalLayoutListener ngl;
        #endregion
        /*
 * What is needed to allow editing form  table viewer 
 * 
 * When displaying records
 * select rowid, t.* form table as t
 * 
 * to include the sqlite rowid
 * 
 * But only if a single field primary key does not exists
 * If there does only
 * select * from table
 * 
 * Then it is possible to update ... where rowid = x
 * 
 */
        #region Event
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            _logging = Prefs.getLogging(this);
            _fontSize = Prefs.getFontSize(this);
            SetContentView(Resource.Layout.table_viewer);
            LoginActionBar();
            tvDB = (TextView)FindViewById(Resource.Id.TableToView);
            _aHeadings = (TableLayout)FindViewById(Resource.Id.headinggrid);
            _aTable = (TableLayout)FindViewById(Resource.Id.datagrid);
            bUp = (Button)FindViewById(Resource.Id.PgUp);
            bDwn = (Button)FindViewById(Resource.Id.PgDwn);
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

            bUp.Visibility = ViewStates.Gone;
            bDwn.Visibility = ViewStates.Gone;
            bFirst.Visibility = ViewStates.Gone;
            bLast.Visibility = ViewStates.Gone;
            _maxWidth = Prefs.getMaxWidth(this);
            limit = Prefs.getPageSize(this);
            //offset = 0;
            _fieldMode = false;

            _cont = this;

            Bundle extras = Intent.Extras;
            if (extras != null)
            {
                _cont = tvDB.Context;
                sourceType = extras.GetInt("type");
                _table = extras.GetString("Table");
                FieldNames = extras.GetStringArrayList("FieldNames");
                FieldDisplayNames = extras.GetStringArrayList("FieldDisplayNames");
                _tableDisplayName = extras.GetString("TableDisplayName");
                if (sourceType == Types.TABLE)
                {
                    tvDB.Text = GetString(Resource.String.DBTable) + " " + _tableDisplayName;
                }
                else if (sourceType == Types.VIEW)
                {
                    tvDB.Text = GetString(Resource.String.DBView) + " " + _tableDisplayName;
                }
                if (bundle != null)
                {
                    offset = bundle.GetInt("PageOffset", 0);
                    _where = bundle.GetString("WhereClause");
                    if (_where == null)
                        _where = "";

                    _order = bundle.GetString("Order");
                    _increasing = bundle.GetBoolean("Increasing");
                }
            }
            updateButtons(true);
            fillDataTableWithArgs();
            SetPagingHeader();
        }
        public void OnClick(View v)
        {
            int key = v.Id;
            if (key == Resource.Id.btnFilter)
            {
                Intent f = new Intent(_cont, typeof(FilterBuilder));
                f.PutExtra("FILTER", _where);
                //f.PutExtra("TABLE", _table);
                f.PutStringArrayListExtra("FieldNames", FieldNames);
                f.PutStringArrayListExtra("FieldDisplayNames", FieldDisplayNames);
                StartActivityForResult(f, 2);
            }
            else if (key == Resource.Id.PgDwn)
            {
                int childs = _aTable.ChildCount;
                if (childs >= limit)
                {  //  No more data on to display - no need to PgDwn
                    offset += limit;
                    fillDataTableWithArgs();
                    childs = _aTable.ChildCount;
                    if (childs >= limit)
                        SetPageNo();
                    else
                    {
                        tvFromRec.Text = (offset + 1).ToString();
                        tvToRec.Text = (offset + childs).ToString();
                    }
                    OnWindowFocusChanged(true);
                }
            }
            else if (key == Resource.Id.PgUp)
            {
                offset -= limit;
                if (offset <= 0)
                    offset = 0;
                SetPageNo();
                fillDataTableWithArgs();
                OnWindowFocusChanged(true);
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
                    using (dbHelper = new NmdMobileDBAdapter(this))
                    {
                        dbHelper.OpenOrCreateDatabase(((SharedEnviroment)ApplicationContext).ActionArgument);
                        int childs = dbHelper.getNoOfRecords(_table, _where);
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
        public override void OnWindowFocusChanged(bool hasFocus)
        {
            if (hasFocus)
            {
                base.OnWindowFocusChanged(hasFocus);
                if (resize)
                    setWidths();

                //if (_updateTable)
                //{
                //	OnClick((Button)this.FindViewById(Resource.Id.Data));
                //	_updateTable = false;
                //}
            }
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
        protected override void OnDestroy()
        {
            base.OnDestroy();
            _table = null;
        }
        #endregion
        #region Function
        private void LoginActionBar()
        {
            ActionBar.SetDisplayHomeAsUpEnabled(false);
            SetActionBarTitle(GetString(Resource.String.SearchForm));
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
        /// Retrieve data for the current table 
        /// </summary>
        private void fillDataTableWithArgs()
        {
            bool isUnUpdateableView = false;
            if (sourceType == Types.VIEW)
            {
                isUnUpdateableView = true;
            }
            else if (sourceType == Types.TABLE)
            {
                isUnUpdateableView = false;
            }
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
                using (dbHelper = new NmdMobileDBAdapter(this))
                {
                    dbHelper.OpenOrCreateDatabase(((SharedEnviroment)ApplicationContext).ActionArgument);
                    dbHelper.BeginTransaction();
                    _data = dbHelper.getTableDataWithWhere(_cont, _table, _where, order, offset, limit, isUnUpdateableView, FieldNames.ToArray());
                    dbHelper.EndTransaction();
                }
                //setTitles(dBHelper.getFieldsNames(_table), !isUnSelectableView);
                setTitles(FieldDisplayNames.ToArray(), !isUnUpdateableView);
                newAppendRows(!isUnUpdateableView);
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
                using (dbHelper = new NmdMobileDBAdapter(this))
                {
                    dbHelper.OpenOrCreateDatabase(((SharedEnviroment)ApplicationContext).ActionArgument);
                    int childs = dbHelper.getNoOfRecords(_table, _where);
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
        /// Add titles to the columns
        /// </summary>
        /// <param name="titles">The titles.</param>
        /// <param name="allowEdit">if set to <c>true</c> [allow edit].</param>
        private void setTitles(string[] titles, bool allowEdit)
        {
            int rowSize = titles.Length;
            _aHeadings.RemoveAllViews();
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
                        //fillDataTableWithArgs();
                        updateData();
                    };
                }
                row.AddView(c);

            }
            if (allowEdit || (_canInsertInView || _canUpdateView || _canDeleteView))
            {
                TextView c = new TextView(this);
                c.TextSize = _fontSize;
                c.Typeface = Typeface.DefaultBold;
                c.SetPadding(5, 5, 5, 5);
                if (!_isOnlySelectable && (allowEdit || _canInsertInView))
                {
                    c.Text = GetText(Resource.String.New);
                }
                row.AddView(c);
            }
            measured = false;
            row.ViewTreeObserver.GlobalLayout += (sender, e) =>
            {
                if (!measured)
                {
                    setWidths();
                    measured = true;
                }
            };
            _trHeadings = row;
            _aHeadings.AddView(_trHeadings, new TableLayout.LayoutParams());
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
        /// Equalise the size of heading and data rows if any rows 
        /// </summary>
        private void setWidths()
        {
            //resizing = true;
            if (_trLastRow != null)
            {
                for (int i = 0; i < _trHeadings.ChildCount; i++)
                {
                    TextView vT = (TextView)_trHeadings.GetChildAt(i);
                    TextView vD = (TextView)_trLastRow.GetChildAt(i);
                    if (vT != null && vD != null)
                    {
                        //Utils.logD("Width " + i + " " + vT.getWidth() + " " + vD.getWidth(), _logging);
                        if (vT.Width > vD.Width)
                            vD.SetWidth(vT.Width);
                        else
                            vT.SetWidth(vD.Width);
                    }
                }
            }
            //resizing = false;
            resize = false;
        }
        /// <summary>
        /// Return a colour based on the field type and a boolean to say if should be light
        /// </summary>
        /// <param name="fieldType">A string with the field type</param>
        /// <param name="light">if set to <c>true</c> the colour will be [light].</param>
        /// <returns>The colour</returns>
        private Color getBackgroundColor(NamaadMobile.Entity.AField.FieldType fieldType, bool light)
        {
            Color color = new Color();
            if (fieldType == AField.FieldType.NULL)
            {
                if (light)
                    color = Color.LightPink;//ParseColor("#760000");//Bloody Red
                else
                    color = Color.Pink;//ParseColor("#400000");//Darker Bloody red
            }
            else if (fieldType == AField.FieldType.TEXT)
            {
                if (light)
                    color = Color.LightSteelBlue;//ParseColor("#000075");//Dark blue
                else
                    color = Color.LightSkyBlue;//ParseColor("#000040");//Darker Blue
            }
            else if (fieldType == AField.FieldType.INTEGER)
            {
                if (light)
                    color = Color.LightGreen;//ParseColor("#007500");//Green
                else
                    color = Color.LightSeaGreen;//ParseColor("#004000");//Dark Green
            }
            else if (fieldType == AField.FieldType.REAL)
            {
                if (light)
                    color = Color.LimeGreen;//ParseColor("#507550");//light green
                else
                    color = Color.Lime;//ParseColor("#254025");//Darker Green
            }
            else if (fieldType == AField.FieldType.BLOB)
            {
                if (light)
                    color = Color.DarkGray; // .parseColor("#509a4a");
                else
                    color = Color.Black; // parseColor("#458540");
            }
            return color;
        }
        /// <summary>
        /// Append rows to the table grid
        /// @param table The layout to add the rows to
        /// </summary>
        /// <param name="aTable">if set to <c>true</c> it is [a table] and false if it is a view</param>
        private void newAppendRows(bool aTable)
        {
            _aTable.RemoveAllViews();
            if (_data == null)
                return;
            int rowSize = _data.Length;
            if (rowSize == 0)
                return;
            int colSize = _data[0].getFields().Length;
            //Go through all the rows of data
            for (int i = 0; i < rowSize; i++)
            {
                int rowNo = i;
                TableRow row = new TableRow(this);
                //Make every second row dark gray
                if (i % 2 == 1)
                    row.SetBackgroundColor(Color.DarkGray);
                //Go through all the columns
                for (int j = 0; j < colSize; j++)
                {
                    if (!aTable || j > 0)
                    {
                        TextView c = new TextView(this);
                        c.TextSize = _fontSize;
                        if (_maxWidth > 0)
                            c.SetMaxWidth(_maxWidth);
                        if (aTable)
                            c.Text = _data[i].getFields()[j].getFieldData();
                        else
                            c.Text = _data[i].getFields()[j].getFieldData();
                        c.SetBackgroundColor(getBackgroundColor(_data[i].getFields()[j].getFieldType(), (i % 2 == 1)));
                        c.SetPadding(5, 5, 5, 5);
                        // Adding a onClickListener to copy cell value to clip board
                        //ToDo: if the field is a BLOB field make it possible to retrieve / change it
                        c.Click += (sender, e) =>
                        {
                            // if the field is not a BLOB field copy it to the clip board
                            string text = c.Text;
                            BuildVersionCodes currentapiVersion = Android.OS.Build.VERSION.SdkInt;

                            try
                            {
                                if (currentapiVersion >= BuildVersionCodes.Honeycomb)
                                { // HONEYCOMB
                                    Android.Content.ClipboardManager clipboard = (Android.Content.ClipboardManager)GetSystemService(ClipboardService);
                                    ClipData clip = ClipData.NewPlainText("simple text", text);
                                    clipboard.PrimaryClip = clip;
                                }
                                else
                                {
                                    Android.Text.ClipboardManager clipboard = (Android.Text.ClipboardManager)GetSystemService(ClipboardService);
                                    clipboard.Text = text;
                                }
                                ExceptionHandler.toastMsg(_cont, GetText(Resource.String.CopiedToClipboard));
                            }
                            catch (Exception ex)
                            {
                                ExceptionHandler.toastMsg(this, ex.Message);
                                ExceptionHandler.logDActivity(ex.ToString(), _logging, TAG);
                            }
                        };
                        row.AddView(c);
                    }
                    // Add a cell with "Select"
                    if (j == colSize - 1
                            && (aTable || (_canInsertInView || _canUpdateView || _canDeleteView)))
                    {
                        TextView c = addSelectField(rowNo, aTable);
                        row.AddView(c);
                    }
                }
                _trLastRow = row;
                _aTable.AddView(row, new TableLayout.LayoutParams());
            }
        }
        /// <summary>
        /// Create a TextView with OnClickListener used to /edit the row
        /// </summary>
        /// <param name="rowNo">The rowId for the record taped on</param>
        /// <param name="aTable">if set to <c>true</c> [a table].The table name</param>
        /// <returns>A TextView with that start a record editor</returns>
        private TextView addSelectField(int rowNo, bool aTable)
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
            if (aTable)
            {
                if (_data[rowNo].getFields()[0].getFieldData() != "")
                {
                    c.Hint = _data[rowNo].getFields()[0].getFieldData();
                    c.Id = id;
                }
            }
            else
            {
                c.Hint = rowNo.ToString();
                c.Id = id;
            }
            c.SetPadding(5, 5, 5, 5);
            // TODO More efficient to make one OnClickListener and assign this to all
            // records edit field?
            c.Text = GetText(Resource.String.Select);
            if (aTable || (_canUpdateView || _canDeleteView))
            { // _canInsertInView ||
                c.Click += (sender, e) =>
                {
                    try
                    {
                        long rowid;
                        if (c.Hint != null)
                        {
                            if (aTable)
                            {
                                rowid = long.Parse(c.Hint);
                            }
                            else
                            {
                                rowid = long.Parse(c.Hint);
                            }
                            // Utils.logD("rowId in hint", _logging);
                        }
                        else
                            rowid = (long)rowNo - 1;
                        TableField[] rec = null;
                        if (aTable)
                        {
                            using (dbHelper = new NmdMobileDBAdapter(this))
                            {
                                dbHelper.OpenOrCreateDatabase(((SharedEnviroment)ApplicationContext).ActionArgument);
                                rec = dbHelper.getRecord(_table, rowid);
                            }
                        }
                        // TODO Handle View, comment by Shahrooz 20140219
                        // TODO OK to UI must be handled as a special case during update
                        /* Create new result intent */
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
            }
            else
            {
                c.PaintFlags = c.PaintFlags | PaintFlags.StrikeThruText;
            }
            return c;
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