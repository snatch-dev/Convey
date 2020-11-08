using System.Collections.Generic;
using Convey.Persistence.OpenStack.OCS.OpenStack.Requests;

namespace Convey.Persistence.OpenStack.OCS.Auth
{
    internal class AuthRequestBuilder : IAuthRequestBuilder
    {
        private readonly AuthRequest _authRequest;

        public AuthRequestBuilder()
        {
            _authRequest = new AuthRequest
            {
                Auth = new OpenStack.Requests.Auth
                {
                    Identity = new Identity
                    {
                        Methods = new List<string>(),
                        Password = new Password
                        {
                            User = new User()
                        }
                    },
                    Scope = new Scope
                    {
                        Project = new Project()
                    }
                }
            };
        }

        public IAuthRequestBuilder WithMethod(string method)
        {
            _authRequest.Auth.Identity.Methods.Add(method);
            return this;
        }

        public IAuthRequestBuilder WithMethods(IEnumerable<string> methods)
        {
            _authRequest.Auth.Identity.Methods.AddRange(methods);
            return this;
        }

        public IAuthRequestBuilder WithProject(string projectId)
        {
            _authRequest.Auth.Scope.Project.Id = projectId;
            return this;
        }

        public IAuthRequestBuilder WithUser(string id, string password)
        {
            _authRequest.Auth.Identity.Password.User.Id = id;
            _authRequest.Auth.Identity.Password.User.Password = password;
            return this;
        }

        public AuthRequest Build() => _authRequest;
    }
}