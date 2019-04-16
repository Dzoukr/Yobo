open System
open CosmoStore
open CosmoStore.TableStorage
open FSharp.Control.Tasks.V2
open Newtonsoft.Json
open Microsoft.FSharpLu.Json
open Microsoft.FSharp.Reflection
open Newtonsoft.Json.Serialization
open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Table
open Microsoft.WindowsAzure.Storage.Table

type MySettings =
    static member formatting = Formatting.None //Microsoft.FSharpLu.Json.Compact.TupleAsArraySettings.formatting
    static member settings =
        let settings = Compact.TupleAsArraySettings.settings
        settings.DateTimeZoneHandling <- DateTimeZoneHandling.Utc
        settings.MissingMemberHandling <- MissingMemberHandling.Ignore
        settings.ContractResolver <- CamelCasePropertyNamesContractResolver()
        settings

type Serializer = With<MySettings>

type KeyRow() =
    inherit TableEntity()
    member val key = "" with get, set    
    member val vector = "" with get, set    
    

let getKeys tableName (conf:Configuration) =
    
    let account = 
        match conf.Account with
        | Cloud (accountName, authKey) -> 
            let credentials = Auth.StorageCredentials(accountName, authKey)
            CloudStorageAccount(credentials, true)
        | LocalEmulator -> CloudStorageAccount.DevelopmentStorageAccount

    let client = account.CreateCloudTableClient()
    let table = client.GetTableReference(tableName)

    task {
        let token = null
        let! operation = table.ExecuteQuerySegmentedAsync((new TableQuery<KeyRow>()), token)
        return operation.Results |> Seq.toList
    }


[<EntryPoint>]
let main argv =
    let folder = "./backup/"
    let keysTable,config =
        match argv.Length with
        | 1 -> argv.[0], Configuration.CreateDefaultForLocalEmulator()
        | _ -> argv.[0], Configuration.CreateDefault argv.[1] argv.[2]

    let eventStore = config |> EventStore.getEventStore

    let backupStream (es:EventStore) (s:Stream) =
        task {
            let! events = es.GetEvents s.Id EventsReadRange.AllEvents
            IO.Directory.CreateDirectory folder |> ignore
            let filename = sprintf "%s/%s.json" folder s.Id
            do! 
                events 
                |> Serializer.serialize 
                |> (fun x -> IO.File.WriteAllTextAsync(filename, x))
        }

    task {
        let! streams = eventStore.GetStreams StreamsReadFilter.AllStreams
        for stream in streams do
            do! stream |> backupStream eventStore

        let! keys = getKeys keysTable config
        keys |> Serializer.serializeToFile (sprintf "%s/__keys.json" folder)
    }
    |> Async.AwaitTask
    |> Async.RunSynchronously
    0 // return an integer exit code
