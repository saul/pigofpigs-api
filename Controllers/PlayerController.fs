namespace PigOfPigs.Controllers

open Microsoft.AspNetCore.Mvc
open PigOfPigs.Data
open Microsoft.EntityFrameworkCore
open System.Linq
open PigOfPigs.Models

type PlayerPosition =
    {
        Name : string
        GamesPlayed : int
        GamesWon : int
        WinRatio : float
        AverageScore : int
        HighestScore : int
    }

[<ApiController>]
[<Route "player">]
type PlayerController (pigContext : PigContext) =
    inherit ControllerBase()

    [<HttpGet>]
    member this.GetPlayers () =
        let q =
            query {
                for p in pigContext.Players.Include(fun p -> p.PlayedGames :> _ seq).ThenInclude(fun (p : PlayerResult) -> p.RoundPoints) do
                    let gamesPlayed = p.PlayedGames.Count
                    let gamesWon = p.PlayedGames.Where(fun g -> g.Winner).Count()
                    select {
                        Name = p.Name
                        GamesPlayed = gamesPlayed
                        GamesWon = gamesWon
                        WinRatio = float gamesWon / float gamesPlayed
                        AverageScore = int (p.PlayedGames.Average(fun g -> g.FinalScore))
                        HighestScore = p.PlayedGames.Max(fun g -> g.FinalScore)
                    }
            }
        q.OrderByDescending(fun p -> p.HighestScore)
            .ToList()
