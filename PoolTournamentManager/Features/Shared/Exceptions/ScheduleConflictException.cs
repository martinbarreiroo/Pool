using System;

namespace PoolTournamentManager.Features.Shared.Exceptions
{
    /// <summary>
    /// Exception thrown when a scheduling conflict is detected between players
    /// </summary>
    public class ScheduleConflictException : Exception
    {
        public Guid Player1Id { get; }
        public Guid Player2Id { get; }
        public DateTime ConflictTime { get; }

        public ScheduleConflictException(string message) : base(message)
        {
        }

        public ScheduleConflictException(string message, Guid player1Id, Guid player2Id, DateTime conflictTime)
            : base(message)
        {
            Player1Id = player1Id;
            Player2Id = player2Id;
            ConflictTime = conflictTime;
        }
    }
}