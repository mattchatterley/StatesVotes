IF EXISTS ( SELECT 1 FROM dbo.SysObjects WHERE id=OBJECT_ID('dbo.Vote_Insert') AND OBJECTPROPERTY(id,'IsProcedure')=1)
	BEGIN
		DROP PROCEDURE dbo.Vote_Insert
	END
GO

CREATE PROCEDURE dbo.Vote_Insert
	@Title			NVARCHAR(500),
	@Date			DATETIME,
	@Reference		NVARCHAR(50),
	@Status			NVARCHAR(150),
	@VoteId			INT OUTPUT
AS
BEGIN
	SET NOCOUNT ON

	INSERT INTO dbo.Vote (Title, Date, Reference, Status) VALUES ( @Title, @Date, @Reference, @Status )

	SET @VoteId = SCOPE_IDENTITY()
END   