using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace WhereToFly.App.Core.Services.SqliteDatabase
{
    /// <summary>
    /// Builder for SQL select queries
    /// </summary>
    public class SqlQueryBuilder
    {
        /// <summary>
        /// String builder used for building the SQL query string
        /// </summary>
        private readonly StringBuilder stringBuilder;

        /// <summary>
        /// List of bound objects
        /// </summary>
        private readonly List<object> boundObjects = new();

        /// <summary>
        /// Indicates if where clause was already added
        /// </summary>
        private bool whereClauseAdded;

        /// <summary>
        /// Array of bound objects to pass to the query
        /// </summary>
        public object[] BoundObjects => this.boundObjects.ToArray();

        /// <summary>
        /// Creates a new SQL query string for given table
        /// </summary>
        /// <param name="tableName">table name</param>
        public SqlQueryBuilder(string tableName)
        {
            this.stringBuilder = new StringBuilder($"select * from " + tableName);
            this.whereClauseAdded = false;
        }

        /// <summary>
        /// Adds a WHERE clause, or when it was already used, an AND clause
        /// </summary>
        /// <param name="whereClause">
        /// where clause expression (without the WHERE keyword)
        /// </param>
        /// <param name="objectsToBind">
        /// object or multiple objects that should be bound; for every ?
        /// placeholder there should be a bound object
        /// </param>
        public void AddWhereClause(string whereClause, params object[] objectsToBind)
        {
            Debug.Assert(
                whereClause.Count(c => c == '?') == objectsToBind.Length,
                "number of placeholder ? chars must be equal to the number of objects to bind");

            this.stringBuilder.Append(
                (!this.whereClauseAdded ? " where " : " and ") + whereClause);

            this.boundObjects.AddRange(objectsToBind);
            this.whereClauseAdded = true;
        }

        /// <summary>
        /// Builds the query string
        /// </summary>
        /// <returns>SQL query string</returns>
        public string Build() => this.stringBuilder.ToString();

        /// <summary>
        /// Returns a string for display
        /// </summary>
        /// <returns>display string</returns>
        public override string ToString() => $"SQL: {this.Build()} + {this.boundObjects.Count} bound objects";
    }
}
