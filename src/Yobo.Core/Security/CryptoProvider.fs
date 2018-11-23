module Yobo.Core.Security.CryptoProvider
open System

type CloudCryptoProvider = {
    GenerateKeyAndVector : Guid -> string * string
    
}