ALTER TABLE dbo.VotingHistory ADD CONSTRAINT FK_VotingHistory_Vote_VoteId FOREIGN KEY (VoteId) REFERENCES dbo.Vote (VoteId)
GO