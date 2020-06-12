open Fake.IO

#r "paket: groupref Build //"
#load ".fake/build.fsx/intellisense.fsx"

open System.IO
open Fake.Core
open Fake.DotNet
open Fake.IO.FileSystemOperators
open Fake.Core.TargetOperators

module Tools =
    let private findTool tool winTool =
        let tool = if Environment.isUnix then tool else winTool
        match ProcessUtils.tryFindFileOnPath tool with
        | Some t -> t
        | _ ->
            let errorMsg =
                tool + " was not found in path. " +
                "Please install it and make sure it's available from your path. "
            failwith errorMsg
            
    let private runTool (cmd:string) args workingDir =
        let arguments = args |> String.split ' ' |> Arguments.OfArgs
        Command.RawCommand (cmd, arguments)
        |> CreateProcess.fromCommand
        |> CreateProcess.withWorkingDirectory workingDir
        |> CreateProcess.ensureExitCode
        |> Proc.run
        |> ignore
        
    let dotnet cmd workingDir =
        let result =
            DotNet.exec (DotNet.Options.withWorkingDirectory workingDir) cmd ""
        if result.ExitCode <> 0 then failwithf "'dotnet %s' failed in %s" cmd workingDir
    
    let femto = runTool "femto"        
    let node = runTool (findTool "node" "node.exe")        
    let yarn = runTool (findTool "yarn" "yarn.cmd")             

let publishDir = Path.getFullName "deploy"
let srcDir = Path.getFullName "src"
let toolsDir = Path.getFullName "tools"

let clientSrcPath = srcDir </> "Yobo.Client"
let serverWatcherPath = srcDir </> "Yobo.Server.Local"
let clientDeployPath = publishDir </> "client"
let serverSrcPath = srcDir </> "Yobo.Server"
let serverDeployPath = publishDir </> "server"
let migrationsSrcPath = toolsDir </> "DbMigrations"
let migrationsDeployPath = publishDir </> "dbMigrations"

// Targets
let clean proj = [ proj </> "bin"; proj </> "obj" ] |> Shell.cleanDirs


Target.create "InstallClient" (fun _ ->
    printfn "Node version:"
    Tools.node "--version" clientSrcPath
    printfn "Yarn version:"
    Tools.yarn "--version" clientSrcPath
    Tools.yarn "install --frozen-lockfile" clientSrcPath
)

Target.create "PublishClient" (fun _ ->
    let clientDeployLocalPath = (clientSrcPath </> "deploy")
    [ clientDeployPath; clientDeployLocalPath] |> Shell.cleanDirs
    Tools.yarn "webpack-cli -p" __SOURCE_DIRECTORY__
    Shell.copyDir clientDeployPath clientDeployLocalPath FileFilter.allFiles
)

Target.create "PublishServer" (fun _ ->
    [ serverDeployPath ] |> Shell.cleanDirs
    let publishArgs = sprintf "publish -c Release -o \"%s\"" serverDeployPath
    Tools.dotnet publishArgs serverSrcPath
)

Target.create "Run" (fun _ ->
    let server = async {
        Tools.dotnet "watch run" serverWatcherPath
    }
    let client = async {
        Tools.yarn "webpack-dev-server" clientSrcPath
    }
    [server;client]
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore
)

open Fake.IO.Globbing.Operators

Target.create "PublishDbMigrations" (fun  _ ->
    let publishArgs = sprintf "publish -c Release -o \"%s\"" migrationsDeployPath
    Tools.dotnet publishArgs migrationsSrcPath
    !! "./database/*.sql" |> Shell.copyFiles migrationsDeployPath
)

Target.create "RunDbMigrations" (fun _ ->
    let connString = "..\Yobo.Private\ConnectionString.txt" |> File.readAsString
    let cmd = sprintf "run \"%s\" \"%s\" " connString (migrationsDeployPath |> Path.getFullName)
    Tools.dotnet cmd migrationsSrcPath
)

"InstallClient" ==> "Run"
"InstallClient" ==> "PublishClient"
"PublishDbMigrations" ==> "PublishServer"

Target.runOrDefaultWithArguments "Run"