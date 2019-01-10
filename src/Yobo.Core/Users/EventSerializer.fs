module Yobo.Core.Users.EventSerializer

open Yobo.Core
open Yobo.Libraries.Security.SymetricCryptoProvider

let private encryptRegister (cryptoProvider:SymetricCryptoProvider) (args:CmdArgs.Register) = 
    do cryptoProvider.SetupKeyAndVector args.Id
    let email = args.Email |> cryptoProvider.Encrypt args.Id
    let firstName = args.FirstName |> cryptoProvider.Encrypt args.Id
    let lastName = args.LastName |> cryptoProvider.Encrypt args.Id
    { args with Email = email; FirstName = firstName; LastName = lastName }

let private decryptRegister (cryptoProvider:SymetricCryptoProvider) (args:CmdArgs.Register) = 
    let email = args.Email |> cryptoProvider.Decrypt args.Id
    let firstName = args.FirstName |> cryptoProvider.Decrypt args.Id
    let lastName = args.LastName |> cryptoProvider.Decrypt args.Id
    { args with Email = email; FirstName = firstName; LastName = lastName }

let toEvent (cryptoProvider:SymetricCryptoProvider) = function
    | "Registered", data -> data |> Serialization.objectFromJToken<CmdArgs.Register> |> decryptRegister cryptoProvider |> Registered
    | "ActivationKeyRegenerated", data -> data |> Serialization.objectFromJToken<CmdArgs.RegenerateActivationKey> |> ActivationKeyRegenerated
    | "Activated", data -> data |> Serialization.objectFromJToken<CmdArgs.Activate> |> Activated
    | "CreditsAdded", data -> data |> Serialization.objectFromJToken<CmdArgs.AddCredits> |> CreditsAdded
    | n,_ -> failwithf "Unrecognized event %s" n

let toData (cryptoProvider:SymetricCryptoProvider) = function
    | Registered args -> "Registered", (args |> encryptRegister cryptoProvider |> Serialization.objectToJToken)
    | ActivationKeyRegenerated args -> "ActivationKeyRegenerated", (args |> Serialization.objectToJToken)
    | Activated args -> "Activated", (args |> Serialization.objectToJToken)
    | CreditsAdded args -> "CreditsAdded", (args |> Serialization.objectToJToken)