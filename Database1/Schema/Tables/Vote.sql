CREATE TABLE dbo.Vote
(
	VoteId			INT IDENTITY(1,1),
	Title			NVARCHAR(500) NOT NULL,
	Date			DATETIME,
	Reference		NVARCHAR(50),
	Status			NVARCHAR(150)
)