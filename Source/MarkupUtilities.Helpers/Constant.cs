using System;
using System.Collections.Generic;
using MarkupUtilities.Helpers.Models;

namespace MarkupUtilities.Helpers
{
  public class Constant
  {
    public class Tables
    {
      public static readonly string ExportManagerQueue = "MarkupUtility_ExportManagerQueue";
      public static readonly string ExportWorkerQueue = "MarkupUtility_ExportWorkerQueue";
      public static readonly string ReproduceManagerQueue = "MarkupUtility_ReproduceManagerQueue";
      public static readonly string ReproduceWorkerQueue = "MarkupUtility_ReproduceWorkerQueue";
      public static readonly string ImportManagerQueue = "MarkupUtility_ImportManagerQueue";
      public static readonly string ImportWorkerQueue = "MarkupUtility_ImportWorkerQueue";
      public static readonly string ExportErrorLog = "MarkupUtility_ExportErrorLog";
      public static readonly string ImportErrorLog = "MarkupUtility_ImportErrorLog";
      public static readonly string ReproduceErrorLog = "MarkupUtility_ReproduceErrorLog";
      public static readonly string ExportResults = "MarkupUtility_ExportResults";
      public static readonly string ImportJob = "MarkupUtilityImportJob";
      public static readonly string ExportJob = "MarkupUtilityExportJob";
      public static readonly string ReproduceJob = "MarkupUtilityReproduceJob";
    }

    public class Names
    {
      public static readonly string ApplicationName = "Markup Utilities";
      public static readonly string TablePrefix = "MarkupUtility_";
      public static readonly string ImportManagerHoldingTablePrefix = $"{TablePrefix}ImportManagerHoldingTable_";
      public static readonly string ExportManagerHoldingTablePrefix = $"{TablePrefix}ExportManagerHoldingTable_";
      public static readonly string ExportWorkerHoldingTablePrefix = $"{TablePrefix}ExportWorkerHoldingTable_";
      public static readonly string ReproduceWorkerHoldingTablePrefix = $"{TablePrefix}ReproduceWorkerHoldingTable_";

      public class Rdos
      {
        public const string MARKUP_UTILITY_EXPORT_JOB = "Markup Utility Export Job";
        public const string MARKUP_UTILITY_IMPORT_JOB = "Markup Utility Import Job";
        public const string MARKUP_UTILITY_REPRODUCE_JOB = "Markup Utility Reproduce Job";
        public const string MARKUP_UTILITY_FILE = "Markup Utility File";
        public const string MARKUP_UTILITY_HISTORY = "Markup Utility History";
        public const string MARKUP_UTILITY_TYPE = "Markup Utility Type";
      }
    }

    public class Guids
    {
      public class Application
      {
        public static readonly Guid ApplicationGuid = new Guid("F4489131-64B8-4E05-8B14-19B866182CC5");
      }

      public class Tabs
      {
        public static readonly Guid MarkupUtilityExportJob = new Guid("B16FFDA6-AA84-4EC5-A851-BD3E9ED3B0F7");
        public static readonly Guid MarkupUtilityImportJob = new Guid("012D36DF-C618-43E6-A321-1BD7085F3296");
        public static readonly Guid MarkupUtilityReproduceJob = new Guid("0AB08EB0-35BE-4149-AF04-2618E7E5AEE8");
        public static readonly Guid MarkupUtilityFile = new Guid("6AA772C6-4349-4723-8980-1BE532719292");
        public static readonly Guid MarkupUtilityHistory = new Guid("6E1FFFB1-3B03-416D-942E-AA01209FBD54");
        public static readonly Guid ExportManagerQueueTab = new Guid("");
        public static readonly Guid ImportManagerQueueTab = new Guid("");
        public static readonly Guid ExportWorkerQueueTab = new Guid("");
        public static readonly Guid ImportWorkerQueueTab = new Guid("");
      }

      public class ObjectType
      {
        public static readonly Guid MarkupUtilityExportJob = new Guid("40354725-BF20-4938-B12E-32EE03975910");
        public static readonly Guid MarkupUtilityImportJob = new Guid("007F09CC-6993-4ABD-9637-4BD6B389222B");
        public static readonly Guid MarkupUtilityReproduceJob = new Guid("C844709A-8FDC-47BF-BCE4-A7FF5362A10B");
        public static readonly Guid MarkupUtilityFile = new Guid("51688890-DB60-4E25-8E51-7F8F71EE3717");
        public static readonly Guid MarkupUtilityHistory = new Guid("3A928359-B8EA-4D5E-8309-B765F5AB0863");
        public static readonly Guid MarkupUtilityType = new Guid("E058A237-83E8-44F8-BACD-F8F4E04AE80C");
      }

      public class Field
      {
        public class MarkupUtilityExportJob
        {
          public static readonly Guid SystemCreatedOn = new Guid("4A9DDF34-4B78-4F98-BC2F-4E054807ABA3");
          public static readonly Guid SystemLastModifiedOn = new Guid("22F9152C-0144-428F-A025-9D6DD7BC819C");
          public static readonly Guid SystemCreatedBy = new Guid("F55BD55F-D7E5-486B-A77A-08090E35CE26");
          public static readonly Guid SystemLastModifiedBy = new Guid("5C14A2C0-31C5-4BD3-9CD4-831678C7D52D");
          public static readonly Guid ArtifactId = new Guid("C8CA662F-3CEA-4EFF-A326-594F69B55E62");
          public static readonly Guid Name = new Guid("D873AA29-B34C-48CF-B5D0-18470421A855");
          public static readonly Guid SavedSearch = new Guid("2E13C08A-59EE-4B9C-A162-B8EA700DCBE5");
          public static readonly Guid MarkupSet = new Guid("51D12145-95B2-46B1-9924-148F2E70A4FE");
          public static readonly Guid Status = new Guid("8B03B031-15F3-46C6-A0B3-9F656F68CF68");
          public static readonly Guid ExportedRedactionCount = new Guid("762B080B-E43E-4B9A-B2D4-712C86186758");
          public static readonly Guid RedactionFile = new Guid("929DE13C-840A-4608-9222-285E461EFB89");
          public static readonly Guid Details = new Guid("7E802495-17C6-4159-B35E-AE682375BAF5");
          public static readonly Guid ExportRedactionType = new Guid("1F6B9B84-254D-45D7-A24A-9C38C8C9A041");
        }

        public class MarkupUtilityImportJob
        {
          public static readonly Guid SystemCreatedOn = new Guid("592F874D-68B3-4054-BE2E-3FA2DBBBE123");
          public static readonly Guid SystemLastModifiedOn = new Guid("FE6C91BB-A7E8-4E4B-BAFF-A0669AC0DCA4");
          public static readonly Guid SystemCreatedBy = new Guid("4F5EE08F-5A86-4C2B-BAF8-B417F2AA3BC6");
          public static readonly Guid SystemLastModifiedBy = new Guid("9037AC48-8060-46F2-9E4F-1D5285281E12");
          public static readonly Guid ArtifactID = new Guid("7BF49C2D-7EC0-41AA-B553-A863ACD14DB9");
          public static readonly Guid Name = new Guid("404453EA-6A3C-4DDB-85A0-37E691CF3AAC");
          public static readonly Guid MarkupSet = new Guid("1AD9803F-02E9-4694-AF6B-7BDB0325FD08");
          public static readonly Guid SkipDuplicateRedactions = new Guid("C4E9E116-97B8-4143-A877-802E63A25B76");
          public static readonly Guid Status = new Guid("53B09591-4C2B-4187-B352-5411247E5619");
          public static readonly Guid Details = new Guid("1155033B-63C3-46B2-8148-197B915F3118");
          public static readonly Guid RedactionFile = new Guid("3B397ABC-5213-4B2D-9FC4-22587803860C");
          public static readonly Guid ImportRedactionType = new Guid("D5B07458-E92D-41E6-A27E-AC427755E482");
          public static readonly Guid JobType = new Guid("854908F4-92CB-430C-8B93-F4D95887CE66");
          public static readonly Guid ImportFileRedactionCount = new Guid("972C998A-8018-4AA7-AE4B-6EED62F936D7");
          public static readonly Guid ExpectedRedactionCount = new Guid("C19ACEF7-87F5-4972-AC26-07704B54C716");
          public static readonly Guid ImportedRedactionCount = new Guid("46103C6D-3DFF-49F7-8078-541E2E3826BF");
          public static readonly Guid SkippedRedactionCount = new Guid("038FE5E5-495D-4C7D-A577-9EBA261F5342");
          public static readonly Guid ErrorRedactionCount = new Guid("BBAA0D49-C8F4-444E-83CE-82184EE625EA");
        }

        public class MarkupUtilityReproduceJob
        {
          public static readonly Guid SystemCreatedOn = new Guid("05C3CAF7-64B2-4CA5-BB80-E7EAD5DC9E64");
          public static readonly Guid SystemLastModifiedOn = new Guid("02120E23-F499-47D4-8001-D1D512A70026");
          public static readonly Guid SystemCreatedBy = new Guid("D173E424-6167-4118-A345-A5C7C381CE17");
          public static readonly Guid SystemLastModifiedBy = new Guid("3FC85D4E-F11E-4465-A638-6E2C4FD8D000");
          public static readonly Guid ArtifactID = new Guid("7D6627DD-1B04-486A-A754-E5B4FB555479");
          public static readonly Guid Name = new Guid("9D5D7871-6E26-41F1-B71A-AC811B2882D4");
          public static readonly Guid SourceMarkupSet = new Guid("65242529-6E41-40AA-A06C-CBEB330FE61A");
          public static readonly Guid DestinationMarkupSet = new Guid("446A3659-BF25-474D-8C4D-286405EFF91C");
          public static readonly Guid SavedSearch = new Guid("CD694438-3517-4348-B483-83C25FA036A9");
          public static readonly Guid Status = new Guid("EDA76A8A-DE15-43D2-9DE1-5EA8A59AF1CA");
          public static readonly Guid Details = new Guid("4822536D-FFF5-4594-8D8D-A82A8BF1DA04");
          public static readonly Guid ReproduceJobType = new Guid("0149C18F-8C66-4D31-8EC7-FF639D455E2F");
          public static readonly Guid HasAutoRedactionsField = new Guid("3C3B7308-34BE-4883-A5C9-98EE2A5D7766");
          public static readonly Guid RelationalField = new Guid("09CE1019-FAAB-4045-8380-0492E9407838");
        }

        public class MarkupUtilityFile
        {
          public static readonly Guid SystemCreatedOn = new Guid("8D596096-109F-4366-A371-0BEA200ECDFF");
          public static readonly Guid SystemLastModifiedOn = new Guid("9178C6B2-8E93-44D4-B137-D53D3D2AD17F");
          public static readonly Guid SystemCreatedBy = new Guid("1C8E157F-B71F-4E77-9B83-8B460EB54FF0");
          public static readonly Guid SystemLastModifiedBy = new Guid("3D69D3DF-ACFD-4781-9B09-F503C24E98F2");
          public static readonly Guid ArtifactID = new Guid("D73F7EE0-0CC6-4DB1-85FF-94C0037190A9");
          public static readonly Guid Name = new Guid("B8D03028-0B1E-413F-8963-383D86DD0009");
          public static readonly Guid File = new Guid("A706F3C4-2932-4EA0-9E19-DEE1753682BB");
          public static readonly Guid FileFileIcon = new Guid("FD9E54A5-C9D3-4395-B3A3-99E42BA4138A");
          public static readonly Guid FileFileSize = new Guid("2B0E1A45-AAF7-46F2-B501-F0493D6BA2CA");
          public static readonly Guid FileText = new Guid("57F694F0-8012-433A-AB63-72D913424413");

        }

        public class MarkupUtilityHistory
        {
          public static readonly Guid SystemCreatedOn = new Guid("7235C9E8-C14E-4577-A4C6-FC436F99D055");
          public static readonly Guid SystemLastModifiedOn = new Guid("17E0F8CE-0C23-4DDD-93DF-07B6C967CE62");
          public static readonly Guid SystemCreatedBy = new Guid("18A8EF6B-E4AB-43B7-ABE4-42929A24D531");
          public static readonly Guid SystemLastModifiedBy = new Guid("93B7734B-510D-4A00-A0E5-52D56AA5E990");
          public static readonly Guid ArtifactId = new Guid("16463D77-E253-4314-9A6B-5256E449432A");
          public static readonly Guid Name = new Guid("410D7A6F-525A-4359-A377-B5324BEE086F");
          public static readonly Guid Status = new Guid("20A5D6ED-4E33-4F78-BEAC-9303F6DF3CC1");
          public static readonly Guid Details = new Guid("926BE935-0242-473D-B1E8-797C6451F177");
          public static readonly Guid DocumentIdentifier = new Guid("73F9D1F8-BC92-40B4-83DF-5D0E169A5568");
          public static readonly Guid PageNumber = new Guid("191A394B-5F97-4B6C-B842-41CC25CBB606");
          public static readonly Guid RedactionData = new Guid("2E44AE0C-4C54-4BCB-B4C6-0C3858828600");
          public static readonly Guid JobType = new Guid("AF85DE22-263A-4BFC-AF6B-15D4340C9040");
          public static readonly Guid RedactionType = new Guid("A6E7833C-C311-401E-8DB4-B0C280170256");
          public static readonly Guid RedactionId = new Guid("F2874821-D73F-4C44-9E3A-0BED445BE99A");
          public static readonly Guid ImportJob = new Guid("53F1A4DA-164F-4509-9E48-8E6A9CEE373F");
          public static readonly Guid ReproduceJob = new Guid("8B49D82F-861F-434C-88A1-D44C8B1CF7F2");
        }

        public class MarkupUtilityType
        {
          public static readonly Guid Name = new Guid("94DE63EF-2A7C-4A90-9605-6894A9D87B57");
          public static readonly Guid Category = new Guid("E6AA2DF2-96EC-48A0-95BE-0F1280F58740");
        }
      }

      public class Choices
      {
        public class RedactionType
        {
          public static readonly Guid Import = new Guid("8891C74F-2AC0-4FA4-B61E-67611C521C44");
          public static readonly Guid Removal = new Guid("FC5B2985-7805-4425-B994-52455FE464F0");
        }

        public class MarkupUtilityType
        {
          public class Category
          {
            public static readonly Guid Redaction = new Guid("DBBCC828-F83D-4A20-A202-27F1052D3074");
            public static readonly Guid Highlight = new Guid("3B918333-660B-4244-9D77-37BA80320F8D");
          }
        }

        public class ImportJobType
        {
          public static readonly Guid Import = new Guid("40CF47D8-F853-4C21-B1AB-4B21531E1E29");
          public static readonly Guid Validate = new Guid("5ABB84D6-3368-4648-818C-9E66B6789B9D");
          public static readonly Guid Revert = new Guid("83F96954-FE1A-4628-A521-6C274543AC84");
        }

        public class ReproduceJobType
        {
          public static readonly Guid AcrossDocumentSet = new Guid("292E5C85-4647-4F11-884D-F2167899EB12");
          public static readonly Guid AcrossRelationalGroup = new Guid("F6735BB6-38DF-4604-9BE9-867A663C89AA");
        }
      }
    }

    public class Sizes
    {
      public static readonly int ReproduceJobBatchSize = 1000;
      public static readonly int ReproduceJobInsertBatchSize = 100;
      public static readonly int ImportJobBatchSize = 10;
      public static readonly int ExportJobManagerBatchSize = 10;
      public static readonly int ExportJobWorkerBatchSize = 100;
      public static readonly int ExportJobHoldingTableBatchSize = 1000;
      public static readonly int ExportJobResultsBatchSize = 500;
    }

    public class Status
    {
      public class Queue
      {
        public static readonly int NotStarted = 0;
        public static readonly int InProgress = 1;
        public static readonly int Error = -1;
      }

      public class Job
      {
        public const string NEW = "New";
        public const string VALIDATING = "Validating";
        public const string VALIDATED = "Validated";
        public const string VALIDATION_FAILED = "Validation failed";
        public const string SUBMITTED = "Submitted";
        public const string IN_PROGRESS_MANAGER = "In Progress - Manager";
        public const string IN_PROGRESS_WORKER = "In Progress - Worker";
        public const string COMPLETED = "Completed";
        public const string COMPLETED_WITH_ERRORS = "Completed With Errors";
        public const string COMPLETED_WITH_SKIPPED_DOCUMENTS = "Completed With Skipped Documents";
        public const string COMPLETED_WITH_ERRORS_AND_SKIPPED_DOCUMENTS = "Completed With Errors and Skipped Documents";
        public const string COMPLETED_MANAGER = "Completed - Manager";
        public const string COMPLETED_WORKER = "Completed - Worker";
        public const string CANCELREQUESTED = "Cancel Requested";
        public const string CANCELLED = "Cancelled";
        public const string ERROR = "Error";
        public const string REVERTING = "Reverting";
        public const string REVERTED = "Reverted";
      }

      public class History
      {
        public const string COMPLETED = "Completed";
        public const string SKIPPED = "Skipped";
        public const string ERROR = "Error";

        public class Details
        {
          public const string EMPTY_STRING = "";
          public const string DUPLICATE_REDACTION_FOUND = "Duplicate redaction found";
          public const string REPRODUCED_REDACTION_IN_DOCUMENT_SET = "Reproduced Redaction in Document Set";
          public const string REPRODUCED_REDACTION_IN_RELATIONAL_GROUP = "Reproduced Redaction in Relational Group";
        }
      }
    }

    public class ImportJobType
    {
      public const string REPRODUCE = "Reproduce";
      public const string IMPORT = "Import";
      public const string REVERT = "Revert";
      public const string VALIDATE = "Validate";
    }

    public class Messages
    {
      public const string PRIORITY_REQUIRED = "Please enter a priority";
      public const string ARTIFACT_ID_REQUIRED = "Please enter an artifact ID";
      public const string AGENT_OFF_HOURS_NOT_FOUND = "No agent off-hours found in the configuration table.";
      public const string AGENT_OFF_HOUR_TIMEFORMAT_INCORRECT = "Please verify that the EDDS.Configuration AgentOffHourStartTime & AgentOffHourEndTime is in the following format HH:MM:SS";
    }

    public class ErrorMessages
    {
      public const string VALIDATE_FILE_CONTENTS_ERROR = "An error occured when validating file contents.";
      public const string COLUMN_NAME_MISMATCH = "Column name mismatch.";
      public const string COLUMN_COUNT_MISMATCH = "Column count mismatch.";
      public const string COLUMN_ORDER_MISMATCH = "Column order mismatch.";
      public const string PARSE_FILE_CONTENTS_ERROR = "An error occured when parsing file contents.";
      public const string INSERT_REDACTION_INTO_REDACTION_TABLE_ERROR = "An error occured when creating redaction.";
      public const string NOT_A_VALID_REDACTION_COUNT_FIELD_ON_THE_IMPORT_JOB_RDO = "Not a valid redaction count field on the import job rdo.";
      public const string REFER_TO_ERRORS_TAB_FOR_MORE_DETAILS = "Refer to Errors tab for more details.";
      public const string CREATE_REDACTION_AUDIT_RECORD_ERROR = "An error occured when creating redaction audit record.";
    }

    public class Buttons
    {
      public const string SUBMIT = "Submit";
      public const string CANCEL = "Cancel";
      public const string VALIDATE = "Validate";
      public const string DOWNLOAD = "Download";
      public const string REVERT = "Revert";
    }

    public class ImportFile
    {
      public const int COLUMN_COUNT = 32;
      public const char COLUMN_SEPARATOR = ',';
    }

    public class ExportFile
    {
      public const string EXPORT_FILE_EXTENSION = ".csv";

      public class Columns
      {
        public const string DOCUMENT_IDENTIFIER = "DocumentIdentifier";
        public const string FILE_ORDER = "FileOrder";
        public const string X = "X";
        public const string Y = "Y";
        public const string WIDTH = "Width";
        public const string HEIGHT = "Height";
        public const string MARKUP_TYPE = "MarkupType";
        public const string FILL_A = "FillA";
        public const string FILL_R = "FillR";
        public const string FILL_G = "FillG";
        public const string FILL_B = "FillB";
        public const string BORDER_SIZE = "BorderSize";
        public const string BORDER_A = "BorderA";
        public const string BORDER_R = "BorderR";
        public const string BORDER_G = "BorderG";
        public const string BORDER_B = "BorderB";
        public const string BORDER_STYLE = "BorderStyle";
        public const string FONT_NAME = "FontName";
        public const string FONT_A = "FontA";
        public const string FONT_R = "FontR";
        public const string FONT_G = "FontG";
        public const string FONT_B = "FontB";
        public const string FONT_SIZE = "FontSize";
        public const string FONT_STYLE = "FontStyle";
        public const string TEXT = "Text";
        public const string Z_ORDER = "ZOrder";
        public const string DRAW_CROSS_LINES = "DrawCrossLines";
        public const string MARKUP_SUB_TYPE = "MarkupSubType";
        public const string X_D = "X_d";
        public const string Y_D = "Y_d";
        public const string WIDTH_D = "Width_d";
        public const string HEIGHT_D = "Height_d";
      }

      public static readonly List<string> ColumnsList = new List<string>
      {
        Columns.DOCUMENT_IDENTIFIER,
        Columns.FILE_ORDER,
        Columns.X,
        Columns.Y,
        Columns.WIDTH,
        Columns.HEIGHT,
        Columns.MARKUP_TYPE,
        Columns.FILL_A,
        Columns.FILL_R,
        Columns.FILL_G,
        Columns.FILL_B,
        Columns.BORDER_SIZE,
        Columns.BORDER_A,
        Columns.BORDER_R,
        Columns.BORDER_G,
        Columns.BORDER_B,
        Columns.BORDER_STYLE,
        Columns.FONT_NAME,
        Columns.FONT_A,
        Columns.FONT_R,
        Columns.FONT_G,
        Columns.FONT_B,
        Columns.FONT_SIZE,
        Columns.FONT_STYLE,
        Columns.TEXT,
        Columns.Z_ORDER,
        Columns.DRAW_CROSS_LINES,
        Columns.MARKUP_SUB_TYPE,
        Columns.X_D,
        Columns.Y_D,
        Columns.WIDTH_D,
        Columns.HEIGHT_D
      };
    }

    public class Sql
    {
      public class AdminTables
      {
        public class ImportManagerQueue
        {
          public class Columns
          {
            public const string ID = "ID";
            public const string TIME_STAMP_UTC = "TimeStampUTC";
            public const string WORKSPACE_ARTIFACT_ID = "WorkspaceArtifactID";
            public const string QUEUE_STATUS = "QueueStatus";
            public const string AGENT_ID = "AgentID";
            public const string IMPORT_JOB_ARTIFACT_ID = "ImportJobArtifactID";
            public const string JOB_TYPE = "JobType";
            public const string CREATED_BY = "CreatedBy";
            public const string CREATED_ON = "CreatedOn";
            public const string RESOURCE_GROUP_ID = "ResourceGroupID";
          }
        }

        public class ImportWorkerQueue
        {
          public class Columns
          {
            public const string ID = "ID";
            public const string TIME_STAMP_UTC = "TimeStampUTC";
            public const string WORKSPACE_ARTIFACT_ID = "WorkspaceArtifactID";
            public const string DOCUMENT_IDENTIFIER = "DocumentIdentifier";
            public const string FILE_ORDER = "FileOrder";
            public const string QUEUE_STATUS = "QueueStatus";
            public const string AGENT_ID = "AgentID";
            public const string IMPORT_JOB_ARTIFACT_ID = "ImportJobArtifactID";
            public const string JOB_TYPE = "JobType";
            public const string X = "X";
            public const string Y = "Y";
            public const string WIDTH = "Width";
            public const string HEIGHT = "Height";
            public const string MARKUP_SET_ARTIFACT_ID = "MarkupSetArtifactID";
            public const string MARKUP_TYPE = "MarkupType";
            public const string FILL_A = "FillA";
            public const string FILL_R = "FillR";
            public const string FILL_G = "FillG";
            public const string FILL_B = "FillB";
            public const string BORDER_SIZE = "BorderSize";
            public const string BORDER_A = "BorderA";
            public const string BORDER_R = "BorderR";
            public const string BORDER_G = "BorderG";
            public const string BORDER_B = "BorderB";
            public const string BORDER_STYLE = "BorderStyle";
            public const string FONT_NAME = "FontName";
            public const string FONT_A = "FontA";
            public const string FONT_R = "FontR";
            public const string FONT_G = "FontG";
            public const string FONT_B = "FontB";
            public const string FONT_SIZE = "FontSize";
            public const string FONT_STYLE = "FontStyle";
            public const string TEXT = "Text";
            public const string Z_ORDER = "ZOrder";
            public const string DRAW_CROSS_LINES = "DrawCrossLines";
            public const string MARKUP_SUB_TYPE = "MarkupSubType";
            public const string RESOURCE_GROUP_ID = "ResourceGroupID";
            public const string SKIP_DUPLICATE_REDACTIONS = "SkipDuplicateRedactions";
            public const string X_D = "X_d";
            public const string Y_D = "Y_d";
            public const string WIDTH_D = "Width_d";
            public const string HEIGHT_D = "Height_d";
          }
        }

        public class ExportResults
        {
          public class Columns
          {
            public const string ID = "ID";
            public const string TIME_STAMP_UTC = "TimeStampUTC";
            public const string WORKSPACE_ARTIFACT_ID = "WorkspaceArtifactID";
            public const string DOCUMENT_IDENTIFIER = "DocumentIdentifier";
            public const string EXPORT_JOB_ARTIFACT_ID = "ExportJobArtifactID";
            public const string FILE_ORDER = "FileOrder";
            public const string X = "X";
            public const string Y = "Y";
            public const string WIDTH = "Width";
            public const string HEIGHT = "Height";
            public const string MARKUP_SET_ARTIFACT_ID = "MarkupSetArtifactID";
            public const string MARKUP_TYPE = "MarkupType";
            public const string FILL_A = "FillA";
            public const string FILL_R = "FillR";
            public const string FILL_G = "FillG";
            public const string FILL_B = "FillB";
            public const string BORDER_SIZE = "BorderSize";
            public const string BORDER_A = "BorderA";
            public const string BORDER_R = "BorderR";
            public const string BORDER_G = "BorderG";
            public const string BORDER_B = "BorderB";
            public const string BORDER_STYLE = "BorderStyle";
            public const string FONT_NAME = "FontName";
            public const string FONT_A = "FontA";
            public const string FONT_R = "FontR";
            public const string FONT_G = "FontG";
            public const string FONT_B = "FontB";
            public const string FONT_SIZE = "FontSize";
            public const string FONT_STYLE = "FontStyle";
            public const string TEXT = "Text";
            public const string Z_ORDER = "ZOrder";
            public const string DRAW_CROSS_LINES = "DrawCrossLines";
            public const string MARKUP_SUB_TYPE = "MarkupSubType";
            public const string X_D = "X_d";
            public const string Y_D = "Y_d";
            public const string WIDTH_D = "Width_d";
            public const string HEIGHT_D = "Height_d";
          }
        }
      }

      public class WorkspaceTables
      {
        public class RedactionMarkupType
        {
          public const string NAME = "RedactionMarkupType";

          public class Columns
          {
            public const string ID = "ID";
            public const string TYPE = "Type";
          }
        }

        public class RedactionMarkupSubType
        {
          public const string NAME = "RedactionMarkupSubType";

          public class Columns
          {
            public const string ID = "ID";
            public const string SUB_TYPE = "SubType";
          }
        }
      }
    }

    public class MarkupSet
    {
      public class MarkupSetMultiChoiceValues
      {
        public const string HAS_REDACTIONS = "Has Redactions";
        public const string HAS_HIGHLIGHTS = "Has Highlights";
      }

      public const string MARKUP_SET_FIELD_NAME_PREFIX = "Markup Set - ";
    }

    public class MarkupType
    {
      public class Redaction
      {
        public const string NAME = "Redaction";
        public const int VALUE = 1;
      }

      public class Highlight
      {
        public const string NAME = "Highlight";
        public const int VALUE = 2;
      }
    }

    public class MarkupSubTypeCategory
    {
      public static readonly List<int> RedactionsList = new List<int>
      {
        1,
        4,
        6,
        5,
        3
      };

      public static readonly List<int> HighlightsList = new List<int>
      {
        8,
        7,
        9,
        10,
        11,
        2
      };

      public static readonly List<MarkupUtilityType> SupportedMarkupUtilityTypes = new List<MarkupUtilityType>()
      {
        new MarkupUtilityType("Black", MarkupType.Redaction.NAME),
        new MarkupUtilityType("Cross", MarkupType.Redaction.NAME),
        new MarkupUtilityType("Inverse", MarkupType.Redaction.NAME),
        new MarkupUtilityType("White", MarkupType.Redaction.NAME),
        new MarkupUtilityType("Text", MarkupType.Redaction.NAME),
        new MarkupUtilityType("Blue",MarkupType.Highlight.NAME),
        new MarkupUtilityType("Green",MarkupType.Highlight.NAME),
        new MarkupUtilityType("Orange",MarkupType.Highlight.NAME),
        new MarkupUtilityType("Pink",MarkupType.Highlight.NAME),
        new MarkupUtilityType("Purple",MarkupType.Highlight.NAME),
        new MarkupUtilityType("Yellow",MarkupType.Highlight.NAME)
      };
    }

    public class AgentRaiseMessages
    {
      public const string AGENT_SERVER_NOT_PART_OF_ANY_RESOURCE_POOL = "This agent server is not part of any resource pools. Agent execution skipped.";
      public const string NO_RECORDS_IN_QUEUE_FOR_THIS_RESOURCE_POOL = "No records in the queue for this resource pool.";
    }

    public class AuditRecord
    {
      public const string APPLICATION_NAME = "Markup Utilities";

      public class AuditAction
      {
        public const int REDACTION_CREATED = 20;
        public const int REDACTION_DELETED = 22;
      }

      public class ValueType
      {
        public const string OLD_VALUE = "oldValue";
        public const string NEW_VALUE = "newValue";
      }
    }

    public class ImportJobRedactionCountFieldNames
    {
      public const string IMPORTED_REDACTION_COUNT = "Imported Redaction Count";
      public const string SKIPPED_REDACTION_COUNT = "Skipped Redaction Count";
      public const string ERROR_REDACTION_COUNT = "Error Redaction Count";
    }
  }
}
