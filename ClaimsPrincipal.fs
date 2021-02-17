namespace PigOfPigs

open System.Runtime.CompilerServices
open System.Security.Claims

[<Extension>]
type ClaimsPrincipalExtensions () =

    [<Extension>]
    static member IsAdmin (user : ClaimsPrincipal) =
        if not user.Identity.IsAuthenticated then false
        else

        let emailClaim =
            user.Claims
            |> Seq.tryFind (fun c -> c.Type = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")

        match emailClaim with
        | None -> false
        | Some emailClaim ->

        [
            "saul.rennison@gmail.com"
            "joshuttley@gmail.com"
        ]
        |> List.contains emailClaim.Value
