ALTER TABLE dbo.VotingHistory ADD CONSTRAINT FK_VotingHistory_Member_MemberId FOREIGN KEY (MemberId) REFERENCES dbo.Member (MemberId)
GO
