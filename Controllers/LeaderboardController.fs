namespace PigOfPigs.Controllers

open System
open System.Linq
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open PigOfPigs.Data
open Microsoft.EntityFrameworkCore
open FSharp.Control.Tasks.V2

type GameResult =
    {
        Player : string
        Date : DateTime
        Game : string
        GameID : int
        Score : int
    }

type RoundPointsResult =
    {
        Player : string
        Date : DateTime
        Game : string
        GameID : int
        Round : int
        Points : int
    }

[<ApiController>]
[<Route("leaderboard")>]
type LeaderboardController (logger : ILogger<LeaderboardController>, pigContext : PigContext) =
    inherit ControllerBase()

    [<Route "best-scores">]
    member _.Scores() = task {
        let! results =
            pigContext.PlayerResults
                .Include(fun x -> x.Game)
                .Include(fun x -> x.Player)
                .OrderByDescending(fun x -> x.FinalScore)
                .Where(fun x -> x.FinalScore > 0)
                .Take(10)
                .ToListAsync()

        return
            results
            |> Seq.map (fun pr ->
                {
                    Player = pr.Player.Name
                    Date = pr.Game.Date
                    Game = pr.Game.Title
                    GameID = pr.Game.ID
                    Score = pr.FinalScore
                }
            )
    }

    [<Route "best-rounds">]
    member _.BestRounds() = task {
        let! results =
            pigContext.RoundPoints
                .Include(fun x -> x.PlayerResult)
                    .ThenInclude(fun x -> x.Game)
                .Include(fun x -> x.PlayerResult)
                    .ThenInclude(fun x -> x.Player)
                .OrderByDescending(fun x -> x.Points)
                .Where(fun x -> x.Points > 0)
                .Take(10)
                .ToListAsync()

        return
            results
            |> Seq.map (fun rs ->
                {
                    Player = rs.PlayerResult.Player.Name
                    Date = rs.PlayerResult.Game.Date
                    Game = rs.PlayerResult.Game.Title
                    GameID = rs.PlayerResult.Game.ID
                    Round = rs.Round
                    Points = rs.Points
                }
            )
    }

    [<Route "worst-rounds">]
    member _.WorstRounds() = task {
        let! results =
            pigContext.RoundPoints
                .Include(fun x -> x.PlayerResult)
                    .ThenInclude(fun x -> x.Game)
                .Include(fun x -> x.PlayerResult)
                    .ThenInclude(fun x -> x.Player)
                .OrderBy(fun x -> x.Points)
                .Where(fun x -> x.Points < 0)
                .Take(10)
                .ToListAsync()

        return
            results
            |> Seq.map (fun rs ->
                {
                    Player = rs.PlayerResult.Player.Name
                    Date = rs.PlayerResult.Game.Date
                    Game = rs.PlayerResult.Game.Title
                    GameID = rs.PlayerResult.Game.ID
                    Round = rs.Round
                    Points = rs.Points
                }
            )
    }

    [<Route "comebacks">]
    member _.Comebacks() = task {
        let! results =
            pigContext.RoundPoints
                .Where(fun x -> x.TrailingBy > 0)
                .OrderByDescending(fun x -> x.TrailingBy)
                .Where(fun x ->
                    // Only games that they ended up winning
                    x.PlayerResult.Game.Results.Max(fun r -> r.FinalScore) = x.PlayerResult.FinalScore
                )
                .Include(fun x -> x.PlayerResult)
                    .ThenInclude(fun x -> x.Game)
                .Include(fun x -> x.PlayerResult)
                    .ThenInclude(fun x -> x.Player)
                .Take(10)
                .ToListAsync()

        return
            results
            |> Seq.distinctBy (fun x -> x.PlayerResult.GameID)
            |> Seq.map (fun rs ->
                {
                    Player = rs.PlayerResult.Player.Name
                    Date = rs.PlayerResult.Game.Date
                    Game = rs.PlayerResult.Game.Title
                    GameID = rs.PlayerResult.Game.ID
                    Round = rs.Round
                    Points = rs.TrailingBy
                }
            )
    }
