using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using kCura.Relativity.Client.Repositories;

namespace MarkupUtilities.Helpers.Utility
{
  public class QuerySubset
  {
    /// <summary>
    /// Publically accessible method that uses the yield keyword so that not only queries are
    /// batched but also batch the returned results to the caller
    /// </summary>
    /// <typeparam name="T"> A DTO with Artifact as the base </typeparam>
    /// <param name="repository"> The repository of the DTO you'd like to query </param>
    /// <param name="originalQuery">
    /// A preconstructed Query that would usually be sent to the DTO repository
    /// </param>
    /// <param name="batchSize">
    /// The maximum batch size of the full artifacts(with fields) requested from the server
    /// </param>
    /// <returns> IEnumerable list or RDOs </returns>
    public static IEnumerable<Result<T>> PerformQuerySubset<T>(IGenericRepository<T> repository, Query<T> originalQuery, int batchSize) where T : kCura.Relativity.Client.DTOs.Artifact, new()
    {
      var artifactIds = GetQueryArtifactIds(repository, originalQuery, batchSize).ToList();
      var currentBatchIds = new List<int>();
      var totalCount = 0;
      foreach (var id in artifactIds)
      {
        currentBatchIds.Add(id);
        totalCount++;

        if (totalCount % batchSize != 0 && totalCount != artifactIds.Count()) continue;

        var currentBatchArtifacts = RetrieveBatch(repository, originalQuery, currentBatchIds);

        foreach (var individualResult in currentBatchArtifacts.Results)
        {
          yield return individualResult;
        }

        currentBatchIds.Clear();
      }
    }

    /// <summary>
    /// requests full artifacts(with selected fields) from the server 
    /// </summary>
    /// <typeparam name="T"> A DTO with Artifact as the base </typeparam>
    /// <param name="repository"> The RSAPI Client </param>
    /// <param name="originalQuery"> The developer's original query </param>
    /// <param name="artifactIdBatch"> The artifactIds of the DTOs to return </param>
    /// <returns> Query Result Set of type RDO </returns>
    private static QueryResultSet<T> RetrieveBatch<T>(IGenericRepository<T> repository, Query<T> originalQuery, IEnumerable<int> artifactIdBatch) where T : kCura.Relativity.Client.DTOs.Artifact, new()
    {
      var batchQuery = new Query<T>
      {
        ArtifactTypeGuid = originalQuery.ArtifactTypeGuid,
        ArtifactTypeID = originalQuery.ArtifactTypeID,
        ArtifactTypeName = originalQuery.ArtifactTypeName,
        Fields = originalQuery.Fields,
        Condition = new WholeNumberCondition(ArtifactQueryFieldNames.ArtifactID, NumericConditionEnum.In, artifactIdBatch.ToList()),
        Sorts = originalQuery.Sorts,
        RelationalField = originalQuery.RelationalField
      };

      var results = QueryRelativity(repository, batchQuery, artifactIdBatch.Count());

      return results;
    }

    /// <summary>
    /// All queries are completed with QuerySubset whether the DTO supports it or not so that
    /// this utitlity method can be used with all DTOs
    /// </summary>
    /// <typeparam name="T"> DTO </typeparam>
    /// <param name="repository"> RSASPI Client Repository </param>
    /// <param name="query"> Query to Execute </param>
    /// <param name="batchSize"> maximum number of items to return in a single call </param>
    /// <returns> traditional Query Result Set </returns>
    private static QueryResultSet<T> QueryRelativity<T>(IGenericRepository<T> repository, Query<T> query, int batchSize) where T : kCura.Relativity.Client.DTOs.Artifact, new()
    {
      var nextStart = 1;
      var retValue = new QueryResultSet<T>();
      var queryResults = repository.Query(query, batchSize);

      CumulateQueryResults(queryResults, retValue);

      if (queryResults.Success && queryResults.Results.Count > 0)
      {
        var queryToken = queryResults.QueryToken;
        var batchAvailable = !string.IsNullOrEmpty(queryToken);

        while (batchAvailable)
        {
          nextStart += queryResults.Results.Count;
          queryResults = repository.QuerySubset(queryToken, nextStart, batchSize);
          CumulateQueryResults(queryResults, retValue);
          queryToken = queryResults.QueryToken;
          batchAvailable = !string.IsNullOrEmpty(queryToken);
        }
      }

      if (queryResults.Success == false)
      {
        throw new Exception("Unable to complete Query: " + queryResults.Message);
      }

      return retValue;
    }

    /// <summary>
    /// Combines the multiple query result sets into each other. 
    /// </summary>
    /// <typeparam name="T"> DTO type </typeparam>
    /// <param name="newAddition"> recent query result </param>
    /// <param name="cumulativeResults"> Query results where values will cumulate </param>
    private static void CumulateQueryResults<T>(QueryResultSet<T> newAddition, QueryResultSet<T> cumulativeResults) where T : kCura.Relativity.Client.DTOs.Artifact, new()
    {
      cumulativeResults.Results.AddRange(newAddition.Results);
      cumulativeResults.Success = newAddition.Success;
      cumulativeResults.Message += newAddition.Message;
    }

    /// <summary>
    /// Executes the developer's original query with no fields and only returns the artifactIds
    /// of the results in order to get the results quickly
    /// </summary>
    /// <typeparam name="T"> A DTO with Artifact as the base </typeparam>
    /// <param name="repository"> The RSAPI Client </param>
    /// <param name="originalQuery"> The developer's original query </param>
    /// <param name="batchSize"> Size of the batch</param>
    /// <returns> An IEnumerable list of results represented solely by their ArtifactId </returns>
    private static IEnumerable<int> GetQueryArtifactIds<T>(IGenericRepository<T> repository, Query<T> originalQuery, int batchSize) where T : kCura.Relativity.Client.DTOs.Artifact, new()
    {
      var retVal = new List<int>();
      var artifactIdQuery = new Query<T>
      {
        ArtifactTypeGuid = originalQuery.ArtifactTypeGuid,
        ArtifactTypeID = originalQuery.ArtifactTypeID,
        ArtifactTypeName = originalQuery.ArtifactTypeName,
        Fields = FieldValue.NoFields,
        Condition = originalQuery.Condition,
        Sorts = originalQuery.Sorts,
        RelationalField = originalQuery.RelationalField
      };

      var results = QueryRelativity(repository, artifactIdQuery, batchSize);

      retVal.AddRange(results.Results.Select(x => x.Artifact.ArtifactID).ToList());

      return retVal;
    }
  }
}