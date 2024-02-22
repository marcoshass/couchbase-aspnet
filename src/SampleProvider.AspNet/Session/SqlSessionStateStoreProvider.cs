using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.SessionState;

//CREATE TABLE [dbo].[Sessions](
//	[SessionId] [nvarchar](80) NOT NULL,
//	[ApplicationName] [nvarchar](255) NOT NULL,
//	[Created] [datetime] NOT NULL,
//	[Expires] [datetime] NOT NULL,
//	[LockDate] [datetime] NOT NULL,
//	[LockId] [int] NOT NULL,
//	[Timeout] [int] NOT NULL,
//	[Locked] [bit] NOT NULL,
//	[SessionItems] [text] NULL,
//	[Flags] [int] NOT NULL,
// CONSTRAINT [PKSessions] PRIMARY KEY CLUSTERED 
//(
//	[SessionId] ASC,
//	[ApplicationName] ASC
//)
namespace SampleProvider.AspNet.Session
{

    public class SqlSessionStateStoreProvider : SessionStateStoreProviderBase
    {
        private readonly object _syncObj = new object();
        private SessionStateSection Config {  get; set; }

        /// <summary>
        /// Takes as input the name of the provider and a NameValueCollection instance of 
        /// configuration settings. This method is used to set property values for the 
        /// provider instance, including implementation-specific values and 
        /// options specified in the configuration file (Machine.config or Web.config).
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>
        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);
            lock (_syncObj)
            {
                var appName = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;
                var webConfig = WebConfigurationManager.OpenWebConfiguration(appName);
                Config = (SessionStateSection)webConfig.GetSection("system.web/sessionState");
            }
        }

        //public /*override*/ void Initialize1(string name, NameValueCollection config)
        //{
        //    if (config is null)
        //        throw new ArgumentNullException("config");

        //    if (name is null || name.Length == 0)
        //        name = "OdbcSessionStateStore";

        //    if (string.IsNullOrEmpty(config["description"]))
        //    {
        //        config.Remove("description");
        //        config.Add("description", "Sample ODBC Session State Store provider");
        //    }

        //    // Initialize the abstract base class.
        //    base.Initialize(name, config);


        //    // 
        //    // Initialize the ApplicationName property.
        //    // 

        //    pApplicationName = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;


        //    // 
        //    // Get <sessionState> configuration element.
        //    // 

        //    Global.System.Configuration.Configuration cfg = WebConfigurationManager.OpenWebConfiguration(ApplicationName);
        //    pConfig = (SessionStateSection)cfg.GetSection("system.web/sessionState");


        //    // 
        //    // Initialize OdbcConnection.
        //    // 

        //    pConnectionStringSettings = ConfigurationManager.ConnectionStrings(config["connectionStringName"]);

        //    if (pConnectionStringSettings is null || pConnectionStringSettings.ConnectionString.Trim() == "")
        //    {

        //        throw new HttpException("Connection string cannot be blank.");
        //    }

        //    connectionString = pConnectionStringSettings.ConnectionString;


        //    pWriteExceptionsToEventLog = false;

        //    if (config["writeExceptionsToEventLog"] is not null)
        //    {
        //        if (config["writeExceptionsToEventLog"].ToUpper() == "TRUE")
        //            pWriteExceptionsToEventLog = true;
        //    }
        //}


        public override SessionStateStoreData CreateNewStoreData(HttpContext context, int timeout)
        {
            throw new NotImplementedException();
        }

        public override void CreateUninitializedItem(HttpContext context, string id, int timeout)
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override void EndRequest(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public override SessionStateStoreData GetItem(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
        {
            throw new NotImplementedException();
        }

        public override SessionStateStoreData GetItemExclusive(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
        {
            throw new NotImplementedException();
        }

        public override void InitializeRequest(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public override void ReleaseItemExclusive(HttpContext context, string id, object lockId)
        {
            throw new NotImplementedException();
        }

        public override void RemoveItem(HttpContext context, string id, object lockId, SessionStateStoreData item)
        {
            throw new NotImplementedException();
        }

        public override void ResetItemTimeout(HttpContext context, string id)
        {
            throw new NotImplementedException();
        }

        public override void SetAndReleaseItemExclusive(HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem)
        {
            throw new NotImplementedException();
        }

        public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
        {
            throw new NotImplementedException();
        }

        /// VBCode

        //private SessionStateSection pConfig = default;
        //private string connectionString;
        //private ConnectionStringSettings pConnectionStringSettings;
        //private string eventSource = "OdbcSessionStateStore";
        //private string eventLog = "Application";
        //private string exceptionMessage = "An exception occurred. Please contact your administrator.";
        //private string pApplicationName;


        //// 
        //// If False, exceptions are thrown to the caller. If True,
        //// exceptions are written to the event log.
        //// 

        //private bool pWriteExceptionsToEventLog = false;

        //public bool WriteExceptionsToEventLog
        //{
        //    get
        //    {
        //        return pWriteExceptionsToEventLog;
        //    }
        //    set
        //    {
        //        pWriteExceptionsToEventLog = value;
        //    }
        //}


        //// 
        //// The ApplicationName property is used to differentiate sessions
        //// in the data source by application.
        //// 

        //public string ApplicationName
        //{
        //    get
        //    {
        //        return pApplicationName;
        //    }
        //}


        //// 
        //// ProviderBase members
        //// 


        //// 
        //// SessionStateStoreProviderBase members
        //// 

        //public override void Dispose()
        //{

        //}


        //// 
        //// SessionStateProviderBase.SetItemExpireCallback
        //// 

        //public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
        //{

        //    return false;
        //}


        //// 
        //// SessionStateProviderBase.SetAndReleaseItemExclusive
        //// 

        //public override void SetAndReleaseItemExclusive(HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem)




        //{

        //    // Serialize the SessionStateItemCollection as a string.
        //    string sessItems = Serialize((SessionStateItemCollection)item.Items);

        //    var conn = new OdbcConnection(connectionString);
        //    OdbcCommand cmd;
        //    OdbcCommand deleteCmd = default;

        //    if (newItem)
        //    {
        //        // OdbcCommand to clear an existing expired session if it exists.
        //        deleteCmd = new OdbcCommand("DELETE FROM Sessions " + "WHERE SessionId = ? AND ApplicationName = ? AND Expires < ?", conn);
        //        deleteCmd.Parameters.Add("@SessionId", OdbcType.VarChar, 80).Value = id;
        //        deleteCmd.Parameters.Add("@ApplicationName", OdbcType.VarChar, 255).Value = ApplicationName;
        //        deleteCmd.Parameters.Add("@Expires", OdbcType.DateTime).Value = DateTime.Now;

        //        // OdbcCommand to insert the New session item.
        //        cmd = new OdbcCommand("INSERT INTO Sessions " + " (SessionId, ApplicationName, Created, Expires, " + "  LockDate, LockId, Timeout, Locked, SessionItems, Flags) " + " Values(?, ?, ?, ?, ?, ? , ?, ?, ?, ?)", conn);


        //        cmd.Parameters.Add("@SessionId", OdbcType.VarChar, 80).Value = id;
        //        cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar, 255).Value = ApplicationName;
        //        cmd.Parameters.Add("@Created", OdbcType.DateTime).Value = DateTime.Now;
        //        cmd.Parameters.Add("@Expires", OdbcType.DateTime).Value = DateTime.Now.AddMinutes((double)item.Timeout);
        //        cmd.Parameters.Add("@LockDate", OdbcType.DateTime).Value = DateTime.Now;
        //        cmd.Parameters.Add("@LockId", OdbcType.Int).Value = 0;
        //        cmd.Parameters.Add("@Timeout", OdbcType.Int).Value = item.Timeout;
        //        cmd.Parameters.Add("@Locked", OdbcType.Bit).Value = false;
        //        cmd.Parameters.Add("@SessionItems", OdbcType.VarChar, sessItems.Length).Value = sessItems;
        //        cmd.Parameters.Add("@Flags", OdbcType.Int).Value = 0;
        //    }
        //    else
        //    {

        //        // OdbcCommand to update the existing session item.
        //        cmd = new OdbcCommand("UPDATE Sessions SET Expires = ?, SessionItems = ?, Locked = ? " + " WHERE SessionId = ? AND ApplicationName = ? AND LockId = ?", conn);

        //        cmd.Parameters.Add("@Expires", OdbcType.DateTime).Value = DateTime.Now.AddMinutes((double)item.Timeout);
        //        cmd.Parameters.Add("@SessionItems", OdbcType.VarChar, sessItems.Length).Value = sessItems;
        //        cmd.Parameters.Add("@Locked", OdbcType.Bit).Value = false;
        //        cmd.Parameters.Add("@SessionId", OdbcType.VarChar, 80).Value = id;
        //        cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar, 255).Value = ApplicationName;
        //        cmd.Parameters.Add("@LockId", OdbcType.Int).Value = lockId;
        //    }

        //    try
        //    {
        //        conn.Open();

        //        if (deleteCmd is not null)
        //            deleteCmd.ExecuteNonQuery();

        //        cmd.ExecuteNonQuery();
        //    }
        //    catch (OdbcException e)
        //    {
        //        if (WriteExceptionsToEventLog)
        //        {
        //            WriteToEventLog(e, "SetAndReleaseItemExclusive");
        //            throw new Exception(exceptionMessage);
        //        }
        //        else
        //        {
        //            throw e;
        //        }
        //    }
        //    finally
        //    {
        //        conn.Close();
        //    }
        //}


        //// 
        //// SessionStateProviderBase.GetItem
        //// 

        //public override SessionStateStoreData GetItem(HttpContext context, string id, ref bool locked, ref TimeSpan lockAge, ref object lockId, ref SessionStateActions actionFlags)





        //{

        //    return GetSessionStoreItem(false, context, id, ref locked, ref lockAge, ref lockId, ref actionFlags);
        //}


        //// 
        //// SessionStateProviderBase.GetItemExclusive
        //// 

        //public override SessionStateStoreData GetItemExclusive(HttpContext context, string id, ref bool locked, ref TimeSpan lockAge, ref object lockId, ref SessionStateActions actionFlags)





        //{

        //    return GetSessionStoreItem(true, context, id, ref locked, ref lockAge, ref lockId, ref actionFlags);
        //}


        //// 
        //// GetSessionStoreItem is called by both the GetItem and 
        //// GetItemExclusive methods. GetSessionStoreItem retrieves the 
        //// session data from the data source. If the lockRecord parameter
        //// is True (in the case of GetItemExclusive), then GetSessionStoreItem
        //// locks the record and sets a New LockId and LockDate.
        //// 

        //private SessionStateStoreData GetSessionStoreItem(bool lockRecord, HttpContext context, string id, ref bool locked, ref TimeSpan lockAge, ref object lockId, ref SessionStateActions actionFlags)






        //{

        //    // Initial values for Return value and out parameters.
        //    SessionStateStoreData item = default;
        //    lockAge = TimeSpan.Zero;
        //    lockId = null;
        //    locked = false;
        //    actionFlags = 0;

        //    // Connection to ODBC database.
        //    var conn = new OdbcConnection(connectionString);
        //    // OdbcCommand for database commands.
        //    OdbcCommand cmd = default;
        //    // DataReader to read database record.
        //    OdbcDataReader reader = default;
        //    // DateTime to check if current session item is expired.
        //    DateTime expires;
        //    // String to hold serialized SessionStateItemCollection.
        //    string serializedItems = "";
        //    // True if a record is found in the database.
        //    bool foundRecord = false;
        //    // True if the returned session item is expired and needs to be deleted.
        //    bool deleteData = false;
        //    // Timeout value from the data store.
        //    int timeout = 0;

        //    try
        //    {
        //        conn.Open();

        //        // lockRecord is True when called from GetItemExclusive and
        //        // False when called from GetItem.
        //        // Obtain a lock if possible. Ignore the record if it is expired.
        //        if (lockRecord)
        //        {
        //            cmd = new OdbcCommand("UPDATE Sessions SET" + " Locked = ?, LockDate = ? " + " WHERE SessionId = ? AND ApplicationName = ? AND Locked = ? AND Expires > ?", conn);


        //            cmd.Parameters.Add("@Locked", OdbcType.Bit).Value = true;
        //            cmd.Parameters.Add("@LockDate", OdbcType.DateTime).Value = DateTime.Now;
        //            cmd.Parameters.Add("@SessionId", OdbcType.VarChar, 80).Value = id;
        //            cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar, 255).Value = ApplicationName;
        //            cmd.Parameters.Add("@Locked", OdbcType.Int).Value = false;
        //            cmd.Parameters.Add("@Expires", OdbcType.DateTime).Value = DateTime.Now;

        //            if (cmd.ExecuteNonQuery() == 0)
        //            {
        //                // No record was updated because the record was locked or not found.
        //                locked = true;
        //            }
        //            else
        //            {
        //                // The record was updated.
        //                locked = false;
        //            }
        //        }

        //        // Retrieve the current session item information.
        //        cmd = new OdbcCommand("SELECT Expires, SessionItems, LockId, LockDate, Flags, Timeout " + "  FROM Sessions " + "  WHERE SessionId = ? AND ApplicationName = ?", conn);


        //        cmd.Parameters.Add("@SessionId", OdbcType.VarChar, 80).Value = id;
        //        cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar, 255).Value = ApplicationName;

        //        // Retrieve session item data from the data source.
        //        reader = cmd.ExecuteReader(CommandBehavior.SingleRow);

        //        while (reader.Read())
        //        {
        //            expires = reader.GetDateTime(0);

        //            if (expires < DateTime.Now)
        //            {
        //                // The record was expired. Mark it as not locked.
        //                locked = false;
        //                // The session was expired. Mark the data for deletion.
        //                deleteData = true;
        //            }
        //            else
        //            {
        //                foundRecord = true;
        //            }

        //            serializedItems = reader.GetString(1);
        //            lockId = reader.GetInt32(2);
        //            lockAge = DateTime.Now.Subtract(reader.GetDateTime(3));
        //            actionFlags = (SessionStateActions)reader.GetInt32(4);
        //            timeout = reader.GetInt32(5);
        //        }

        //        reader.Close();


        //        // If the returned session item is expired, 
        //        // delete the record from the data source.
        //        if (deleteData)
        //        {
        //            cmd = new OdbcCommand("DELETE FROM Sessions " + "WHERE SessionId = ? AND ApplicationName = ?", conn);
        //            cmd.Parameters.Add("@SessionId", OdbcType.VarChar, 80).Value = id;
        //            cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar, 255).Value = ApplicationName;

        //            cmd.ExecuteNonQuery();
        //        }

        //        // The record was not found. Ensure that locked is False.
        //        if (!foundRecord)
        //            locked = false;

        //        // If the record was found and you obtained a lock, then set 
        //        // the lockId, clear the actionFlags,
        //        // and create the SessionStateStoreItem to return.
        //        if (foundRecord && !locked)
        //        {
        //            lockId = Conversions.ToInteger(lockId) + 1;

        //            cmd = new OdbcCommand("UPDATE Sessions SET" + " LockId = ?, Flags = 0 " + " WHERE SessionId = ? AND ApplicationName = ?", conn);

        //            cmd.Parameters.Add("@LockId", OdbcType.Int).Value = lockId;
        //            cmd.Parameters.Add("@SessionId", OdbcType.VarChar, 80).Value = id;
        //            cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar, 255).Value = ApplicationName;

        //            cmd.ExecuteNonQuery();

        //            // If the actionFlags parameter is not InitializeItem, 
        //            // deserialize the stored SessionStateItemCollection.
        //            if (actionFlags == SessionStateActions.InitializeItem)
        //            {
        //                item = CreateNewStoreData(context, pConfig.Timeout.TotalMinutes);
        //            }
        //            else
        //            {
        //                item = Deserialize(context, serializedItems, timeout);
        //            }
        //        }
        //    }
        //    catch (OdbcException e)
        //    {
        //        if (WriteExceptionsToEventLog)
        //        {
        //            WriteToEventLog(e, "GetSessionStoreItem");
        //            throw new Exception(exceptionMessage);
        //        }
        //        else
        //        {
        //            throw e;
        //        }
        //    }
        //    finally
        //    {
        //        if (reader is not null)
        //            reader.Close();
        //        conn.Close();
        //    }

        //    return item;
        //}




        //// 
        //// Serialize is called by the SetAndReleaseItemExclusive method to 
        //// convert the SessionStateItemCollection into a Base64 string to    
        //// be stored in an Access Memo field.
        //// 

        //private string Serialize(SessionStateItemCollection items)
        //{
        //    var ms = new MemoryStream();
        //    var writer = new BinaryWriter(ms);

        //    if (items is not null)
        //        items.Serialize(writer);

        //    writer.Close();

        //    return Convert.ToBase64String(ms.ToArray());
        //}

        //// 
        //// Deserialize is called by the GetSessionStoreItem method to 
        //// convert the Base64 string stored in the Access Memo field to a 
        //// SessionStateItemCollection.
        //// 

        //private SessionStateStoreData Deserialize(HttpContext context, string serializedItems, int timeout)
        //{

        //    var ms = new MemoryStream(Convert.FromBase64String(serializedItems));

        //    var sessionItems = new SessionStateItemCollection();

        //    if (ms.Length > 0L)
        //    {
        //        var reader = new BinaryReader(ms);
        //        sessionItems = SessionStateItemCollection.Deserialize(reader);
        //    }

        //    return new SessionStateStoreData(sessionItems, SessionStateUtility.GetSessionStaticObjects(context), timeout);

        //}

        //// 
        //// SessionStateProviderBase.ReleaseItemExclusive
        //// 

        //public override void ReleaseItemExclusive(HttpContext context, string id, object lockId)

        //{

        //    var conn = new OdbcConnection(connectionString);
        //    var cmd = new OdbcCommand("UPDATE Sessions SET Locked = 0, Expires = ? " + "WHERE SessionId = ? AND ApplicationName = ? AND LockId = ?", conn);

        //    cmd.Parameters.Add("@Expires", OdbcType.DateTime).Value = DateTime.Now.AddMinutes(pConfig.Timeout.TotalMinutes);
        //    cmd.Parameters.Add("@SessionId", OdbcType.VarChar, 80).Value = id;
        //    cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar, 255).Value = ApplicationName;
        //    cmd.Parameters.Add("@LockId", OdbcType.Int).Value = lockId;

        //    try
        //    {
        //        conn.Open();

        //        cmd.ExecuteNonQuery();
        //    }
        //    catch (OdbcException e)
        //    {
        //        if (WriteExceptionsToEventLog)
        //        {
        //            WriteToEventLog(e, "ReleaseItemExclusive");
        //            throw new Exception(exceptionMessage);
        //        }
        //        else
        //        {
        //            throw e;
        //        }
        //    }
        //    finally
        //    {
        //        conn.Close();
        //    }
        //}


        //// 
        //// SessionStateProviderBase.RemoveItem
        //// 

        //public override void RemoveItem(HttpContext context, string id, object lockId, SessionStateStoreData item)


        //{

        //    var conn = new OdbcConnection(connectionString);
        //    var cmd = new OdbcCommand("DELETE * FROM Sessions " + "WHERE SessionId = ? AND ApplicationName = ? AND LockId = ?", conn);
        //    cmd.Parameters.Add("@SessionId", OdbcType.VarChar, 80).Value = id;
        //    cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar, 255).Value = ApplicationName;
        //    cmd.Parameters.Add("@LockId", OdbcType.Int).Value = lockId;

        //    try
        //    {
        //        conn.Open();

        //        cmd.ExecuteNonQuery();
        //    }
        //    catch (OdbcException e)
        //    {
        //        if (WriteExceptionsToEventLog)
        //        {
        //            WriteToEventLog(e, "RemoveItem");
        //            throw new Exception(exceptionMessage);
        //        }
        //        else
        //        {
        //            throw e;
        //        }
        //    }
        //    finally
        //    {
        //        conn.Close();
        //    }
        //}



        //// 
        //// SessionStateProviderBase.CreateUninitializedItem
        //// 

        //public override void CreateUninitializedItem(HttpContext context, string id, int timeout)

        //{

        //    var conn = new OdbcConnection(connectionString);
        //    var cmd = new OdbcCommand("INSERT INTO Sessions " + " (SessionId, ApplicationName, Created, Expires, " + "  LockDate, LockId, Timeout, Locked, SessionItems, Flags) " + " Values(?, ?, ?, ?, ?, ? , ?, ?, ?, ?)", conn);


        //    cmd.Parameters.Add("@SessionId", OdbcType.VarChar, 80).Value = id;
        //    cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar, 255).Value = ApplicationName;
        //    cmd.Parameters.Add("@Created", OdbcType.DateTime).Value = DateTime.Now;
        //    cmd.Parameters.Add("@Expires", OdbcType.DateTime).Value = DateTime.Now.AddMinutes(timeout);
        //    cmd.Parameters.Add("@LockDate", OdbcType.DateTime).Value = DateTime.Now;
        //    cmd.Parameters.Add("@LockId", OdbcType.Int).Value = 0;
        //    cmd.Parameters.Add("@Timeout", OdbcType.Int).Value = timeout;
        //    cmd.Parameters.Add("@Locked", OdbcType.Bit).Value = false;
        //    cmd.Parameters.Add("@SessionItems", OdbcType.VarChar, 0).Value = "";
        //    cmd.Parameters.Add("@Flags", OdbcType.Int).Value = 1;

        //    try
        //    {
        //        conn.Open();

        //        cmd.ExecuteNonQuery();
        //    }
        //    catch (OdbcException e)
        //    {
        //        if (WriteExceptionsToEventLog)
        //        {
        //            WriteToEventLog(e, "CreateUninitializedItem");
        //            throw new Exception(exceptionMessage);
        //        }
        //        else
        //        {
        //            throw e;
        //        }
        //    }
        //    finally
        //    {
        //        conn.Close();
        //    }
        //}


        //// 
        //// SessionStateProviderBase.CreateNewStoreData
        //// 

        //public override SessionStateStoreData CreateNewStoreData(HttpContext context, int timeout)

        //{

        //    return new SessionStateStoreData(new SessionStateItemCollection(), SessionStateUtility.GetSessionStaticObjects(context), timeout);

        //}



        //// 
        //// SessionStateProviderBase.ResetItemTimeout
        //// 

        //public override void ResetItemTimeout(HttpContext context, string id)

        //{
        //    var conn = new OdbcConnection(connectionString);
        //    var cmd = new OdbcCommand("UPDATE Sessions SET Expires = ? " + "WHERE SessionId = ? AND ApplicationName = ?", conn);

        //    cmd.Parameters.Add("@Expires", OdbcType.DateTime).Value = DateTime.Now.AddMinutes(pConfig.Timeout.TotalMinutes);
        //    cmd.Parameters.Add("@SessionId", OdbcType.VarChar, 80).Value = id;
        //    cmd.Parameters.Add("@ApplicationName", OdbcType.VarChar, 255).Value = ApplicationName;

        //    try
        //    {
        //        conn.Open();

        //        cmd.ExecuteNonQuery();
        //    }
        //    catch (OdbcException e)
        //    {
        //        if (WriteExceptionsToEventLog)
        //        {
        //            WriteToEventLog(e, "ResetItemTimeout");
        //            throw new Exception(exceptionMessage);
        //        }
        //        else
        //        {
        //            throw e;
        //        }
        //    }
        //    finally
        //    {
        //        conn.Close();
        //    }
        //}


        //// 
        //// SessionStateProviderBase.InitializeRequest
        //// 

        //public override void InitializeRequest(HttpContext context)
        //{

        //}


        //// 
        //// SessionStateProviderBase.EndRequest
        //// 

        //public override void EndRequest(HttpContext context)
        //{

        //}


        //// 
        //// WriteToEventLog
        //// This is a helper function that writes exception detail to the 
        //// event log. Exceptions are written to the event log as a security
        //// measure to ensure Private database details are not returned to 
        //// browser. If a method does not Return a status or Boolean
        //// indicating the action succeeded or failed, the caller also 
        //// throws a generic exception.
        //// 

        //private void WriteToEventLog(Exception e, string action)
        //{
        //    var log = new EventLog();
        //    log.Source = eventSource;
        //    log.Log = eventLog;

        //    string message = "An exception occurred communicating with the data source." + Constants.vbCrLf + Constants.vbCrLf;
        //    message += "Action: " + action + Constants.vbCrLf + Constants.vbCrLf;
        //    message += "Exception: " + e.ToString();

        //    log.WriteEntry(message);
        //}
    }
}