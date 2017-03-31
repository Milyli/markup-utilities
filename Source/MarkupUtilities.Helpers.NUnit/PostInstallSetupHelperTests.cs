using System.Data;
using System.Threading.Tasks;
using MarkupUtilities.Helpers.Rsapi;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace MarkupUtilities.Helpers.NUnit
{
  public class PostInstallSetupHelperTests
  {
    private IPostInstallSetupHelper _postInstallSetupHelper;
    private Mock<IQuery> _mockQuery;
    private Mock<ArtifactQueries> _mockArtifactQueries;
    private Mock<IServicesMgr> _mockServicesMgr;
    private Mock<IDBContext> _mockWorkspaceDbContext;

    [SetUp]
    public void Setup()
    {
      _mockQuery = new Mock<IQuery>();
      _mockArtifactQueries = new Mock<ArtifactQueries>();
      _mockServicesMgr = new Mock<IServicesMgr>();
      _mockWorkspaceDbContext = new Mock<IDBContext>();
    }

    [TearDown]
    public void Teardown()
    {
      _postInstallSetupHelper = null;
      _mockQuery = null;
      _mockArtifactQueries = null;
      _mockServicesMgr = null;
      _mockWorkspaceDbContext = null;

    }

    [Test]
    public async Task CreateRecordsForMarkupUtilityTypeRdoTests()
    {
      //Arrange
      _postInstallSetupHelper = new PostInstallSetupHelper(_mockQuery.Object, _mockArtifactQueries.Object);
      _mockQuery
        .Setup(x => x.RetrieveMarkupTypesAsync(_mockWorkspaceDbContext.Object))
        .Returns(Task.FromResult(GetMarkupTypeDataTable()));

      _mockQuery
        .Setup(x => x.RetrieveMarkupSubTypesAsync(_mockWorkspaceDbContext.Object))
        .Returns(Task.FromResult(new DataTable()));

      //Act

      //Assert
      await _postInstallSetupHelper.CreateRecordsForMarkupUtilityTypeRdoAsync(_mockServicesMgr.Object, ExecutionIdentity.System, 123, _mockWorkspaceDbContext.Object);
    }

    private static DataTable GetMarkupTypeDataTable()
    {
      var dataTable = new DataTable(Constant.Sql.WorkspaceTables.RedactionMarkupType.NAME);
      dataTable.Columns.Add(Constant.Sql.WorkspaceTables.RedactionMarkupType.Columns.ID);
      dataTable.Columns.Add(Constant.Sql.WorkspaceTables.RedactionMarkupType.Columns.TYPE);
      dataTable.Rows.Add(1, "Redaction");
      dataTable.Rows.Add(2, "Highlight");

      return dataTable;
    }
  }
}
