﻿using System;
using System.Collections.Generic;
using DevExpress.Data.Entity;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Native;
using DevExpress.DataAccess.Sql;
using DevExpress.DataAccess.Web;
using DevExpress.DataAccess.Wizard.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace AspNetCore.Reporting.Common.Services.Reporting {
    public class CustomSqlDataConnectionProviderFactory : IConnectionProviderFactory {
        readonly IConnectionProviderService connectionProviderService;
        public CustomSqlDataConnectionProviderFactory(IConnectionProviderService connectionProviderService) {
            this.connectionProviderService = connectionProviderService;
        }
        public IConnectionProviderService Create() {
            return connectionProviderService;
        }
    }

    public class CustomConnectionProviderService : IConnectionProviderService {
        readonly IConfiguration configuration;
        readonly IHttpContextAccessor httpContextAccessor;
        public CustomConnectionProviderService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) {
            this.configuration = configuration;
            this.httpContextAccessor = httpContextAccessor;
        }

        public SqlDataConnection LoadConnection(string connectionName) {
            var connectionStringSection = configuration.GetSection("ReportingDataConnectionStrings");
            var connectionString = connectionStringSection?.GetValue<string>(connectionName);
            var connectionStringInfo = new ConnectionStringInfo { RunTimeConnectionString = connectionString, ProviderName = "SQLite" };
            DataConnectionParametersBase connectionParameters;
            if(string.IsNullOrEmpty(connectionString)
                || !AppConfigHelper.TryCreateSqlConnectionParameters(connectionStringInfo, out connectionParameters)
                || connectionParameters == null) {
                throw new KeyNotFoundException($"Connection string '{connectionName}' not found.");
            }
            return new SqlDataConnection(connectionName, connectionParameters);
        }

        public SqlDataConnection WrongLoadConnection(string connectionName) {
            var connectionString = configuration.GetSection("ReportingDataConnectionStrings")?.GetValue<string>(connectionName);
            if(string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("There is no connection with name: " + connectionName);
            return new SqlDataConnection {
                Name = connectionName,
                ConnectionString = connectionString,
                StoreConnectionNameOnly = true,
                ConnectionStringSerializable = connectionString,
                ProviderKey = "SQLite"
            };
        }
    }
}
