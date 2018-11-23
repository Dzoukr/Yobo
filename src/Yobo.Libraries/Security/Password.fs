module Yobo.Libraries.Security.Password

open System
open System.Security.Cryptography;

let SALT_BYTES = 24
let HASH_BYTES = 24
let PBKDF2_ITERATIONS = 64000

let private PBKDF2 password (salt:byte []) iterations outputBytes =
    use pbkdf2 = new Rfc2898DeriveBytes(password, salt)
    pbkdf2.IterationCount <- iterations
    pbkdf2.GetBytes(outputBytes)
    
let createHash password =
    let salt: byte [] = Array.zeroCreate SALT_BYTES
    use csprng = new RNGCryptoServiceProvider()
    csprng.GetBytes(salt)
    
    let hash = PBKDF2 password salt PBKDF2_ITERATIONS HASH_BYTES
    "sha1:" + (string PBKDF2_ITERATIONS) + ":" + (string hash.Length) + ":" + Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash)
           
let verifyPassword password (goodHash:string) =
    let delimiter = [|':'|]
    let split = goodHash.Split(delimiter)
    let salt = Convert.FromBase64String(split.[3])
    let hash = Convert.FromBase64String(split.[4])
    let iterations = Int32.Parse(split.[1])
    let testHash = PBKDF2 password salt iterations hash.Length
    hash = testHash