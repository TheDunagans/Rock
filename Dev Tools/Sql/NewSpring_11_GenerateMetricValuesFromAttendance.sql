/*
	This script deletes/repopulates metric values for:
		- Attendance
			- KidSpring Attendance
				- Service Numbers
				- Total Numbers
			- Fuse Attendance
				- Total Numbers
		- Volunteers
			- ***
				- ***
					- * Service Attendance
					- * Total Attendance
*/

-- Constants
DECLARE @metricValueTypeMeasure AS INT = 0; -- From enum in Rock code
DECLARE @defaultOrder AS INT = 0;
DECLARE @metricForeignIdToGroupIdOffset AS INT = 0; -- This is because groups were removed and readded to beta rock, but the foreign id's were not updated

-- Get entity Ids for this instance of Rock
DECLARE @campusEntityTypeId AS INT = (SELECT Id FROM EntityType WHERE Name = 'Rock.Model.Campus');
DECLARE @metricCategoryEntityTypeId AS INT = (SELECT Id FROM EntityType WHERE Name = 'Rock.Model.MetricCategory');

-- Get KidSpring Ids
DECLARE @ksCategoryId AS INT = (SELECT Id FROM Category WHERE Name = 'KidSpring Attendance' AND EntityTypeId = @metricCategoryEntityTypeId);
DECLARE @ksServiceMetricId AS INT = (SELECT m.Id FROM [Metric] m JOIN MetricCategory mc ON mc.MetricId = m.Id WHERE mc.CategoryId = @ksCategoryId AND m.[Title] = 'Service Numbers');
DECLARE @ksTotalMetricId AS INT = (SELECT m.Id FROM [Metric] m JOIN MetricCategory mc ON mc.MetricId = m.Id WHERE mc.CategoryId = @ksCategoryId AND m.[Title] = 'Total Numbers');

-- Get Fuse Ids
DECLARE @fuseCategoryId AS INT = (SELECT Id FROM Category WHERE Name = 'Fuse Attendance' AND EntityTypeId = @metricCategoryEntityTypeId);
DECLARE @fuseTotalMetricId AS INT = (SELECT m.Id FROM [Metric] m JOIN MetricCategory mc ON mc.MetricId = m.Id WHERE mc.CategoryId = @fuseCategoryId AND m.[Title] = 'Total Numbers');

-- Delete existing metric values
DELETE FROM MetricValue WHERE MetricId IN (@ksServiceMetricId, @ksTotalMetricId, @fuseTotalMetricId);

-- Re/create temp table that stores the parent group ids of all KidSpring attendance groups we are interested in
IF OBJECT_ID('tempdb..#tempKidSpringAttendeeParentGroupIds') IS NOT NULL DROP TABLE #tempKidSpringAttendeeParentGroupIds;

SELECT
	Id
INTO 
	#tempKidSpringAttendeeParentGroupIds
FROM
	[Group]
WHERE
	Name IN (
		'Nursery Attendee',
		'Preschool Attendee',
		'Elementary Attendee',
		'Special Needs Attendee'
	);

-- Re/create a temp table that has Sunday schedules with a time field (looks like '0915', '1115', etc)
IF OBJECT_ID('tempdb..#tempSchedules') IS NOT NULL DROP TABLE #tempSchedules;

SELECT
	[Id],
	REPLACE(Name, 'Sunday ', '') AS [Time]
INTO
	#tempSchedules
FROM [Schedule]
WHERE
	Name like 'Sunday %';

-- Re/create temp table with counts of kids attending by campus per service
IF OBJECT_ID('tempdb..#tempByCampus') IS NOT NULL DROP TABLE #tempByCampus;

SELECT
	COUNT(a.[Id]) AS Value,
	a.[CampusId],
	CONVERT(DATETIME, CONCAT(a.SundayDate, ' ', s.[Time])) AS [Date]
INTO
	#tempByCampus
FROM 
	[Attendance] a
	JOIN [Group] g ON a.GroupId = g.Id
	JOIN [#tempKidSpringAttendeeParentGroupIds] p ON p.Id = g.ParentGroupId
	JOIN #tempSchedules s ON s.Id = a.ScheduleId
WHERE 
	a.DidAttend = 1
GROUP BY
	a.CampusId,
	a.SundayDate,
	s.[Time];

-- Insert values for the "by service" metric
INSERT INTO [MetricValue] (
	[MetricValueType]
    ,[YValue]
    ,[Order]
    ,[MetricId]
    ,[MetricValueDateTime]
    ,[Guid]
    ,[EntityId]
	,[CreatedDateTime]
) SELECT
	@metricValueTypeMeasure,
	v.Value,
	@defaultOrder,
	@ksServiceMetricId,
	v.[Date],
	NEWID(),
	v.CampusId,
	GETDATE()
FROM
	#tempByCampus v;

-- Insert values for the "total" metric
INSERT INTO [MetricValue] (
	[MetricValueType]
    ,[YValue]
    ,[Order]
    ,[MetricId]
    ,[MetricValueDateTime]
    ,[Guid]
    ,[EntityId]
	,[CreatedDateTime]
) SELECT
	@metricValueTypeMeasure,
	SUM(v.Value),
	@defaultOrder,
	@ksTotalMetricId,
	CONVERT(DATE, v.[Date]),
	NEWID(),
	v.CampusId,
	GETDATE()
FROM
	#tempByCampus v
GROUP BY
	CONVERT(DATE, v.[Date]),
	v.CampusId;

-- Insert values for the "total" Fuse metric
WITH cteFuseValues AS (
	SELECT
		a.CampusId,
		COUNT(a.Id) AS Value,
		CONVERT(DATE, a.StartDateTime) AS [Date]
	FROM 
		Attendance a
		JOIN [Group] g ON a.GroupId = g.Id
		JOIN [Group] p ON g.ParentGroupId = p.Id
	WHERE
		p.Name = 'Fuse Attendee'
	GROUP BY
		a.CampusId,
		CONVERT(DATE, a.StartDateTime)
)
INSERT INTO [MetricValue] (
	[MetricValueType]
    ,[YValue]
    ,[Order]
    ,[MetricId]
    ,[MetricValueDateTime]
    ,[Guid]
    ,[EntityId]
	,[CreatedDateTime]
) SELECT
	@metricValueTypeMeasure,
	v.Value,
	@defaultOrder,
	@fuseTotalMetricId,
	v.[Date],
	NEWID(),
	v.CampusId,
	GETDATE()
FROM
	cteFuseValues v;

-- Create volunteer metric to group mapping temp table
IF OBJECT_ID('tempdb..#tempVolMetricToGroup') IS NOT NULL DROP TABLE #tempVolMetricToGroup;

SELECT 
	g.Id AS GroupId,
	m.Id AS MetricId,
	m.Title AS MetricTitle
INTO
	#tempVolMetricToGroup
FROM 
	Metric m
	JOIN MetricCategory mc ON mc.MetricId = m.Id
	JOIN Category c ON c.Id = mc.CategoryId
	JOIN Category pc ON pc.Id = c.ParentCategoryId
	JOIN Category gpc ON gpc.Id = pc.ParentCategoryId
	JOIN [Group] g ON g.Id = (m.ForeignId + @metricForeignIdToGroupIdOffset)
WHERE
	gpc.Name = 'Volunteers'
	AND gpc.EntityTypeId = @metricCategoryEntityTypeId
ORDER BY MetricId;
	
-- Update the foreign ids if there is an offset so that the offset can be eliminated
IF @metricForeignIdToGroupIdOffset <> 0
BEGIN
	UPDATE m
	SET m.ForeignId = temp.GroupId
	FROM 
		Metric m
		JOIN #tempVolMetricToGroup temp ON temp.MetricId = m.Id

	SELECT @metricForeignIdToGroupIdOffset = 0;
	SELECT 'Foreign ids fixed - @metricForeignIdToGroupIdOffset needs to be set to 0 before running script again!!!';
END 

-- Delete existing metric values
DELETE FROM MetricValue WHERE MetricId IN (SELECT MetricId FROM #tempVolMetricToGroup);

-- Re/create temp table with counts of volunteers by metric by group by campus per service
IF OBJECT_ID('tempdb..#tempVolsByGroupAndCampus') IS NOT NULL DROP TABLE #tempVolsByGroupAndCampus;

SELECT
	COUNT(a.[Id]) AS Value,
	a.[CampusId],
	CONVERT(DATETIME, CONCAT(a.SundayDate, ' ', s.[Time])) AS [Date],
	m.GroupId
INTO
	#tempVolsByGroupAndCampus
FROM 
	[Attendance] a
	JOIN [#tempVolMetricToGroup] m ON m.GroupId = a.GroupId
	JOIN #tempSchedules s ON s.Id = a.ScheduleId
WHERE 
	a.DidAttend = 1
GROUP BY
	a.CampusId,
	a.SundayDate,
	s.[Time],
	m.GroupId;

-- Insert values by service
INSERT INTO [MetricValue] (
	[MetricValueType]
    ,[YValue]
    ,[Order]
    ,[MetricId]
    ,[MetricValueDateTime]
    ,[Guid]
    ,[EntityId]
	,[CreatedDateTime]
)
SELECT
	@metricValueTypeMeasure,
	v.Value,
	@defaultOrder,
	g.MetricId,
	v.[Date],
	NEWID(),
	v.CampusId,
	GETDATE()
FROM
	#tempVolsByGroupAndCampus v
	JOIN #tempVolMetricToGroup g ON v.GroupId = g.GroupId
WHERE
	g.MetricTitle LIKE '% Service Attendance';

-- Insert values by day
INSERT INTO [MetricValue] (
	[MetricValueType]
    ,[YValue]
    ,[Order]
    ,[MetricId]
    ,[MetricValueDateTime]
    ,[Guid]
    ,[EntityId]
	,[CreatedDateTime]
)
SELECT
	@metricValueTypeMeasure,
	SUM(v.Value),
	@defaultOrder,
	g.MetricId,
	CONVERT(DATE, v.[Date]),
	NEWID(),
	v.CampusId,
	GETDATE()
FROM
	#tempVolsByGroupAndCampus v
	JOIN #tempVolMetricToGroup g ON v.GroupId = g.GroupId
WHERE
	g.MetricTitle LIKE '% Total Attendance'
GROUP BY
	CONVERT(DATE, v.[Date]),
	v.CampusId,
	g.MetricId;