ALTER TABLE dbo.VotingHistory ADD CONSTRAINT FK_VotingHistory_VoteState_VoteStateId FOREIGN KEY (VoteStateId) REFERENCES dbo.VoteState (VoteStateId)
GO