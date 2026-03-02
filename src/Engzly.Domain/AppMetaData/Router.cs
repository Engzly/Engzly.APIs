namespace Engzly.Domain.AppMetaData
{
    public class Router
    {
        public const string singleRoute = "/{id}";
        // api/v1/
        public const string root = "api";
        public const string version = "V1";
        public const string rule = root + "/" + version;

        public static class UserRouting
        {
            public const string prefix = rule + "/Users";
            public const string list = prefix + "/list";
            public const string GetById = prefix + singleRoute;
            public const string Create = prefix + "/Register";
            public const string Edit = prefix + "/Edit";
            public const string Delete = prefix + singleRoute;
            public const string ChangePassword = prefix + "/ChangePassword";
        }
        
        //public static class Authentication
        //{
        //    public const string prefix = rule + "/Authentication";
        //    //public const string list = prefix + "/list";
        //    //public const string GetById = prefix + singleRoute;
        //    public const string SignIn = prefix + "/SignIn";
        //    //public const string Edite = prefix + "/Edit";
        //    //public const string Delete = prefix + singleRoute;
        //    //public const string ChangePassword = prefix + "/ChangePassword";
        //}
    }
}