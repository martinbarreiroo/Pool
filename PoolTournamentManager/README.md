# 8-Ball Pool Tournament Manager

A comprehensive ASP.NET Core Web API application for managing pool tournaments, matches, and player profiles.

## Features

- Player management with profile pictures stored in Amazon S3
- Match scheduling with double-booking prevention
- Tournament and league tracking
- Ranking system based on match results

## Tech Stack

- **ASP.NET Core 9.0** - Web API framework
- **Entity Framework Core** - ORM for database operations
- **PostgreSQL** - Database
- **Amazon S3** - Storage for player profile pictures
- **Docker** - Containerization for development environment

## Project Structure

This project follows a domain-driven design approach:

```
PoolTournamentManager/
├── Features/
│   ├── Players/     # Player domain
│   ├── Matches/     # Match domain
│   └── Tournaments/ # Tournament domain
├── Shared/          # Cross-cutting concerns
│   ├── Extensions/
│   └── Infrastructure/
└── Migrations/      # Database migrations
```

## Getting Started

### Prerequisites

- .NET 9.0 SDK or later
- Docker Desktop
- AWS Account (for S3 storage)
- AWS CLI (configured with credentials)

### Setting Up the Development Environment

1. Clone the repository:
   ```
   git clone https://github.com/yourusername/PoolTournamentManager.git
   cd PoolTournamentManager
   ```

2. Configure the environment variables:
   Create a `.env` file in the project root with the following content:
   ```
   AWS_ACCESS_KEY_ID=your-access-key-id
   AWS_SECRET_ACCESS_KEY=your-secret-access-key
   AWS_REGION=us-east-1
   S3_BUCKET_NAME=pool-tournament-manager
   
   # Database Configuration
   DB_HOST=localhost
   DB_PORT=5432
   DB_USER=admin
   DB_PASSWORD=admin
   DB_NAME=pool_tournament_manager-db
   ```

3. Start the PostgreSQL database:
   ```
   docker compose up -d
   ```

4. Set up the S3 bucket:
   ```
   ./setup-s3.sh
   ```

5. Apply the database migrations:
   ```
   dotnet ef database update
   ```

6. Run the application:
   ```
   dotnet run
   ```

The API will be available at `http://localhost:5260`

## API Endpoints

### Players

- `GET /api/players` - Get all players
- `GET /api/players/{id}` - Get a specific player
- `POST /api/players` - Create a new player
- `PUT /api/players/{id}` - Update a player
- `DELETE /api/players/{id}` - Delete a player
- `POST /api/players/{id}/profile-picture` - Generate a pre-signed URL for profile picture upload

### Tournaments

- `GET /api/tournaments` - Get all tournaments
- `GET /api/tournaments?isActive=true` - Get active tournaments
- `GET /api/tournaments/{id}` - Get a specific tournament
- `POST /api/tournaments` - Create a new tournament
- `PUT /api/tournaments/{id}` - Update a tournament
- `DELETE /api/tournaments/{id}` - Delete a tournament

### Matches

- `GET /api/matches` - Get all matches
- `GET /api/matches?startDate=X&endDate=Y` - Get matches in a date range
- `GET /api/matches?playerId=X` - Get matches for a specific player
- `GET /api/matches?tournamentId=X` - Get matches for a specific tournament
- `GET /api/matches/{id}` - Get a specific match
- `POST /api/matches` - Create a new match
- `PUT /api/matches/{id}` - Update a match
- `DELETE /api/matches/{id}` - Delete a match

## License

This project is licensed under the MIT License - see the LICENSE file for details.
