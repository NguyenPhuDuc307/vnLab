namespace vnLab.Helpers
{
    public static class Constants
    {
        public static class Roles
        {
            public const string Administrator = "Admin";
            public const string Member = "Member";
            public const string User = "User";
        }

        public static class Policies
        {
            public const string RequireAdmin = "RequireAdmin";
            public const string RequireMember = "RequireMember";
        }
    }
}