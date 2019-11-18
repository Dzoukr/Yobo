#r "paket: groupref build //"
#load "./.fake/build.fsx/intellisense.fsx"

#if !FAKE
#r "netstandard"
#r "Facades/netstandard" // https://github.com/ionide/ionide-vscode-fsharp/issues/839#issuecomment-396296095
#endif

open System

open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators

let functionAppPath = Path.getFullName "./src/Yobo.FunctionApp"
let functionAppWatcherPath = Path.getFullName "./src/Yobo.FunctionApp.Local"
let functionAppDeployPath = Path.getFullName "./deploy/functionApp"
let functionAppCorePath = Path.getFullName "./src/Yobo.Core"
let clientOutputPath = Path.getFullName "./src/Yobo.Client/output"
let clientDeployPath = Path.getFullName "./deploy/client"
let migrationsPath = Path.getFullName "./tools/DbMigrations"
let migrationsDeployPath = Path.getFullName "./deploy/dbMigrations"


let platformTool tool winTool =
    let tool = if Environment.isUnix then tool else winTool
    match ProcessUtils.tryFindFileOnPath tool with
    | Some t -> t
    | _ ->
        let errorMsg =
            tool + " was not found in path. " +
            "Please install it and make sure it's available from your path. " +
            "See https://safe-stack.github.io/docs/quickstart/#install-pre-requisites for more info"
        failwith errorMsg

let nodeTool = platformTool "node" "node.exe"
let yarnTool = platformTool "yarn" "yarn.cmd"

let runTool cmd args workingDir =
    let arguments = args |> String.split ' ' |> Arguments.OfArgs
    Command.RawCommand (cmd, arguments)
    |> CreateProcess.fromCommand
    |> CreateProcess.withWorkingDirectory workingDir
    |> CreateProcess.ensureExitCode
    |> Proc.run
    |> ignore

let runDotNet cmd workingDir =
    let result =
        DotNet.exec (DotNet.Options.withWorkingDirectory workingDir) cmd ""
    if result.ExitCode <> 0 then failwithf "'dotnet %s' failed in %s" cmd workingDir

Target.create "CleanClient" (fun _ ->
    [ clientOutputPath; clientDeployPath ] |> Shell.cleanDirs
)

Target.create "CleanFunctionApp" (fun _ ->
    [ functionAppDeployPath ] |> Shell.cleanDirs
)

Target.create "InstallClient" (fun _ ->
    printfn "Node version:"
    runTool nodeTool "--version" __SOURCE_DIRECTORY__
    printfn "Yarn version:"
    runTool yarnTool "--version" __SOURCE_DIRECTORY__
    runTool yarnTool "install --frozen-lockfile" __SOURCE_DIRECTORY__
)

Target.create "Run" (fun _ ->
    let server = async {
        runDotNet "watch run" functionAppWatcherPath
    }
    let client = async {
        runTool yarnTool "webpack-dev-server" __SOURCE_DIRECTORY__
    }

    [client;server]
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore
)

Target.create "RunDbMigrations" (fun _ ->
    let connString = "..\Yobo.Private\ConnectionString.txt" |> File.readAsString
    let cmd = sprintf "DbMigrations.dll \"%s\"" connString
    runDotNet cmd migrationsDeployPath
)

Target.create "RefreshSchema" (fun _ ->
    let srcFile = "..\Yobo.Private\ReadDb.fs"
    let original = functionAppCorePath + "\ReadDb.fs"
    let schemaFile = ".\database\yobo.schema"

    schemaFile |> File.delete
    let backup = original |> File.readAsString
    srcFile |> File.readAsString |> File.replaceContent original
    runDotNet (sprintf "build %s" functionAppCorePath) "."
    backup |> File.replaceContent original
)

Target.create "PublishDbMigrations" (fun  _ ->
    let publishArgs = sprintf "publish -c Release -o \"%s\"" migrationsDeployPath
    runDotNet publishArgs migrationsPath
    !! "./database/*.sql" |> Shell.copyFiles (migrationsDeployPath + "\scripts")
)

Target.create "PublishFunctionApp" (fun _ ->
    let publishArgs = sprintf "publish -c Release -o \"%s\"" functionAppDeployPath
    runDotNet publishArgs functionAppPath
)

Target.create "PublishClient" (fun _ ->
    runTool yarnTool "webpack-cli -p" __SOURCE_DIRECTORY__
    Shell.copyDir clientDeployPath clientOutputPath FileFilter.allFiles
)

Target.create "Publish" (fun _ ->
    ["PublishClient";"PublishFunctionApp"] |> List.iter (fun t -> Target.run 1 t [])
)

open Fake.Core.TargetOperators

"CleanClient" ==> "InstallClient" ==> "PublishClient"
"CleanFunctionApp" ==> "PublishDbMigrations" ==> "PublishFunctionApp"
"PublishDbMigrations" ==> "RunDbMigrations" ==> "RefreshSchema"

Target.runOrDefaultWithArguments "Run"
