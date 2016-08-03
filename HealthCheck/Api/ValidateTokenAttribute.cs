using StorageLayer.repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Api
{
    public class ValidateTokenAttribute : ActionFilterAttribute
    {
        //private static Logger log = new Logger(typeof(ValidateTokenAttribute));

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            string userToken = GetUserToken(actionContext.Request);
            OwnerEntity user;
            OwnerEntity organization;
            if (!Validate(userToken, out user, out organization))
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(
                    HttpStatusCode.Unauthorized,
                    String.IsNullOrEmpty(userToken) ? "Missing user token" : "Invalid user token");
                return;
            }

            actionContext.Request.Properties.Add("Owner", organization ?? user);
            actionContext.Request.Properties.Add("SignedInUser", user);
            //log.AddErrorContext("User", user.Email);
            //if (organization != null)
                //log.AddErrorContext("Organization", organization.ID);
        }

        private static string GetUserToken(HttpRequestMessage request)
        {
            IEnumerable<string> values;
            if (request.Headers.TryGetValues("x-user-token", out values))
                return values.FirstOrDefault();

            return request.GetQueryNameValuePairs().Where(
                kv => string.Equals(kv.Key, "userToken", StringComparison.OrdinalIgnoreCase))
                .Select(kv => kv.Value).FirstOrDefault();
        }

        private static bool Validate(string userToken, out OwnerEntity user,
            out OwnerEntity organization)
        {
            user = null;
            organization = null;
            if (string.IsNullOrEmpty(userToken))
                return false;

            Guid token;
            if (!Guid.TryParse(userToken, out token))
                return false;

            var tuple = Lookup(token);
            if (tuple == null)
                return false;

            user = tuple.Item1;
            organization = tuple.Item2;
            return true;
        }

        private static Tuple<OwnerEntity, OwnerEntity> Lookup(Guid token)
        {
            string key = string.Format("OwnerAndOrgForToken:{0}", token);
            var cache = MemoryCache.Default;
            object o = cache.Get(key);
            if (o != null)
                return (Tuple<OwnerEntity, OwnerEntity>)o;

            using (var repository = OwnerRepository.Get())
            {
                var user = repository.SelectByUserToken(token);
                if (user == null)
                    return null;

                OwnerEntity organization = null;
                if (user.OrganizationID != null)
                {
                    organization = repository.SelectByID(user.OrganizationID.Value);
                    if (organization == null)
                        throw new Exception(string.Format("Organization {0} not found.", user.OrganizationID));
                }
                var tuple = Tuple.Create(user, organization);
                cache.Set(key, tuple, new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTime.Now.AddMinutes(10)
                });
                return tuple;
            }
        }
    }
}