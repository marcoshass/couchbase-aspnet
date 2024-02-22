using Common.Logging;
using System;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.Configuration;
using System.Web.SessionState;

/*
CREATE TABLE [dbo].[Sessions](
	[SessionId] [nvarchar](80) NOT NULL,
	[ApplicationName] [nvarchar](255) NOT NULL,
	[Created] [datetime] NOT NULL,
	[Expires] [datetime] NOT NULL,
	[LockDate] [datetime] NOT NULL,
	[LockId] [int] NOT NULL,
	[Timeout] [int] NOT NULL,
	[Locked] [bit] NOT NULL,
	[SessionItems] [ntext] NULL,
	[Flags] [int] NOT NULL,
 CONSTRAINT [PKSessions] PRIMARY KEY CLUSTERED 
 (
	[SessionId] ASC,
	[ApplicationName] ASC 
  )
)
 */
namespace SampleProvider.AspNet.Session
{

    public class SqlSessionStateStoreProvider : SessionStateStoreProviderBase, ISqlWebProvider
    {
        private readonly object _syncObj = new object();
        private readonly ILog _log = LogManager.GetLogger<SqlSessionStateStoreProvider>();

        public string ApplicationName { get; set; }
        public string ConnectionString { get; set; }
        public string Database { get; set; }
        public bool ThrowOnError { get; set; }
        public string Schema { get; set; }
        public string Table { get; set; }

        private SessionStateSection Config { get; set; }

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
                ApplicationName = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;
                var webConfig = WebConfigurationManager.OpenWebConfiguration(ApplicationName);
                var connectionString = webConfig.ConnectionStrings.ConnectionStrings[config["connectionStringName"]];
                Config = (SessionStateSection)webConfig.GetSection("system.web/sessionState");
                ConnectionString =  connectionString.ConnectionString;

                var bootStrapper = new BootStrapper();
                bootStrapper.Bootstrap(name, config, this);
            }
        }

        public override void Dispose()
        {
        }

        #region Not Supported

        public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
        {
            return false;
        }

        public override void SetAndReleaseItemExclusive(HttpContext context,
            string id,
            SessionStateStoreData item,
            object lockId,
            bool newItem)
        {
            // Serialize the SessionStateItemCollection as a string.
            string sessItems = Serialize((SessionStateItemCollection)item.Items);

            var conn = new SqlConnection(ConnectionString);
            SqlCommand cmd;
            SqlCommand deleteCmd = null;

            if (newItem)
            {
                // SqlCommand to clear an existing expired session if it exists.
                deleteCmd = new SqlCommand($@"
DELETE FROM [{Schema}].[Sessions]
WHERE SessionId = @SessionId AND
      ApplicationName = @ApplicationName AND
      Expires < @Expires", conn);
                deleteCmd.Parameters.Add(new SqlParameter("@SessionId", SqlDbType.NVarChar, 80)).Value = id;
                deleteCmd.Parameters.Add(new SqlParameter("@ApplicationName", SqlDbType.NVarChar, 255)).Value = ApplicationName;
                deleteCmd.Parameters.Add(new SqlParameter("@Expires", SqlDbType.DateTime)).Value = DateTime.Now;

                // SqlCommand to insert the New session item.
                cmd = new SqlCommand($@"
INSERT INTO [{Schema}].[Sessions]
           (SessionId
           ,ApplicationName
           ,Created
           ,Expires
           ,LockDate
           ,LockId
           ,Timeout
           ,Locked
           ,SessionItems
           ,Flags)
     VALUES
           (@SessionId
           ,@ApplicationName
           ,@Created
           ,@Expires
           ,@LockDate
           ,@LockId
           ,@Timeout
           ,@Locked
           ,@SessionItems
           ,@Flags)", conn);

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(new SqlParameter("@SessionId", SqlDbType.NVarChar, 80)).Value = id;
                cmd.Parameters.Add(new SqlParameter("@ApplicationName", SqlDbType.NVarChar, 255)).Value = ApplicationName;
                cmd.Parameters.Add(new SqlParameter("@Created", SqlDbType.DateTime)).Value = DateTime.Now;
                cmd.Parameters.Add(new SqlParameter("@Expires", SqlDbType.DateTime)).Value = DateTime.Now.AddMinutes(item.Timeout);
                cmd.Parameters.Add(new SqlParameter("@LockDate", SqlDbType.DateTime)).Value = DateTime.Now;
                cmd.Parameters.Add(new SqlParameter("@LockId", SqlDbType.Int)).Value = 0;
                cmd.Parameters.Add(new SqlParameter("@Timeout", SqlDbType.Int)).Value = item.Timeout;
                cmd.Parameters.Add(new SqlParameter("@Locked", SqlDbType.Bit)).Value = 0;
                cmd.Parameters.Add(new SqlParameter("@SessionItems", SqlDbType.NVarChar, -1)).Value = sessItems;
                cmd.Parameters.Add(new SqlParameter("@Flags", SqlDbType.Int)).Value = 0;
            }
            else
            {
                // SqlCommand to update the existing session item.
                cmd = new SqlCommand($@"
UPDATE [{Schema}].[Sessions]
   SET Expires = @Expires
      ,SessionItems = @SessionItems
      ,Locked = @Locked
 WHERE SessionId = @SessionId AND
       ApplicationName = @ApplicationName AND 
       LockId = @LockId", conn);
                cmd.Parameters.Add(new SqlParameter("@Expires", SqlDbType.DateTime)).Value = DateTime.Now.AddMinutes(item.Timeout);
                cmd.Parameters.Add(new SqlParameter("@SessionItems", SqlDbType.NVarChar, -1)).Value = sessItems;
                cmd.Parameters.Add(new SqlParameter("@Locked", SqlDbType.Bit)).Value = 0;
                cmd.Parameters.Add(new SqlParameter("@SessionId", SqlDbType.NVarChar, 80)).Value = id;
                cmd.Parameters.Add(new SqlParameter("@ApplicationName", SqlDbType.NVarChar, 255)).Value = ApplicationName;
                cmd.Parameters.Add(new SqlParameter("@LockId", SqlDbType.Int)).Value = lockId;
            }

            try
            {
                conn.Open();

                if (deleteCmd != null)
                    deleteCmd.ExecuteNonQuery();

                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                LogAndOrThrow(e, id);
            }
            finally
            {
                conn.Close();
            }
        }

        public override SessionStateStoreData GetItem(HttpContext context,
            string id,
            out bool locked,
            out TimeSpan lockAge,
            out object lockId,
            out SessionStateActions actionFlags)
        {
            return GetSessionStoreItem(false, context, id, out locked, out lockAge, out lockId, out actionFlags);
        }

        public override SessionStateStoreData GetItemExclusive(HttpContext context, 
            string id, 
            out bool locked, 
            out TimeSpan lockAge,
            out object lockId, 
            out SessionStateActions actions)
        {
            _log.TraceFormat("GetSessionExclusive called for item {0}", id);
            return GetSessionStoreItem(true, context, id, out locked, out lockAge, out lockId, out actions);
        }

        /// <summary>
        /// GetSessionStoreItem is called by both the GetItem and 
        /// GetItemExclusive methods. GetSessionStoreItem retrieves the 
        /// session data from the data source. If the lockRecord parameter
        /// is True (in the case of GetItemExclusive), then GetSessionStoreItem
        /// locks the record and sets a New LockId and LockDate.
        /// </summary>
        /// <param name="lockRecord"></param>
        /// <param name="context"></param>
        /// <param name="id"></param>
        /// <param name="locked"></param>
        /// <param name="lockAge"></param>
        /// <param name="lockId"></param>
        /// <param name="actionFlags"></param>
        /// <returns></returns>
        private SessionStateStoreData GetSessionStoreItem(bool lockRecord,
            HttpContext context,
            string id,
            out bool locked,
            out TimeSpan lockAge,
            out object lockId,
            out SessionStateActions actionFlags)
        {
            SessionStateStoreData item = null;
            lockAge = TimeSpan.Zero;
            lockId = null;
            locked = false;
            actionFlags = SessionStateActions.None;

            var conn = new SqlConnection(ConnectionString);
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            DateTime expires;
            var serializedItems = "";
            var foundRecord = false;
            var deleteData = false;
            int timeout = 0;

            try
            {
                conn.Open();

                // lockRecord is True when called from GetItemExclusive and
                // False when called from GetItem.
                // Obtain a lock if possible. Ignore the record if it is expired.
                if (lockRecord)
                {
                    cmd = new SqlCommand($@"
UPDATE [{Schema}].[Sessions]
   SET Locked = @Locked
      ,LockDate = @LockDate
 WHERE SessionId = @SessionId AND
       ApplicationName = @ApplicationName AND 
       Locked = @PrevLocked AND 
       Expires > @Expires", conn);

                    cmd.Parameters.Add(new SqlParameter("@Locked", SqlDbType.Bit)).Value = 1;
                    cmd.Parameters.Add(new SqlParameter("@LockDate", SqlDbType.DateTime)).Value = DateTime.Now;
                    cmd.Parameters.Add(new SqlParameter("@SessionId", SqlDbType.NVarChar, 80)).Value = id;
                    cmd.Parameters.Add(new SqlParameter("@ApplicationName", SqlDbType.NVarChar, 255)).Value = ApplicationName;
                    cmd.Parameters.Add(new SqlParameter("@PrevLocked", SqlDbType.Bit)).Value = 0;
                    cmd.Parameters.Add(new SqlParameter("@Expires", SqlDbType.DateTime)).Value = DateTime.Now;

                    if (cmd.ExecuteNonQuery() == 0)
                    {
                        // No record was updated because the record was locked or not found.
                        locked = true;
                    }
                    else
                    {
                        // The record was updated.
                        locked = false;
                    }
                }

                // Retrieve the current session item information.
                cmd = new SqlCommand($@"
SELECT Expires
      ,SessionItems
      ,LockId
      ,LockDate
      ,Flags
      ,Timeout
  FROM [{Schema}].[Sessions]
WHERE
    SessionId = @SessionId AND
    ApplicationName = @ApplicationName", conn);
                cmd.Parameters.Add(new SqlParameter("@SessionId", SqlDbType.NVarChar, 80)).Value = id;
                cmd.Parameters.Add(new SqlParameter("@ApplicationName", SqlDbType.NVarChar, 255)).Value = ApplicationName;

                // Retrieve session item data from the data source.
                reader = cmd.ExecuteReader(CommandBehavior.SingleRow);

                while (reader.Read())
                {
                    expires = reader.GetDateTime(0);

                    if (expires < DateTime.Now)
                    {
                        // The record was expired. Mark it as not locked.
                        locked = false;
                        // The session was expired. Mark the data for deletion.
                        deleteData = true;
                    }
                    else
                    {
                        foundRecord = true;
                    }

                    serializedItems = reader.GetString(1);
                    lockId = reader.GetInt32(2);
                    lockAge = DateTime.Now.Subtract(reader.GetDateTime(3));
                    actionFlags = (SessionStateActions)reader.GetInt32(4);
                    timeout = reader.GetInt32(5);
                }

                reader.Close();

                // If the returned session item is expired, 
                // delete the record from the data source.
                if (deleteData)
                {
                    cmd = new SqlCommand($@"
DELETE FROM [{Schema}].[Sessions]
WHERE
    SessionId = @SessionId AND
    ApplicationName = @ApplicationName", conn);
                    cmd.Parameters.Add(new SqlParameter("@SessionId", SqlDbType.NVarChar, 80)).Value = id;
                    cmd.Parameters.Add(new SqlParameter("@ApplicationName", SqlDbType.NVarChar, 255)).Value = ApplicationName;

                    cmd.ExecuteNonQuery();
                }

                // The record was not found. Ensure that locked is False.
                if (!foundRecord)
                    locked = false;

                // If the record was found and you obtained a lock, then set 
                // the lockId, clear the actionFlags,
                // and create the SessionStateStoreItem to return.
                if (foundRecord && !locked)
                {
                    lockId = (int)lockId + 1;

                    cmd = new SqlCommand($@"
UPDATE [{Schema}].[Sessions]
   SET LockId = @LockId
      ,Flags = 0
 WHERE SessionId = @SessionId AND
       ApplicationName = @ApplicationName", conn);
                    cmd.Parameters.Add(new SqlParameter("@LockId", SqlDbType.Int)).Value = lockId;
                    cmd.Parameters.Add(new SqlParameter("@SessionId", SqlDbType.NVarChar, 80)).Value = id;
                    cmd.Parameters.Add(new SqlParameter("@ApplicationName", SqlDbType.NVarChar, 255)).Value = ApplicationName;

                    cmd.ExecuteNonQuery();

                    // If the actionFlags parameter is not InitializeItem, 
                    // deserialize the stored SessionStateItemCollection.
                    if (actionFlags == SessionStateActions.InitializeItem)
                    {
                        item = CreateNewStoreData(context, (int)Config.Timeout.TotalMinutes);
                    }
                    else
                    {
                        item = Deserialize(context, serializedItems, timeout);
                    }
                }
            }
            catch (Exception e)
            {
                LogAndOrThrow(e, id);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                conn.Close();
            }

            return item;
        }

        /// <summary>
        /// Serialize is called by the SetAndReleaseItemExclusive method to 
        /// convert the SessionStateItemCollection into a Base64 string to    
        /// be stored in an Access Memo field.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        private string Serialize(SessionStateItemCollection items)
        {
            using (var ms = new MemoryStream())
            {
                var writer = new BinaryWriter(ms);
                if (items != null)
                    items.Serialize(writer);

                return Convert.ToBase64String(ms.ToArray());
            }
        }

        /// <summary>
        /// Deserialize is called by the GetSessionStoreItem method to 
        /// convert the Base64 string stored in the Access Memo field to a
        /// SessionStateItemCollection.</summary>
        /// <param name="context"></param>
        /// <param name="serializedItems"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private SessionStateStoreData Deserialize(HttpContext context, string serializedItems, int timeout)
        {
            using (var ms = new MemoryStream(Convert.FromBase64String(serializedItems)))
            {
                var sessionItems = new SessionStateItemCollection();

                if (ms.Length > 0)
                {
                    var reader = new BinaryReader(ms);
                    sessionItems = SessionStateItemCollection.Deserialize(reader);
                }

                return new SessionStateStoreData(sessionItems,
                    SessionStateUtility.GetSessionStaticObjects(context), timeout);
            }
        }

        /*
         * Takes as input the HttpContext instance for the current request, the SessionID value for the current request,
         * and the lock identifier for the current request, and releases the lock on an item in the session data store.
         * This method is called when the GetItem or GetItemExclusive method is called and the data store specifies that
         * the requested item is locked, but the lock age has exceeded the ExecutionTimeout value. The lock is cleared by
         * this method, freeing the item for use by other requests.
         * */
        public override void ReleaseItemExclusive(HttpContext context, string id, object lockId)
        {
            var conn = new SqlConnection(ConnectionString);
            var cmd = new SqlCommand($@"
UPDATE [{Schema}].[Sessions]
   SET Locked = 0
      ,Expires = @Expires
 WHERE SessionId = @SessionId AND
       ApplicationName = @ApplicationName AND 
       LockId = @LockId", conn);
            cmd.Parameters.Add(new SqlParameter("@Expires", SqlDbType.DateTime)).Value = DateTime.Now.AddMinutes(Config.Timeout.TotalMinutes);
            cmd.Parameters.Add(new SqlParameter("@SessionId", SqlDbType.NVarChar, 80)).Value = id;
            cmd.Parameters.Add(new SqlParameter("@ApplicationName", SqlDbType.NVarChar, 255)).Value = ApplicationName;
            cmd.Parameters.Add(new SqlParameter("@LockId", SqlDbType.Int)).Value = lockId;

            try
            {
                conn.Open();

                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                LogAndOrThrow(e, id);
            }
            finally
            {
                conn.Close();
            }
        }

        public override void RemoveItem(HttpContext context, 
            string id, 
            object lockId, 
            SessionStateStoreData item)
        {
            var conn = new SqlConnection(ConnectionString);
            // SqlCommand to clear an existing expired session if it exists.
            var cmd = new SqlCommand($@"
DELETE FROM [{Schema}].[Sessions]
WHERE SessionId = @SessionId AND
      ApplicationName = @ApplicationName AND
      LockId = @LockId", conn);
            cmd.Parameters.Add(new SqlParameter("@SessionId", SqlDbType.NVarChar, 80)).Value = id;
            cmd.Parameters.Add(new SqlParameter("@ApplicationName", SqlDbType.NVarChar, 255)).Value = ApplicationName;
            cmd.Parameters.Add(new SqlParameter("@LockId", SqlDbType.Int)).Value = lockId;

            try
            {
                conn.Open();

                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                LogAndOrThrow(e, id);
            }
            finally
            {
                conn.Close();
            }
        }

        public override void CreateUninitializedItem(HttpContext context, string id, int timeout)
        {
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.Connection = connection;
                    command.CommandText = $@"
INSERT INTO [{Schema}].[Sessions]
           (SessionId
           ,ApplicationName
           ,Created
           ,Expires
           ,LockDate
           ,LockId
           ,Timeout
           ,Locked
           ,SessionItems
           ,Flags)
     VALUES
           (@SessionId
           ,@ApplicationName
           ,@Created
           ,@Expires
           ,@LockDate
           ,@LockId
           ,@Timeout
           ,@Locked
           ,@SessionItems
           ,@Flags)";

                    command.CommandType = CommandType.Text;
                    command.Parameters.Add(new SqlParameter("@SessionId", SqlDbType.NVarChar, 80)).Value = id;
                    command.Parameters.Add(new SqlParameter("@ApplicationName", SqlDbType.NVarChar, 255)).Value = ApplicationName;
                    command.Parameters.Add(new SqlParameter("@Created", SqlDbType.DateTime)).Value = DateTime.Now;
                    command.Parameters.Add(new SqlParameter("@Expires", SqlDbType.DateTime)).Value = DateTime.Now.AddMinutes(timeout);
                    command.Parameters.Add(new SqlParameter("@LockDate", SqlDbType.DateTime)).Value = DateTime.Now;
                    command.Parameters.Add(new SqlParameter("@LockId", SqlDbType.Int)).Value = 0;
                    command.Parameters.Add(new SqlParameter("@Timeout", SqlDbType.Int)).Value = timeout;
                    command.Parameters.Add(new SqlParameter("@Locked", SqlDbType.Bit)).Value = 0;
                    command.Parameters.Add(new SqlParameter("@SessionItems", SqlDbType.NVarChar, -1)).Value = "";
                    command.Parameters.Add(new SqlParameter("@Flags", SqlDbType.Int)).Value = 1; // InitializeItem
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                LogAndOrThrow(e, id);
            }
        }

        public override SessionStateStoreData CreateNewStoreData(HttpContext context, int timeout)
        {
            _log.Trace("CreateNewStoreData called.");
            return new SessionStateStoreData(new SessionStateItemCollection(),
                SessionStateUtility.GetSessionStaticObjects(context),
                timeout);
        }

        public override void ResetItemTimeout(HttpContext context, string id)
        {
            var conn = new SqlConnection(ConnectionString);
            // SqlCommand to clear an existing expired session if it exists.
            var cmd = new SqlCommand($@"
UPDATE [{Schema}].[Sessions]
   SET Expires = @Expires
 WHERE SessionId = @SessionId AND
       ApplicationName = @ApplicationName", conn);
            cmd.Parameters.Add(new SqlParameter("@Expires", SqlDbType.DateTime)).Value = DateTime.Now.AddMinutes(Config.Timeout.TotalMinutes);
            cmd.Parameters.Add(new SqlParameter("@SessionId", SqlDbType.NVarChar, 80)).Value = id;
            cmd.Parameters.Add(new SqlParameter("@ApplicationName", SqlDbType.NVarChar, 255)).Value = ApplicationName;

            try
            {
                conn.Open();

                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                LogAndOrThrow(e, id);
            }
            finally
            {
                conn.Close();
            }
        }

        public override void InitializeRequest(HttpContext context)
        {
        }

        public override void EndRequest(HttpContext context)
        {
        }

        #endregion

        /// <summary>
        /// Logs the reason why an operation fails and throws and exception if <see cref="ThrowOnError"/> is
        /// <c>true</c> and logging the issue as WARN.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <param name="key">The key.</param>
        /// <exception cref="CouchbaseSessionStateException"></exception>
        private void LogAndOrThrow(Exception e, string key)
        {
            _log.Error($"Could not retrieve, remove or write key '{key}' - reason: {e}");
            if (ThrowOnError)
            {
                throw new SqlSessionStateException($"Could not retrieve, remove or write key '{key}'", e);
            }
        }
    }
}