/****** Script for SelectTopNRows command from SSMS  ******/
SELECT
(SELECT COUNT(Id) FROM [MessageBroker].[queue].[Audit] with(nolock)) AS 'Audit',
(SELECT COUNT(Id) FROM [MessageBroker].[queue].[Error] with(nolock)) AS 'Error',
(SELECT COUNT(Id) FROM [MessageBroker].[queue].[IntegrationTest] with(nolock)) AS 'Queue',
(SELECT COUNT(Id) FROM [MessageBroker].[timeout].[IntegrationTest] with(nolock)) AS 'Timeout',
(SELECT COUNT(Id) FROM [IntegrationTest].[dbo].[Records] with(nolock)) AS 'Records',
(SELECT COUNT(Id) FROM [IntegrationTest].[dbo].[RecordLogs] with(nolock)) AS 'Logs'



--TRUNCATE TABLE [MessageBroker].[queue].[Audit]
--TRUNCATE TABLE [MessageBroker].[queue].[Error]
--TRUNCATE TABLE [MessageBroker].[queue].[IntegrationTest]
--TRUNCATE TABLE [MessageBroker].[timeout].[IntegrationTest]