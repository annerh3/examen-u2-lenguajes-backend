namespace ProyectoExamenU2.Constants
{
    public static class TablesConstant
    {
        public const string DBO_SCHEMA = "dbo";


            public const string ACCOUNT_CATALOG = $"{DBO_SCHEMA}.account_catalog";
            public const string BALANCES = $"{DBO_SCHEMA}.balances";      //journal_entry
            public const string JOURNAL_ENTRY = $"{DBO_SCHEMA}.journal_entry"; //
            public const string JOURNAL_ENTRY_DETAIL = $"{DBO_SCHEMA}.journal_entry_detail"; //

        public const string SECURITY_SCHEMA = "security";


            public const string ROLES = $"{DBO_SCHEMA}.roles";
            public const string ROLES_CLAIMS = $"{DBO_SCHEMA}.roles_claims";
            public const string USERS = $"{DBO_SCHEMA}.users";
            public const string USERS_CLAIMS = $"{DBO_SCHEMA}.users_claims";
            public const string USERS_LOGINS = $"{DBO_SCHEMA}.users_logins";
            public const string USERS_ROLES = $"{DBO_SCHEMA}.users_roles";
            public const string USERS_TOKENS = $"{DBO_SCHEMA}.users_tokens";


    }
}
