/*
 * Author: Shahrooz Sabet
 * Date: 20140628
 * Updated:20150801
 * */
#region using
using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Java.Lang;
using NamaadMobile.SharedElement;
#endregion
namespace NamaadMobile.Util
{
    /// <summary>
    /// TODO: A GUI can be created.
    /// TODO: Additional methods can be reduced.
    /// </summary>
    [Activity(Label = "Pref", Theme = "@style/SearchForm")]
    public class Pref : NamaadPrefBase
    {
        #region Defins
        // Option names and default values
        private const string OPT_PAGESIZE = "PageSize";
        private const string OPT_PAGESIZE_DEF = "20";
        private const string OPT_SAVESQL = "SaveSQL";
        private const bool OPT_SAVESQL_DEF = false;
        private const string OPT_FILENO = "RecentFiles";
        private const string OPT_FILENO_DEF = "5";
        private const string OPT_FONTSIZE = "FontSize";
        private const string OPT_FONTSIZE_DEF = "12";
        private const string OPT_FK2LIST = "FKList";
        private const bool OPT_FK2LIST_DEF = false;
        private const string OPT_FKON = "EnableForeignKeys";
        private const bool OPT_FKON_DEF = false;
        private const string OPT_LOGGING = "Logging";
        private const bool OPT_LOGGING_DEF = true;
        private const string OPT_VERTICAL = "MainVertical";
        private const bool OPT_VERTICAL_DEF = false;
        private const string OPT_PAUSE = "Pause";
        private const string OPT_PAUSE_DEF = "500";
        private const string OPT_SULOCATION = "SuShell";
        private const string OPT_SULOCATION_DEF = null;
        private const string OPT_TESTROOT = "TestRoot";
        private const bool OPT_TESTROOT_DEF = false;
        private const string OPT_MAX_WIDTH = "MaxWidth";
        private const string OPT_MAX_WIDTH_DEF = "0";
        private const string OPT_QEDIT_MAX_LINES = "QEMaxLines";
        private const string OPT_QEDIT_MAX_LINES_DEF = "5";
        private const string OPT_QEDIT_MIN_LINES = "QEMinLines";
        private const string OPT_QEDIT_MIN_LINES_DEF = "2";
        private const string OPT_SPATIALITE = "Spatialite";
        private const bool OPT_SPATIALITE_DEF = true;
        private const string IsDBInSDCard = "IsDBInSDCard";
        private const bool IsDBInSDCard_DEF = true;
        #endregion
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            this.SetActionBarTitle(((SharedEnviroment)ApplicationContext).ActionName);
            AddPreferencesFromResource(Resource.Xml.asqlitemanager_setting);
        }
        public static int getQEMaxLines(Context context)
        {
            return validPositiveIntegerOrNumber(PreferenceManager.GetDefaultSharedPreferences(context)
                  .GetString(OPT_QEDIT_MAX_LINES, OPT_QEDIT_MAX_LINES_DEF), 5);
        }
        public static int getQEMinLines(Context context)
        {
            return validPositiveIntegerOrNumber(PreferenceManager.GetDefaultSharedPreferences(context)
                  .GetString(OPT_QEDIT_MIN_LINES, OPT_QEDIT_MIN_LINES_DEF), 1);
        }
        /**
         * Return the numbers of records to retrieve when paging data
         * @param context
         * @return page size
         */
        public static int getPageSize(Context context)
        {
            return validPositiveIntegerOrNumber(PreferenceManager.GetDefaultSharedPreferences(context)
                  .GetString(OPT_PAGESIZE, OPT_PAGESIZE_DEF), 0);
        }
        /**
         * Return the numbers of records to retrieve when paging data
         * @param context
         * @return page size
         */
        public static int getFontSize(Context context)
        {
            return validPositiveIntegerOrNumber(PreferenceManager.GetDefaultSharedPreferences(context)
                  .GetString(OPT_FONTSIZE, OPT_FONTSIZE_DEF), 0);
        }
        /**
         * Return true if executed statements are stored in database
         * @param context
         * @return
         */
        public static bool getSaveSQL(Context context)
        {
            return PreferenceManager.GetDefaultSharedPreferences(context)
                .GetBoolean(OPT_SAVESQL, OPT_SAVESQL_DEF);
        }
        public static int getNoOfFiles(Context context)
        {
            return validPositiveIntegerOrNumber(PreferenceManager.GetDefaultSharedPreferences(context)
                .GetString(OPT_FILENO, OPT_FILENO_DEF), 0);
        }
        public static int getMaxWidth(Context context)
        {
            return validPositiveIntegerOrNumber(PreferenceManager.GetDefaultSharedPreferences(context)
            .GetString(OPT_MAX_WIDTH, OPT_MAX_WIDTH_DEF), 0);
        }
        public static bool getFKList(Context context)
        {
            return PreferenceManager.GetDefaultSharedPreferences(context)
                .GetBoolean(OPT_FK2LIST, OPT_FK2LIST_DEF);
        }
        public static bool getEnableFK(Context context)
        {
            return PreferenceManager.GetDefaultSharedPreferences(context)
                .GetBoolean(OPT_FKON, OPT_FKON_DEF);
        }
        public static bool getLogging(Context context)
        {
            return PreferenceManager.GetDefaultSharedPreferences(context)
                .GetBoolean(OPT_LOGGING, OPT_LOGGING_DEF);
        }
        public static bool getMainVertical(Context context)
        {
            return PreferenceManager.GetDefaultSharedPreferences(context)
                .GetBoolean(OPT_VERTICAL, OPT_VERTICAL_DEF);
        }
        public static int getPause(Context context)
        {
            return validPositiveIntegerOrNumber(PreferenceManager.GetDefaultSharedPreferences(context)
                .GetString(OPT_PAUSE, OPT_PAUSE_DEF), 0);
        }
        public static string getSuLocation(Context context)
        {
            return PreferenceManager.GetDefaultSharedPreferences(context)
            .GetString(OPT_SULOCATION, OPT_SULOCATION_DEF);
        }
        public static bool getTestRoot(Context context)
        {
            return PreferenceManager.GetDefaultSharedPreferences(context)
                .GetBoolean(OPT_TESTROOT, OPT_TESTROOT_DEF);
        }
        public static int getDefaultView(Context context)
        {
            return new Integer(PreferenceManager.GetDefaultSharedPreferences(context)
            .GetString("DefaultView", "1")).IntValue();
        }
        public static bool getUseSpatialite(Context context)
        {
            return PreferenceManager.GetDefaultSharedPreferences(context)
                .GetBoolean(OPT_SPATIALITE, OPT_SPATIALITE_DEF);
        }
        /**
         * convert a Sting to a int >= 0 all negative and invalid numbers are treated as zero
         * ToDo: Exception Handling
         * @param strVal
         * @return
         */
        private static int validPositiveIntegerOrNumber(string strVal, int number)
        {
            if (strVal.Trim().Equals(""))
                strVal = "" + number;
            int i = number;
            i = new Integer(strVal).IntValue();
            if (i < 0)
                i = 0;
            return i;
        }
        public static bool GetIsDBInSDCard(Context context)
        {
            return PreferenceManager.GetDefaultSharedPreferences(context).GetBoolean(IsDBInSDCard, IsDBInSDCard_DEF);
        }
        public static void PutIsNotDBInSDCard(Context context)
        {
            PreferenceManager.GetDefaultSharedPreferences(context).Edit().PutBoolean(IsDBInSDCard, false).Commit();
        }
        public static void PutIsDBInSDCard(Context context)
        {
            PreferenceManager.GetDefaultSharedPreferences(context).Edit().PutBoolean(IsDBInSDCard, true).Commit();
        }
    }
}