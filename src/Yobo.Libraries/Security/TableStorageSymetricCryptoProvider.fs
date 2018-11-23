module Yobo.Libraries.Security.TableStorageSymetricCryptoProvider

open System
open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Table
open FSharp.Control.Tasks.V2
open SymetricCryptoProvider

type StorageAccount =
    | Cloud of accountName:string * authKey:string
    | LocalEmulator

type Configuration = {
    TableName : string
    Account : StorageAccount
}

let private rowKey = "hash"
let private keyKey = "key"
let private vectorKey = "vector"

let private setupKeyAndVector (table:CloudTable) (userId:Guid) = 
    let key,vector = SymetricCrypto.generateKeyAndVector()
    task {
        let entity = DynamicTableEntity(userId.ToString(),rowKey)
        entity.Properties.Add(keyKey, EntityProperty.GeneratePropertyForString(key))
        entity.Properties.Add(vectorKey, EntityProperty.GeneratePropertyForString(vector))
        let operation = TableOperation.Insert(entity)
        let! _ = table.ExecuteAsync(operation)
        return ()
    }

let private getKeyAndVector (table:CloudTable) (userId:Guid) =
    let operation = TableOperation.Retrieve<DynamicTableEntity>(userId.ToString(), rowKey)
    task {
        let! opR = table.ExecuteAsync(operation)
        let entity = opR.Result :?> DynamicTableEntity
        let key = entity.Properties.[keyKey].StringValue
        let vector = entity.Properties.[vectorKey].StringValue
        return (key, vector)
    }

let private encrypt (table:CloudTable) (userId:Guid) toEncrypt =
    task {
        let! key, vector = userId |> getKeyAndVector table
        return toEncrypt |> SymetricCrypto.encrypt key vector
    }

let private decrypt (table:CloudTable) (userId:Guid) toDecrypt =
    task {
        let! key, vector = userId |> getKeyAndVector table
        return toDecrypt |> SymetricCrypto.decrypt key vector
    }

let create (conf:Configuration) : SymetricCryptoProvider =
    let account = 
        match conf.Account with
        | Cloud (accountName, authKey) -> 
            let credentials = Auth.StorageCredentials(accountName, authKey)
            CloudStorageAccount(credentials, true)
        | LocalEmulator -> CloudStorageAccount.DevelopmentStorageAccount

    let client = account.CreateCloudTableClient()
    client.GetTableReference(conf.TableName).CreateIfNotExistsAsync() |> Async.AwaitTask |> Async.RunSynchronously |> ignore
    let table = client.GetTableReference(conf.TableName)
    {
        SetupKeyAndVector = setupKeyAndVector table >> Async.AwaitTask >> Async.RunSynchronously
        Encrypt = fun userId -> encrypt table userId >> Async.AwaitTask >> Async.RunSynchronously
        Decrypt = fun userId -> decrypt table userId >> Async.AwaitTask >> Async.RunSynchronously
    }