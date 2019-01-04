namespace Yobo.Libraries.Authorization

open System

type Base64String = private Base64String of string with

    static member Decode (base64String : Base64String) =
        let (Base64String text) = base64String
        let pad text =
            let padding = 3 - ((String.length text + 3) % 4)
            if padding = 0 then text else (text + new String('=', padding))
        Convert.FromBase64String(pad(text.Replace('-', '+').Replace('_', '/')))

    static member Create data =
        Convert.ToBase64String(data)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_') |> Base64String;

    static member fromString = Base64String

    override this.ToString() =
        let (Base64String str) = this
        str

type Configuration = {
    Issuer : string
    Audience : string
    Secret : Base64String
    TokenLifetime : TimeSpan
}

type Token = {
    AccessToken : string
    Expires : TimeSpan
}

type Authorizator = {
    CreateToken : seq<Security.Claims.Claim> -> Token
    ValidateToken : string -> seq<Security.Claims.Claim> option
}