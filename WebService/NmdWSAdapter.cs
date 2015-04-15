/*
 * Author: Shahrooz Sabet
 * Date: 20140628
 * */
#region using
using Android.Content;
using Android.Telephony;
using NamaadMobile.Data;
using NamaadMobile.SharedElement;
using NamaadMobile.Util;
using System;
using System.Data;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.Threading.Tasks;
#endregion
namespace NamaadMobile.WebService
{
    /// <summary>
    /// TODO: Web Service account should be hardcoded?
    /// TODO: Should use zip dll to compress/decompress data
    /// </summary>
    class NmdWSAdapter : IDisposable
    {
        #region Defins
        /// <summary>
        /// The tag For Debugging purpose
        /// </summary>
        private const string TAG = "NamaadMobile.WebService.NmdWSAdapter";
        private Context mCtx;
        private bool _logging;

        private static EndpointAddress EndPoint = new EndpointAddress("https://192.168.100.78/NmdMobileWCFService/Service.svc");
        private readonly string Username;
        private static string Password = "_123456";
        private IService _client;
        //private ChannelFactory<IService> cf;

        private NmdMobileDBAdapter dbHelper;
        private bool flagCon = false;
        #endregion
        public NmdWSAdapter(Context ctx)
        {
            mCtx = ctx;
            _logging = Prefs.getLogging(ctx);
            Username = ((TelephonyManager)ctx.GetSystemService(Context.TelephonyService)).DeviceId;
        }

        public void SetDbHelper(NmdMobileDBAdapter dbHelper, string dBNameNamaad, string dBNameClient)
        {
            this.dbHelper = dbHelper;
            ((SharedEnviroment)(mCtx.ApplicationContext)).DbNameServer = dBNameNamaad;
            ((SharedEnviroment)(mCtx.ApplicationContext)).DbNameClient = dBNameClient;
        }

        /// <summary>
        /// Refreshes the Sync ws call
        /// </summary>
        /// <param name="dt">The dt.</param>
        /// <param name="userCode">The user code.</param>
        /// <returns></returns>
        public async Task RefreshWSCall(DataTable dt, int userCode)//DBNameNamaad=DBNamaad
        {
            //Task<RefreshDataCT> res = null;
            RefreshDataCT res = null;
            if (dt.Rows.Count == 0)
                return;

            if (dbHelper == null)
            {
                dbHelper = new NmdMobileDBAdapter(mCtx);
                if (int.Parse(dt.Rows[0]["SystemCode"].ToString()) == 1)
                    dbHelper.OpenOrCreateDatabase(((SharedEnviroment)mCtx.ApplicationContext).DbNameServer);
                else
                    dbHelper.OpenOrCreateDatabase(((SharedEnviroment)mCtx.ApplicationContext).DbNameClient);
                flagCon = true;
            }
            InitializeHelloWorldServiceClient();
            RefreshDataCT ct = new RefreshDataCT();
            using (dt)
            {
                foreach (DataRow row in dt.Rows)
                {
                    ct.SystemCode = int.Parse(row["SystemCode"].ToString());
                    ct.TableCode = int.Parse(row["TableCode"].ToString());
                    ct.TableName = (string)row["tableName"];
                    ct.TableDataVersion = 0;
                    if (ct.TableCode >= 1000 && dbHelper.Exist(ct.TableName))
                    {
                        string s;
                        if (!string.IsNullOrEmpty(s = dbHelper.ExecuteSQL("Select Max(TableDataVersion) As maxTableDataVersion From " + ct.TableName).Rows[0]["maxTableDataVersion"].ToString()))
                            ct.TableDataVersion = int.Parse(s);
                    }
                    ct.UserCode = userCode;

                    res = await Task<RefreshDataCT>.Factory.FromAsync(_client.BeginRefreshData, _client.EndRefreshData, ct, null);
                    //res.Wait();//For Throwing exception, otherwise next line will be executed in case of exception.

                    //ReciveGetData(res.Result);
                    //ReciveGetData(_client.RefreshData(ct));
                    ReciveGetData(res);
                }
            }
        }
        private void ReciveGetData(RefreshDataCT ct)
        {
            string sqlString = null;

            if (ct.HasCreateTable)
            {
                dbHelper.DropTable(ct.TableName);
                if (ct.QueryCreateTable != "")
                {
                    sqlString = ct.QueryCreateTable.Replace("@" + ct.TableName, ct.TableName);
                    dbHelper.ExecuteNonQuery(sqlString);
                }
            }
            if (ct.QueryBeforeExec1 != "")
            {
                sqlString = ct.QueryBeforeExec1.Replace("@" + ct.TableName, ct.TableName);
                dbHelper.ExecuteNonQuery(sqlString);
            }
            if (ct.QueryBeforeExec2 != "")
            {
                sqlString = ct.QueryBeforeExec2.Replace("@" + ct.TableName, ct.TableName);
                dbHelper.ExecuteNonQuery(sqlString);
            }

            string destTableName = "";
            dbHelper.DropTable("Tmp" + ct.TableName);

            using (DataTable dt = ToDataTable(ct.DataTableArray))
                if (dt.Rows.Count > 0)
                {
                    destTableName = ct.TableName;
                    if (ct.IsCreateTmpTable)
                    {
                        destTableName = "Tmp" + destTableName;
                        sqlString = ct.QueryCreateTable.Replace("@" + ct.TableName, destTableName);
                        dbHelper.ExecuteNonQuery(sqlString);
                    }
                    dbHelper.BeginTransaction();
                    dbHelper.CopyToDB(dt, destTableName, ct.TableName, ct.DeleteKey);
                    dbHelper.EndTransaction();

                    if (ct.QueryAfterExec1 != "")
                    {
                        sqlString = ct.QueryAfterExec1.Replace("@" + ct.TableName, ct.TableName);
                        //sqlString = sqlString.Replace( "\r\n", " ");
                        dbHelper.ExecuteNonQuery(sqlString);
                    }
                    if (ct.QueryAfterExec2 != "")
                    {
                        sqlString = ct.QueryAfterExec2.Replace("@" + ct.TableName, ct.TableName);
                        dbHelper.ExecuteNonQuery(sqlString);
                    }
                    if (ct.QueryAfterExec3 != "")
                    {
                        sqlString = ct.QueryAfterExec3.Replace("@" + ct.TableName, ct.TableName);
                        dbHelper.ExecuteNonQuery(sqlString);
                    }
                }
            dbHelper.DropTable("Tmp" + ct.TableName);

        }
        private DataTable ToDataTable(byte[] dataTableArray)
        {
            using (MemoryStream ms = new MemoryStream(dataTableArray))
            {
                BinaryFormatter binFormatter = new BinaryFormatter();
                return ((DataTable)binFormatter.Deserialize(ms));
            }
        }
        private void InitializeHelloWorldServiceClient()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback +=
            (se, cert, chain, sslerror) => { return true; };

            _client = new ServiceClient(CreateBasicHttp(), EndPoint);
            ((System.ServiceModel.ClientBase<IService>)_client).ClientCredentials.UserName.UserName = Username;
            ((System.ServiceModel.ClientBase<IService>)_client).ClientCredentials.UserName.Password = Password;
        }
        private static BasicHttpBinding CreateBasicHttp()
        {
            BasicHttpBinding binding = new BasicHttpBinding
            {
                Name = "basicHttpBinding",
                MaxBufferSize = 2147483647,
                MaxReceivedMessageSize = 2147483647,
            };
            TimeSpan timeout = new TimeSpan(0, 0, 30);
            binding.SendTimeout = timeout;
            binding.OpenTimeout = timeout;
            binding.ReceiveTimeout = timeout;

            binding.Security.Mode = BasicHttpSecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
            binding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
            binding.Security.Transport.Realm = "";
            return binding;
        }

        public void Dispose()
        {
            if (flagCon == true)
            {
                dbHelper.Dispose();
                dbHelper = null;
            }
            mCtx = null;
            ((System.ServiceModel.ClientBase<IService>)_client).Close();
        }
    }
}


//cf = new ChannelFactory<IService>(CreateBasicHttp(), EndPoint);
//cf.Credentials.UserName.UserName = "_123456";
//cf.Credentials.UserName.Password = "_123456";
//_client = cf.CreateChannel();
//_client = new Service();
//_client.Credentials = new NetworkCredential("_123456", "_123456");
//_client.Dispose();
//((IClientChannel)_client).Close();
//cf.Close();