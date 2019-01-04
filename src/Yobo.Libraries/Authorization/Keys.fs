module Yobo.Libraries.Authorization.Keys

open Microsoft.IdentityModel.Tokens

let createSecurityKey secret =
    let symmetricKey = secret |> Base64String.Decode
    new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(symmetricKey) :> SecurityKey   

let hmacSha256 secretKey = 
    new SigningCredentials(secretKey,SecurityAlgorithms.HmacSha256)

let createCredentials = (createSecurityKey >> hmacSha256)