using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Project.Application.Services;

namespace Project.RestApi.Services
{
    public class UserDescriptor : IUserDescriptor
    {
        private readonly HttpContext httpContext;

        public UserDescriptor(IHttpContextAccessor httpContextAccessor)
        {
            httpContext = httpContextAccessor.HttpContext;
        }

        public Guid GetId()
        {
            var claim = httpContext?.User.FindFirst(ClaimTypes.NameIdentifier);

            return claim is null ? Guid.Empty : Guid.Parse(claim.Value);
        }

        public string GetClient()
        {
            var userAgent = httpContext?.Request.Headers["User-Agent"];

            return userAgent is null ? "Project" : userAgent.ToString()!;
        }

        public string GetClientAddress()
        {
            var remoteIpAddress = httpContext?.Connection.RemoteIpAddress;

            return remoteIpAddress is null ? "Project" : remoteIpAddress.MapToIPv4().ToString();
        }
    }
}