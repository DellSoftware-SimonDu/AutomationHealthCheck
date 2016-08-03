using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using StorageLayer.repository;
using SpotlightApiClient;
using StorageLayer.queries.healthcheck;

namespace Api.Controllers
{
    public class HealthCheckController : ApiController
    {
        [HttpGet]
        [Route("sql-server/{connectionName}/health-check")]
        [ValidateToken]
        public dynamic GetHealthCheckForConnection(Guid diagnosticServerID, string connectionName)
        {
            var owner = GetCurrentOwner();
            var client = new SpotlightApiClientLightWeight(new Uri(Host.ApiHost), "HealthCheckApi");
            var apiConnResult = client.GetConnections(owner.UserToken, diagnosticServerID, connectionName);
            apiConnResult.HandleNonSuccess();
            var connectionList = apiConnResult.Result;

            if (connectionList.Count == 0)
                return null;

            return SqlServerStaticHealthCheckResultQueries.GetMostRecentInstanceStaticHealthCheck(owner, diagnosticServerID, connectionName, connectionList[0].DisplayName);
        }

        [HttpGet]
        [Route("sql-server/health-check")]
        [ValidateToken]
        public dynamic GetHealthCheckForOwner()
        {
            OwnerEntity owner = GetCurrentOwner();
            List<FactSqlServerInstanceStaticHealthCheckResult> summaryList = new List<FactSqlServerInstanceStaticHealthCheckResult>();
            var client = new SpotlightApiClientLightWeight(new Uri(Host.ApiHost), "HealthCheckApi");
            var apiConnResult = client.GetConnections(owner.UserToken, null, null);
            apiConnResult.HandleNonSuccess();
            var connectionList = apiConnResult.Result;
            foreach (var connection in connectionList)
            {
                var fact = SqlServerStaticHealthCheckResultQueries.GetMostRecentInstanceStaticHealthCheck(owner, connection.DiagnosticServerID, connection.ConnectionName, connection.DisplayName);

                if (fact.IsHaveStaticHealthCheckResultJson)
                {
                    fact.ObservationTime = DateTime.SpecifyKind(fact.ObservationTime.AddMinutes(owner.TimezoneOffset), DateTimeKind.Local);
                }

                summaryList.Add(fact);
            }

            return new SqlServerStaticHealthCheckResultQueries.HealthCheckSummary
            {
                OwnerID = owner.ID,
                Facts = summaryList
            };
        }

        protected OwnerEntity GetCurrentOwner()
        {
            var owner = (OwnerEntity)Request.Properties["Owner"];
            if (owner == null)
                throw new Exception("Owner property is not set.");
            return owner;
        }
    }
}