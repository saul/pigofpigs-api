namespace PigOfPigs.Models

open System
open System.Collections.Generic
open System.ComponentModel.DataAnnotations
open System.Text.Json.Serialization

[<AllowNullLiteral>]
type PlayerResult () =
    member val PlayerResultID : int = 0 with get, set
    member val GameID : int = 0 with get, set
    member val PlayerID : int = 0 with get, set

    /// Sum of round score points
    member val FinalScore : int = 0 with get, set

    /// Whether this player won the game
    member val Winner : bool = false with get, set

    member val RoundPoints : RoundPoints ICollection = null with get, set
    member val Player : Player = null with get, set

    [<JsonIgnore>]
    member val Game : Game = null with get, set

and [<AllowNullLiteral>] RoundPoints () =
    member val ID : int = 0 with get, set
    member val PlayerResultID : int = 0 with get, set
    member val Round : int = 0 with get, set
    member val Points : int = 0 with get, set
    member val TrailingBy : int = 0 with get, set

    member val PlayerResult : PlayerResult = null with get, set

and [<AllowNullLiteral>] Player () =
    member val ID : int = 0 with get, set

    [<Required>]
    member val Name : string = null with get, set

    [<JsonIgnore>]
    member val PlayedGames : PlayerResult ICollection = null with get, set

and [<AllowNullLiteral>] Game () =
    member val ID : int = 0 with get, set

    [<Required>]
    member val Title : string = null with get, set

    member val Date : DateTime = DateTime.MinValue with get, set

    member val Results : PlayerResult ICollection = null with get, set
