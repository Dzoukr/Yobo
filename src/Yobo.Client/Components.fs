module Yobo.Client.Components

open System
open Fulma

module Calendar =
    open Fable.Core
    open Fable.Core.JsInterop

    [<Emit("bulmaCalendar.attach($0, $1)[0].on('date:selected', date => { $2(date) });")>]
    let private attachCalendarScript (selector: string) (opts:obj) (fn:obj -> unit) : unit = jsNative

    let private jsFormat = "YYYY-MM-DD"
    let private netFormat = "yyyy-MM-dd"

    type DisplayMode =
        | Default
        | Dialog
        | Inline
        
    type Options = {
        StartDate : DateTime option
        EndDate : DateTime option
        WeekStart : int
        DisplayMode : DisplayMode
        Lang : string
        IsRange : bool
        MinimumDate : DateTime option
        MaximumDate : DateTime option
    }
    with
        static member Default = {
            StartDate = None
            EndDate = None
            WeekStart = 0
            DisplayMode = Default
            Lang = "en"
            IsRange = false
            MinimumDate = None
            MaximumDate = None
        }
        member this.ToJsOpts() = jsOptions(fun x ->
            x?dateFormat <- jsFormat
            x?weekStart <- this.WeekStart
            x?lang <- this.Lang
            x?displayMode <- this.DisplayMode |> string |> fun x -> x.ToLower()
            x?isRange <- this.IsRange
            if this.StartDate.IsSome then
                x?startDate <- this.StartDate.Value.ToString(netFormat)
            if this.EndDate.IsSome then
                x?endDate <- this.EndDate.Value.ToString(netFormat)
            if this.MinimumDate.IsSome then
                x?minDate <- this.MinimumDate.Value.ToString(netFormat)
            if this.MaximumDate.IsSome then
                x?maxDate <- this.MaximumDate.Value.ToString(netFormat)
        )

    let private attachCalendar (opts:Options) (onDateChange: DateTime option * DateTime option -> unit) (elm: Fable.Import.Browser.Element) =
        let tryToDateTime obj =
            match obj with
            | null -> None
            | x -> x?toISOString() |> DateTime.Parse |> Some

        if elm |> isNull |> not then
            let selector = sprintf "[id=\"%s\"]" elm.id
            if elm.nextSibling |> isNull then
                attachCalendarScript selector (opts.ToJsOpts()) (fun d ->
                    let s = d?start |> tryToDateTime
                    let e = d?``end`` |> tryToDateTime
                    onDateChange (s, e)
                )

    let view (opts:Options) (componentId:string) (onDateChange: DateTime option * DateTime option -> unit) =
        let dValue (st:DateTime option) (en:DateTime option) =
            (match st, en with
            | Some s, Some e -> sprintf "%s - %s" (s.ToString(netFormat)) (e.ToString(netFormat))
            | Some s, _ -> s.ToString(netFormat)
            | _ -> "") 
            |> Input.Option.DefaultValue
        
        Input.date [
            if opts.IsRange then yield dValue opts.StartDate opts.EndDate
            else yield dValue opts.StartDate None
            yield Input.Option.Ref(attachCalendar opts onDateChange)
            yield Input.Option.Id componentId
        ]
    
    let viewDefault = view Options.Default