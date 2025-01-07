namespace Discussly.Server.DTO.Users
{
    public static class Role
    {
        public static readonly string User = nameof(User);

        public static readonly string Anonym = nameof(Anonym);

        public static int DefineRoleId(IList<string> roles)
        {
            if (roles.Contains(Anonym))
                return 2;

            return 1;
        }
    }
}