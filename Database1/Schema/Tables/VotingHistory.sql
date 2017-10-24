CREATE TABLE dbo.VotingHistory
(
	VotingHistoryId INT IDENTITY(1,1),
	VoteId			INT NOT NULL,
	MemberId		INT NOT NULL,
	VoteStateId		INT NOT NULL
)