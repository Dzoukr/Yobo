module Yobo.Libraries.Authorization.Jwt

open System
open System.IdentityModel.Tokens.Jwt
open Microsoft.IdentityModel.Tokens

let private createJwtToken issuer expiration credentials audience claims =
    let issuedOn = DateTime.UtcNow
    let expiresBy = issuedOn.Add(expiration)
    new JwtSecurityToken(issuer, audience, claims, (issuedOn |> Nullable), (expiresBy |> Nullable), credentials)

let private extractAccessToken (expiration:TimeSpan) jwtToken =
    let handler = new JwtSecurityTokenHandler()
    let accessToken = handler.WriteToken(jwtToken)
    {AccessToken = accessToken; Expires = expiration}

let private isBeforeValid (before:Nullable<DateTime>) = 
    if before.HasValue && before.Value > DateTime.UtcNow then false else true
let private isExpirationValid (expires:Nullable<DateTime>) = 
    if expires.HasValue && DateTime.UtcNow > expires.Value then false else true

let private createToken (config:Configuration) claims = 
    let credentials = Keys.createCredentials config.Secret
    createJwtToken config.Issuer config.TokenLifetime credentials config.Audience claims
    |> extractAccessToken config.TokenLifetime 

let private validateToken (config:Configuration) token =
    try
        let parameters = 
            let validationParams = new TokenValidationParameters()
            validationParams.RequireExpirationTime <- true
            validationParams.ValidAudience <- config.Audience
            validationParams.ValidIssuer <- config.Issuer
            validationParams.ValidateLifetime <- true
            validationParams.LifetimeValidator <- (fun before expires _ _  -> isBeforeValid before && isExpirationValid expires)
            validationParams.ValidateIssuerSigningKey <- true
            validationParams.IssuerSigningKey <- config.Secret |> Keys.createSecurityKey
            validationParams
    
        let handler = new JwtSecurityTokenHandler()
        let principal = handler.ValidateToken(token, parameters, ref null)
        
        principal.Claims |> Some
    with _ -> None

let createAuthorizator (config:Configuration) = {
    CreateToken = createToken config
    ValidateToken = validateToken config
}