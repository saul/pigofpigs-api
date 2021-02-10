namespace PigOfPigs.Data

open System
open PigOfPigs.Models

[<RequireQualifiedAccess>]
module DbInitializer =

    let ensureCreated (context : PigContext) : unit =
        context.Database.EnsureCreated() |> ignore

        if not (Seq.isEmpty context.Players) then ()
        else

        Game(
            Title="Seed game",
            Date=DateTime(2021, 02, 07, 14, 06, 00),
            Results=[|
                PlayerResult(
                    Player=Player(Name="Saul"),
                    FinalScore=123,
                    Winner=true,
                    RoundPoints=[|
                        RoundPoints(Round=1, Points=0, TrailingBy=0) // 0
                        RoundPoints(Round=2, Points=0, TrailingBy=11) // 0
                        RoundPoints(Round=3, Points=0, TrailingBy=16) // 0
                        RoundPoints(Round=4, Points=10, TrailingBy=15) // 10
                        RoundPoints(Round=5, Points=8, TrailingBy=0) // 18
                        RoundPoints(Round=6, Points=11, TrailingBy=0) // 29
                        RoundPoints(Round=7, Points=10, TrailingBy=0) // 39
                        RoundPoints(Round=8, Points=(-39), TrailingBy=70) // 0
                        RoundPoints(Round=9, Points=60, TrailingBy=20) // 60
                        RoundPoints(Round=10, Points=63, TrailingBy=0) // 123
                    |]
                )
                PlayerResult(
                    Player=Player(Name="Josh"),
                    FinalScore=98,
                    Winner=false,
                    RoundPoints=[|
                        RoundPoints(Round=1, Points=0, TrailingBy=0) // 0
                        RoundPoints(Round=2, Points=11, TrailingBy=0) // 11
                        RoundPoints(Round=3, Points=5, TrailingBy=0) // 16
                        RoundPoints(Round=4, Points=9, TrailingBy=0) // 25
                        RoundPoints(Round=5, Points=(-25), TrailingBy=18) // 0
                        RoundPoints(Round=6, Points=18, TrailingBy=11) // 18
                        RoundPoints(Round=7, Points=1, TrailingBy=20) // 19
                        RoundPoints(Round=8, Points=51, TrailingBy=0) // 70
                        RoundPoints(Round=9, Points=10, TrailingBy=0) // 80
                        RoundPoints(Round=10, Points=18, TrailingBy=25) // 98
                    |]
                )
            |]
        )
        |> context.Games.Add
        |> ignore

        context.SaveChanges() |> ignore
