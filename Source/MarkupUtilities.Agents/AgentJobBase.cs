using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarkupUtilities.Helpers;
using Relativity.API;

namespace MarkupUtilities.Agents
{
  public abstract class AgentJobBase
  {
    public int AgentId { get; set; }
    public IAgentHelper AgentHelper { get; set; }
    public IQuery QueryHelper { get; set; }
    public string QueueTable { get; set; }
    public int WorkspaceArtifactId { get; set; }
    public int RecordId { get; set; }
    public int Priority { get; set; }
    public string OffHoursStartTime { get; set; }
    public string OffHoursEndTime { get; set; }
    public DateTime ProcessedOnDateTime { get; set; }
    public IEnumerable<int> AgentResourceGroupIds { get; set; }
    public delegate void RaiseMessageEventHandler(object sender, string message);
    public event RaiseMessageEventHandler OnMessage;

    public virtual async Task ResetUnfishedJobsAsync(IDBContext eddsDbContext)
    {
      await QueryHelper.ResetUnfishedJobsAsync(eddsDbContext, AgentId, QueueTable);
    }

    protected virtual void RaiseMessage(string message)
    {
      var handler = OnMessage;
      handler?.Invoke(this, message);
    }

    private async Task GetOffHoursTimesAsync()
    {
      var dt = await QueryHelper.RetrieveOffHoursAsync(AgentHelper.GetDBContext(-1));
      if (dt?.Rows == null || dt.Rows.Count == 0 || string.IsNullOrEmpty(dt.Rows[0]["AgentOffHourStartTime"].ToString()) || string.IsNullOrEmpty(dt.Rows[0]["AgentOffHourEndTime"].ToString()) || dt.Rows[0]["AgentOffHourStartTime"] == null || dt.Rows[0]["AgentOffHourEndTime"] == null)
      {
        throw new Helpers.Exceptions.MarkupUtilityException(Constant.Messages.AGENT_OFF_HOURS_NOT_FOUND);
      }
      OffHoursStartTime = dt.Rows[0]["AgentOffHourStartTime"].ToString();
      OffHoursEndTime = dt.Rows[0]["AgentOffHourEndTime"].ToString();
    }

    public async Task<bool> IsOffHoursAsync(DateTime? currentTime = null)
    {
      var now = currentTime.GetValueOrDefault(DateTime.Now);
      var isOffHours = false;

      try
      {
        await GetOffHoursTimesAsync();
        var todayOffHourStart = DateTime.Parse(now.ToString("d") + " " + OffHoursStartTime);
        var todayOffHourEnd = DateTime.Parse(now.ToString("d") + " " + OffHoursEndTime);

        if (now.Ticks >= todayOffHourStart.Ticks && now.Ticks <= todayOffHourEnd.Ticks)
        {
          isOffHours = true;
        }
      }
      catch (FormatException)
      {
        RaiseMessage(Constant.Messages.AGENT_OFF_HOUR_TIMEFORMAT_INCORRECT);
        throw;
      }

      return isOffHours;
    }

    public string GetCommaDelimitedListOfResourceIds(IEnumerable<int> agentResourceGroupIds)
    {
      return string.Join(",", agentResourceGroupIds);
    }

    public abstract Task ExecuteAsync();
  }
}
