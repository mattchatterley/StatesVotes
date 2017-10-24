CREATE TABLE dbo.Member
(
	MemberId			INT IDENTITY(1,1),
	Position			NVARCHAR(250) NOT NULL,
	Name				NVARCHAR(250) NOT NULL
)