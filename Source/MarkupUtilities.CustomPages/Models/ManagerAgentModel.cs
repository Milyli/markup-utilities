using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MarkupUtilities.Helpers;
using Relativity.API;

namespace MarkupUtilities.CustomPages.Models
{
  public class ManagerAgentModel
  {
    public List<ManagerQueueRecordModel> Records { get; set; }
    public IQuery QueryHelper;

    public ManagerAgentModel(IQuery queryModel)
    {
      QueryHelper = queryModel;
      Records = new List<ManagerQueueRecordModel>();
    }

    public ManagerAgentModel()
    {
      QueryHelper = new Query();
      Records = new List<ManagerQueueRecordModel>();
    }

    public async Task GetAllAsync(IDBContext eddsDbContext)
    {
      var dt = await QueryHelper.RetrieveAllInExportManagerQueueAsync(eddsDbContext);

      foreach (DataRow thisRow in dt.Rows)
      {
        Records.Add(new ManagerQueueRecordModel(thisRow, QueryHelper));
      }
    }
  }
}
