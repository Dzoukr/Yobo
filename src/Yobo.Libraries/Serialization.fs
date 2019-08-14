module Yobo.Libraries.Serialization

open Newtonsoft.Json
open Microsoft.FSharpLu.Json
open Microsoft.FSharp.Reflection
open Newtonsoft.Json.Serialization

type SingleCaseUnionConverter () =
    inherit JsonConverter()
    override __.CanConvert t =
        let isList = t.IsGenericType && typedefof<List<_>>.Equals (t.GetGenericTypeDefinition())
        let isUnion = FSharpType.IsUnion t || (t.DeclaringType <> null && FSharpType.IsUnion (t.DeclaringType))
        
        if (not isList) && isUnion then
            let cases = FSharpType.GetUnionCases(t)
            cases.Length = 1 && cases.[0].Name = cases.[0].DeclaringType.Name
        else false

    override __.WriteJson(writer, value, serializer) =
        let value = 
            if isNull value then null
            else 
                let _,fields = FSharpValue.GetUnionFields(value, value.GetType())
                fields.[0]  
        serializer.Serialize(writer, value)

    override __.ReadJson(reader, t, _, serializer) =        
        let case = FSharpType.GetUnionCases(t).[0]
        let field = case.GetFields().[0]
        let value = serializer.Deserialize(reader, field.PropertyType)
        FSharpValue.MakeUnion(case, [|value|])

type CustomSettings =
    static member formatting = Formatting.None //Microsoft.FSharpLu.Json.Compact.TupleAsArraySettings.formatting
    static member settings =
        let settings = Compact.TupleAsArraySettings.settings
        let converters = ResizeArray<JsonConverter>()
        converters.Add(SingleCaseUnionConverter())
        settings.Converters |> Seq.iter converters.Add

        settings.DateTimeZoneHandling <- DateTimeZoneHandling.Utc
        settings.MissingMemberHandling <- MissingMemberHandling.Ignore
        settings.ContractResolver <- CamelCasePropertyNamesContractResolver()
        settings.Converters <- converters
        settings

type Serializer = With<CustomSettings>