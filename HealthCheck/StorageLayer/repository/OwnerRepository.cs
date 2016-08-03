using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageLayer.repository
{
    public class OwnerRepository : AbstractRepository<OwnerEntity>
    {
        private static Dictionary<long, Guid> IDs = new Dictionary<long, Guid>();
        private IDbTransaction _transaction;
        public long ID { get; set; }
        public Guid UserId { get; set; }
        public int TimezoneOffset { get; set; }
        public string TimezoneOffsetDesc { get; set; }
        public Guid UserToken { get; set; }
        public long? OrganizationID { get; set; }
        private const string BASIC_SELECT_STATEMENT =
            @"select id, 
                user_id,
                timezoneOffset,
                timezoneOffsetDesc,
                userToken,
                organizationID
              from operational_owners ";

        public OwnerRepository(string repository) :
            base(repository)
        {
        }

        public static string TableName
        {
            get
            {
                return "operational_owners";
            }
        }

        public override string GetTableName()
        {
            return TableName;
        }

        public static Guid? OwnerIDToUserID(long ownerID)
        {
            if (IDs.ContainsKey(ownerID))
                return IDs[ownerID];

            using (var ownerRepository = Get())
            {
                var e = ownerRepository.SelectByID(ownerID);
                if (e == null)
                    return null;

                IDs[e.ID] = e.UserId;
                return e.UserId;
            }
        }

        public static OwnerRepository Get()
        {
            return new OwnerRepository(Constants.CONNECTION_STRING_OPERATIONS);
        }

        public OwnerEntity SelectByEmail(string email)
        {
            return SelectByEmail("Lucy", email);
        }

        private static string _impersonateEmail = "";   //<- Keep this line - it allows you to impersonate another use, when debugging locally, using Test|Prod databases
        public OwnerEntity SelectByEmail(string applicationName, string email)
        {
            var cmd = GetDefaultCommand();

            if (!string.IsNullOrWhiteSpace(_impersonateEmail))
                email = _impersonateEmail;

            cmd.CommandText = BASIC_SELECT_STATEMENT + " where application_name = @application_name and email = @email";

            cmd.Parameters.Add(new SqlParameter("@application_name", applicationName));
            cmd.Parameters.Add(new SqlParameter("@email", email));

            return ExecuteSingletonSelectWithRetries(cmd);
        }

        public OwnerEntity SelectByUserToken(Guid userToken)
        {
            var cmd = GetDefaultCommand();

            cmd.CommandText = BASIC_SELECT_STATEMENT + " where user_token = @user_token";
            cmd.Parameters.Add(new SqlParameter("@user_token", userToken));

            return ExecuteSingletonSelectWithRetries(cmd);
        }

        public OwnerEntity SelectByID(long id)
        {
            var cmd = GetDefaultCommand();

            cmd.CommandText = BASIC_SELECT_STATEMENT + " where id = @id";

            cmd.SetParameter("@id", id);

            return ExecuteSingletonSelectWithRetries(cmd);
        }
    }

    public class OwnerEntity : AbstractRepositoryEntity
    {
        public long ID { get; set; }
        public Guid UserId { get; set; }
        public int TimezoneOffset { get; set; }
        public string TimezoneOffsetDesc { get; set; }
        public Guid UserToken { get; set; }
        public long? OrganizationID { get; set; }

        public override void InitializeFromRow(System.Data.IDataReader aReader)
        {
            ID = (long)aReader["id"];
            UserId = (Guid)aReader["user_id"];
            TimezoneOffset = (int)aReader["timezone_offset"];
            TimezoneOffsetDesc = (string)aReader["timezone_offset_desc"];
            UserToken = (Guid)aReader["user_token"];
            OrganizationID = aReader.GetNullable<long>("organization_id");
        }
    }
}
