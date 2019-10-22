using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Convey.WebApi
{
    public interface IEndpointsBuilder
    {
        IEndpointsBuilder Get(string path, Func<HttpContext, Task> context = null);
        IEndpointsBuilder Get<T>(string path, Func<T, HttpContext, Task> context = null) where T : class;

        IEndpointsBuilder Get<TRequest, TResult>(string path, Func<TRequest, HttpContext, Task> context = null)
            where TRequest : class;

        IEndpointsBuilder Post(string path, Func<HttpContext, Task> context = null);
        IEndpointsBuilder Post<T>(string path, Func<T, HttpContext, Task> context = null) where T : class;
        IEndpointsBuilder Put(string path, Func<HttpContext, Task> context = null);
        IEndpointsBuilder Put<T>(string path, Func<T, HttpContext, Task> context = null) where T : class;
        IEndpointsBuilder Delete(string path, Func<HttpContext, Task> context = null);
        IEndpointsBuilder Delete<T>(string path, Func<T, HttpContext, Task> context = null) where T : class;
    }
}