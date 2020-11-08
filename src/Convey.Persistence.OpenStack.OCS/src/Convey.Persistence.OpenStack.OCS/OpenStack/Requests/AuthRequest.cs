using System.Collections.Generic;

namespace Convey.Persistence.OpenStack.OCS.OpenStack.Requests
{
    internal class AuthRequest
    {
        public Auth Auth { get; set; }
    }

    internal class Auth
    {
        public Identity Identity { get; set; }
        public Scope Scope { get; set; }
    }

    internal class Identity
    {
        public List<string> Methods { get; set; }
        public Password Password { get; set; }
    }

    internal class Password
    {
        public User User { get; set; }
    }

    internal class User
    {
        public string Id { get; set; }
        public string Password { get; set; }
    }

    internal class Scope
    {
        public Project Project { get; set; }
    }

    internal class Project
    {
        public string Id { get; set; }
    }
}