/*
 * Author: Shahrooz Sabet
 * Date: 20150504
 * */
#region using
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Data;
#endregion
namespace NamaadMobile.Function
{
    class NConvert
    {
        #region Function
        /// <summary>
        /// Convert the SQLite 0 / 1 boolean to Java boolean 
        /// </summary>
        /// <param name="intBool">The int bool.</param>
        /// <returns>Boolean of the given int</returns>
        public static bool Int2Bool(int intBool)
        {
            if (intBool == 1)
                return true;
            return false;
        }
        /// <summary>
        /// Convert Boolean to the int.
        /// </summary>
        /// <param name="boolInt">if set to <c>true</c> return 1</param>
        /// <returns>return 1 if boolInt is True, Otherwise 0</returns>
        public static int Bool2Int(bool boolInt)
        {
            if (boolInt)
                return 1;
            return 0;
        }
        /// <summary>
        /// Convert the given Datarow dr To the data table.
        /// </summary>
        /// <param name="dr">The dr.</param>
        /// <returns>A Datatable with a row which is dr </returns>
        public static DataTable ToDataTable(DataRow dr)
        {
            DataTable dt = dr.Table.Clone();
            DataRow drNew = dt.NewRow();
            drNew.ItemArray = dr.ItemArray;
            dt.Rows.Add(drNew);
            dt.AcceptChanges();
            return dt;
        }
        /// <summary>
        /// Convert the array of Datarow drArr, To the data table.
        /// </summary>
        /// <param name="drArr">The dr arr.</param>
        /// <returns>A Datatable which inclued drArr</returns>
        public static DataTable ToDataTable(DataRow[] drArr)
        {
            DataTable dt = drArr[0].Table.Clone();
            foreach (DataRow dr in drArr)
            {
                DataRow drNew = dt.NewRow();
                drNew.ItemArray = dr.ItemArray;
                dt.Rows.Add(drNew);
            }
            dt.AcceptChanges();
            return dt;
        }
        #endregion
    }
}