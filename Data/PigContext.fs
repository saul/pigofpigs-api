namespace PigOfPigs.Data

open Microsoft.EntityFrameworkCore
open PigOfPigs.Models

type PigContext(options : PigContext DbContextOptions) =
    inherit DbContext(options)

    [<DefaultValue>]
    val mutable private players : Player DbSet
    member this.Players with get() = this.players and set(v) = this.players <- v

    [<DefaultValue>]
    val mutable private playerResults : PlayerResult DbSet
    member this.PlayerResults with get() = this.playerResults and set(v) = this.playerResults <- v

    [<DefaultValue>]
    val mutable private games : Game DbSet
    member this.Games with get() = this.games and set(v) = this.games <- v

    [<DefaultValue>]
    val mutable private roundPoints : RoundPoints DbSet
    member this.RoundPoints with get() = this.roundPoints and set(v) = this.roundPoints <- v

    override _.OnModelCreating(modelBuilder) =
        modelBuilder.Entity<Player>().ToTable("Player") |> ignore
        modelBuilder.Entity<PlayerResult>().ToTable("PlayerResult") |> ignore
        modelBuilder.Entity<Game>().ToTable("Game") |> ignore
