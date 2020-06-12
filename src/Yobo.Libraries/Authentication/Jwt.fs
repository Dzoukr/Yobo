module Yobo.Libraries.Authentication.Jwt

open System
open System.IdentityModel.Tokens.Jwt
open Microsoft.IdentityModel.Tokens

let private isBeforeValid (before:Nullable<DateTime>) = 
    if before.HasValue && before.Value > DateTime.UtcNow then false else true
let private isExpirationValid (expires:Nullable<DateTime>) = 
    if expires.HasValue && DateTime.UtcNow > expires.Value then false else true

let getKey (secret:string) = SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secret)) 

let createJwtToken audience issuer secret expiration claims =
    let credentials = SigningCredentials(getKey secret, SecurityAlgorithms.HmacSha256)
    let issuedOn = DateTime.UtcNow
    let expiresOn = issuedOn.Add(expiration)
    let jwtToken = JwtSecurityToken(issuer, audience, claims, (issuedOn |> Nullable), (expiresOn |> Nullable), credentials)
    let handler = JwtSecurityTokenHandler()
    {| Token = handler.WriteToken(jwtToken); ExpiresOnUtc = expiresOn |}

let getParameters audience issuer secret = 
    let validationParams = TokenValidationParameters()
    validationParams.RequireExpirationTime <- true
    validationParams.ValidAudience <- audience
    validationParams.ValidIssuer <- issuer
    validationParams.ValidateLifetime <- true
    validationParams.LifetimeValidator <- (fun before expires _ _  -> isBeforeValid before && isExpirationValid expires)
    validationParams.ValidateIssuerSigningKey <- true
    validationParams.IssuerSigningKey <- secret |> getKey
    validationParams

let validateToken (validationParams:TokenValidationParameters) (token:string) =
    try
        let handler = JwtSecurityTokenHandler()
        let principal = handler.ValidateToken(token, validationParams, ref null)
        principal.Claims |> Some
    with _ -> None