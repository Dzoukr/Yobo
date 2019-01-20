module Yobo.Libraries.Security.SymetricCrypto

open System.IO
open System.Security.Cryptography

let generateKeyAndVector () =
    use aesAlg = Aes.Create()
    let key = aesAlg.Key
    let iv = aesAlg.IV
    (key |> System.Convert.ToBase64String), (iv |> System.Convert.ToBase64String)

let toBase64String (t:string) =
    let plainTextBytes = System.Text.Encoding.UTF8.GetBytes(t)
    plainTextBytes |> System.Convert.ToBase64String

let fromBase64String (t:string) =
    let base64EncodedBytes = System.Convert.FromBase64String(t)
    System.Text.Encoding.UTF8.GetString(base64EncodedBytes)

let encrypt key iv (text:string) =
    let safeText = text |> toBase64String 
    use aesAlg = Aes.Create()
    aesAlg.Key <- (key |> System.Convert.FromBase64String)
    aesAlg.IV <- (iv |> System.Convert.FromBase64String)
    use encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV)
    use input = new MemoryStream(safeText.ToCharArray() |> Array.map System.Convert.ToByte)
    use csEncrypt = new CryptoStream(input, encryptor, CryptoStreamMode.Read)
    use msEncrypt = new MemoryStream()
    csEncrypt.CopyTo(msEncrypt)
    msEncrypt.ToArray() |> System.Convert.ToBase64String

let decrypt key iv (text:string) =
    use aesAlg = Aes.Create()
    aesAlg.Key <- (key |> System.Convert.FromBase64String)
    aesAlg.IV <- (iv |> System.Convert.FromBase64String)
    let text = text |> System.Convert.FromBase64String
    use decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV)
    use input = new MemoryStream(text)
    use csDecrypt = new CryptoStream(input, decryptor, CryptoStreamMode.Read)
    use msDecrypt = new MemoryStream()
    csDecrypt.CopyTo(msDecrypt)
    msDecrypt.ToArray() |> System.Text.Encoding.UTF8.GetString |> fromBase64String