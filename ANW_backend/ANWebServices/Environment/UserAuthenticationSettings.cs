using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ANWebServices.Environment
{
    public class UserAuthenticationSettings
    {
        public static string ClaimRole
        {
            get { return "rol"; }
        }
        public static string[] AdministratorRole
        {
            get { return new string[] { "10" }; }
        }
        public static string[] ManagerRole
        {
            get { return new string[] { "9" }; }
        }
        public static string[] AuthorRole
        {
            get { return new string[] { "7","8" }; }
        }
        public static string[] VerifiedRole
        {
            get { return new string[] { "4", "5", "6" }; }
        }
        public static string[] ActivedRole
        {
            get { return new string[] { "1", "2", "3" }; }
        }
        public static string[] DefaultRole
        {
            get { return new string[] { "0" }; }
        }
    }
}
