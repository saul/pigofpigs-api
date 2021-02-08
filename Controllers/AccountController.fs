namespace PigOfPigs.Controllers

open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Google
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Mvc
open FSharp.Control.Tasks.V2
open PigOfPigs

[<ApiController>]
[<Route "account">]
type AccountController () =
    inherit ControllerBase()

    [<AllowAnonymous; Route "google-login">]
    member this.GoogleLogin ([<FromQuery>] redirect : string) =
        let redirect = if isNull redirect then this.Url.Action(nameof this.Claims) else redirect
        let properties = AuthenticationProperties(RedirectUri = redirect)
        ChallengeResult(GoogleDefaults.AuthenticationScheme, properties)

    [<AllowAnonymous; Route "claims">]
    member this.Claims() = task {
        let claims =
            this.User.Claims
            |> Seq.map (fun claim -> {| Issuer = claim.Issuer; OriginalIssuer = claim.OriginalIssuer; Type = claim.Type; Value = claim.Value |})
            |> Array.ofSeq
        return
            {|
                Claims = claims
                IsAdmin = this.User.IsAdmin()
            |}
    }

    [<HttpGet; Authorize>]
    member this.View() : ActionResult =
        if not this.User.Identity.IsAuthenticated then
            this.Forbid() :> _
        else

        let email =
            this.User.Claims
            |> Seq.find (fun c -> c.Type = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")

        this.Ok(
            {|
                Name = this.User.Identity.Name
                Email = email.Value
            |}
        ) :> _
