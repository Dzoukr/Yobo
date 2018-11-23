module Yobo.Libraries.Security.SymetricCryptoProvider

open System

type SymetricCryptoProvider = {
    SetupKeyAndVector : Guid -> unit
    Encrypt: Guid -> string -> string
    Decrypt: Guid -> string -> string
}