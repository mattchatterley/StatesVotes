IF EXISTS ( SELECT 1 FROM dbo.SysObjects WHERE id=OBJECT_ID('dbo.VotingHistory_Insert') AND OBJECTPROPERTY(id,'IsProcedure')=1)
	BEGIN
		DROP PROCEDURE dbo.VotingHistory_Insert
	END
GO

CREATE PROCEDURE dbo.VotingHistory_Insert
	@VoteId			INT,
	@Position		NVARCHAR(250),
	@Name			NVARCHAR(250),
	@Voted			NVARCHAR(250)
AS
BEGIN
	SET NOCOUNT ON

	-- 1. Ensure state exists
	DECLARE @VoteStateId INT

	IF NOT EXISTS ( SELECT 1 FROM dbo.VoteState WHERE Label = @Voted )
		BEGIN
			INSERT INTO dbo.VoteState (Label) VALUES (@Voted)
		END

	SELECT @VoteStateId = VoteStateId FROM dbo.VoteState WHERE Label = @Voted

	-- 2. Ensure member exists
	DECLARE @MemberId INT

	IF NOT EXISTS ( SELECT 1 FROM dbo.Member WHERE Name = @Name )
		BEGIN
			INSERT INTO dbo.Member (Position, Name) VALUES (@Position, @Name)
		END

	SELECT @MemberId = MemberId FROM dbo.Member WHERE Name = @Name

	-- update their current rank (assume we load oldest first)
	UPDATE dbo.Member SET Position = @Position WHERE MemberId = @MemberId

	-- store vote
	INSERT INTO dbo.VotingHistory (VoteId, MemberId, VoteStateId) VALUES (@VoteId, @MemberId, @VoteStateId)
END   

