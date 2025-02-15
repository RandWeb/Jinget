﻿using Dapper;
using Jinget.Core.Contracts;
using Jinget.Core.ExpressionToSql.Internal;
using System.Collections.Generic;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Linq;
using Jinget.Core.ExtensionMethods.ExpressionToSql;
using Jinget.Core.ExtensionMethods.Collections;

namespace Jinget.Core.ExtensionMethods.Database.SqlClient
{
    public static class IDbConnectionExtensions
    {
        /// <summary>
        /// Opens a connection to database safely
        /// </summary>
        /// <remarks>This method first check if the connection state is already equal to <seealso cref="ConnectionState.Closed"/> or not, if not, 
        /// first it tries to Close the connection and then reopen it</remarks>
        public static void SafeOpen(this IDbConnection connection)
        {
            if (connection.State != ConnectionState.Closed)
                connection.Close();
            connection.Open();
        }

        private static (string queryText, Dictionary<string, object>? queryParameters) PrepareQuery(Query sql, object? param)
        {
            var query = sql.ToSql();
            var queryText = query.query.ToString();

            if (queryText.IndexOf("SELECT FROM ", StringComparison.InvariantCultureIgnoreCase) > 0)
                queryText = queryText.Replace("SELECT FROM ", "SELECT * FROM ");

            string countQueryText = "";
            if (param != null)
            {
                var orderBy = (param as IOrderBy)?.OrderBy;
                var paging = (param as IPaginated)?.PagingConfig;

                if (paging != null)
                {
                    countQueryText = queryText[queryText.IndexOf(" FROM ", StringComparison.CurrentCultureIgnoreCase)..];
                    countQueryText = "Select Count(*) " + countQueryText;

                    queryText = string.Join(" ", queryText, paging.GetPaging(orderBy));
                }
            }

            var queryParams = query.parameters;
            if (query.parameters != null && !query.parameters.Any() && param != null)
                queryParams.Merge(param.ToDictionary());

            return (countQueryText == "" ? queryText : string.Join(";", queryText, countQueryText), queryParams);
        }

        private static async Task<(IEnumerable<R> Data, int TotalCount)> ExecQueryAsync<R>(IDbConnection cnn, Query sql, object? param, IDbTransaction? transaction, int? commandTimeout, CommandType? commandType)
        {
            var (queryText, queryParameters) = PrepareQuery(sql, param);

            var result =
                await cnn.QueryMultipleAsync(queryText, queryParameters, transaction, commandTimeout,
                commandType);

            return (Data: await result.ReadAsync<R>(),
                TotalCount: param is IPaginated paginated && paginated.PagingConfig != null
                    ? (await result.ReadAsync<int>()).FirstOrDefault()
                    : 0);
        }

        private static (IEnumerable<R> Data, int TotalCount) ExecQuery<R>(
            IDbConnection cnn,
            Query sql,
            object? param,
            IDbTransaction? transaction,
            bool buffered,
            int? commandTimeout,
            CommandType? commandType)
        {
            var (queryText, queryParameters) = PrepareQuery(sql, param);

            var result = cnn.QueryMultiple(queryText, queryParameters, transaction, commandTimeout, commandType);
            return (Data: result.Read<R>(),
                TotalCount: param is IPaginated paginated && paginated.PagingConfig != null
                    ? result.Read<int>().FirstOrDefault()
                    : 0);
        }

        public static async Task<(IEnumerable<R> Data, int TotalCount)> QueryAsync<T, R>(this IDbConnection cnn, Select<T, R> sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
            => await ExecQueryAsync<R>(cnn, sql, param, transaction, commandTimeout, commandType);

        public static async Task<(IEnumerable<R> Data, int TotalCount)> QueryAsync<T, R>(this IDbConnection cnn, Where<T, R> sql, object? param = null, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
            => await ExecQueryAsync<R>(cnn, sql, param, transaction, commandTimeout, commandType);

        public static (IEnumerable<R> Data, int TotalCount) Query<T, R>(this IDbConnection cnn, Select<T, R> sql, object? param = null, IDbTransaction? transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
            => ExecQuery<R>(cnn, sql, param, transaction, buffered, commandTimeout, commandType);

        public static (IEnumerable<R> Data, int TotalCount) Query<T, R>(this IDbConnection cnn, Where<T, R> sql, object? param = null, IDbTransaction? transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
            => ExecQuery<R>(cnn, sql, param, transaction, buffered, commandTimeout, commandType);
    }
}
