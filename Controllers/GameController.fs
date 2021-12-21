namespace PigOfPigs.Controllers

open System
open System.Linq
open System.ComponentModel.DataAnnotations
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open PigOfPigs
open PigOfPigs.Data
open PigOfPigs.Models
open Microsoft.EntityFrameworkCore
open FSharp.Control.Tasks.V2

[<CLIMutable>]
type CreateGamePlayer =
    {
        [<Required>]
        Name : string

        [<MinLength 10>]
        Scores : int[]
    }

[<CLIMutable>]
type CreateGameRequest =
    {
        [<Required>]
        Title : string

        [<Required>]
        Date : DateTime

        [<MinLength 2>]
        Players : CreateGamePlayer[]
    }

[<ApiController>]
[<Route("game")>]
type GameController (logger : ILogger<GameController>, pigContext : PigContext) =
    inherit ControllerBase()

    [<HttpGet>]
    [<Route "{id}">]
    member this.GetGame(id : int) : ActionResult Task = task {
        let! game =
            pigContext.Games
                .Include(fun g -> g.Results :> _ seq)
                    .ThenInclude(fun (pr : PlayerResult) -> pr.Player)
                .Include(fun g -> g.Results :> _ seq)
                    .ThenInclude(fun (pr : PlayerResult) -> pr.RoundPoints :> _ seq)
                .SingleOrDefaultAsync(fun g -> g.ID = id)

        if isNull game then
            return this.NotFound() :> _
        else
            return this.Ok({
                Title = game.Title
                Date = game.Date
                Players = [|
                    for pr in game.Results ->
                        let scores =
                            pr.RoundPoints
                            |> Seq.sortBy (fun rp -> rp.Round)
                            |> Seq.map (fun rp -> rp.Points)
                            |> Seq.scan (+) 0
                            |> Seq.skip 1
                            |> Array.ofSeq

                        { Name = pr.Player.Name; Scores = scores }
                |]
            }) :> _
    }

    [<HttpGet>]
    member this.GetGames() = task {
        let! games =
            pigContext.Games
                .Include(fun g -> g.Results :> _ seq)
                    .ThenInclude(fun (pr : PlayerResult) -> pr.Player)
                .OrderByDescending(fun g -> g.ID)
                .ToListAsync()

        return this.Ok(
            games
            |> Seq.map (fun g ->
                {|
                    Id = g.ID
                    Title = g.Title
                    Date = g.Date
                    Players =
                        g.Results
                        |> Seq.sortByDescending (fun pr -> pr.FinalScore)
                        |> Seq.map (fun pr -> {|
                            Name = pr.Player.Name
                            Score = pr.FinalScore
                        |})
                |}
            )
        )
    }

    [<HttpPost>]
    member this.CreateGame([<FromBody>] game : CreateGameRequest) : ActionResult Task = task {
        if not this.User.Identity.IsAuthenticated then
            return this.StatusCode(403, "You must login before adding games") :> _
        elif not (this.User.IsAdmin()) then
            return this.StatusCode(403, "You are not allowed to add new games") :> _
        else

        let rounds = game.Players |> Seq.map (fun p -> p.Scores.Length) |> Seq.max

        let maxRoundScores =
            [|
                for i in 0..rounds-1 ->
                    game.Players
                    |> Seq.map (fun p -> p.Scores.[i])
                    |> Seq.max
            |]

        let winningPoints = Array.last maxRoundScores

        let toPlayerResultAsync (createPlayer : CreateGamePlayer) =
            let finalScore, reverseRoundPoints =
                createPlayer.Scores
                |> Seq.indexed
                |> Seq.fold
                    (fun (scoreLastRound, points) (i, scoreThisRound) ->
                        let roundPoints =
                            RoundPoints(
                                Round=i + 1,
                                Points=scoreThisRound - scoreLastRound,
                                TrailingBy=maxRoundScores.[i] - scoreThisRound
                            )
                        scoreThisRound, roundPoints::points
                    )
                    (0, [])

            task {
                let! player = pigContext.Players.FirstOrDefaultAsync(fun p -> p.Name = createPlayer.Name)
                return
                    PlayerResult(
                        Player=(if isNull player then Player(Name=createPlayer.Name) else player),
                        FinalScore=finalScore,
                        Winner=(finalScore = winningPoints),
                        RoundPoints=(reverseRoundPoints |> Seq.rev |> Array.ofSeq)
                    )
            }

        let! playerResults =
            Task.WhenAll [|
                for player in game.Players ->
                    for score in player.Scores do
                        if score < 0 then failwithf "Invalid score %d for player %s" score player.Name
                    toPlayerResultAsync player
            |]

        let game =
            Game(
                Title=game.Title,
                Date=game.Date,
                Results=playerResults
            )

        game
        |> pigContext.Games.Add
        |> ignore

        let! _ = pigContext.SaveChangesAsync()

        return this.Ok({| id = game.ID |}) :> _
    }

    [<HttpPost; Route "{id}/delete">]
    member this.DeleteGame([<FromRoute>] id : int) : ActionResult Task = task {
        if not this.User.Identity.IsAuthenticated then
            return this.StatusCode(403, "You must login before adding games") :> _
        elif not (this.User.IsAdmin()) then
            return this.StatusCode(403, "You are not allowed to add new games") :> _
        else
            let! game = pigContext.Games.FirstOrDefaultAsync(fun g -> g.ID = id)
            pigContext.Remove game |> ignore
            let! _ = pigContext.SaveChangesAsync()
            return this.Ok() :> _
    }
