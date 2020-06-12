module Yobo.Libraries.Tasks

open System.Threading.Tasks
open FSharp.Control.Tasks.V2

[<RequireQualifiedAccess>]
module Task =
    let bind (f : 'a -> Task<'b>) (x : Task<'a>) = task {
        let! x = x
        return! f x
    }

    let map f = bind (f >> Task.FromResult)

    let apply f x =
        bind (fun f' ->
            bind (fun x' -> Task.FromResult(f' x')) x) f

    let result (t:Task<'t>) = 
        t.Result

    let whenAll (ts:Task<_> seq) =
        ts
        |> Array.ofSeq
        |> Task.WhenAll
    
    let ignore (t:Task<_>) = t |> map ignore      
        
module Operators =
    let (<!>) = Task.map
    let (|>>) x f = Task.map f x
    let (<*>) = Task.apply
    let (>>=) x f = Task.bind f x
    let (>=>) f g x = f x >>= g     