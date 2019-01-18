[<RequireQualifiedAccess>]
module Yobo.Core.Serialization

open System
open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open Microsoft.FSharp.Reflection
open Newtonsoft.Json.Linq

type OptionConverter() =
    inherit JsonConverter()
    
    override __.CanConvert(t) = 
        t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<option<_>>

    override __.WriteJson(writer, value, serializer) =
        let value = 
            if isNull value then null
            else 
                let _,fields = FSharpValue.GetUnionFields(value, value.GetType())
                fields.[0]  
        serializer.Serialize(writer, value)

    override __.ReadJson(reader, t, _, serializer) =        
        let innerType = t.GetGenericArguments().[0]
        let innerType = 
            if innerType.IsValueType then (typedefof<Nullable<_>>).MakeGenericType([|innerType|])
            else innerType        
        let value = serializer.Deserialize(reader, innerType)
        let cases = FSharpType.GetUnionCases(t)
        if isNull value then FSharpValue.MakeUnion(cases.[0], [||])
        else FSharpValue.MakeUnion(cases.[1], [|value|])

type RequireAllPropertiesContractResolver() =
    inherit CamelCasePropertyNamesContractResolver()
    
    override __.CreateProperty(memb, serialization) =
        let prop = base.CreateProperty(memb, serialization)
        let isRequired = not (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() = typedefof<option<_>>)
        if isRequired then prop.Required <- Required.Always
        prop

// settings
let private settings = JsonSerializerSettings()
settings.DateTimeZoneHandling <- DateTimeZoneHandling.Utc
settings.NullValueHandling <- NullValueHandling.Ignore
settings.ContractResolver <- RequireAllPropertiesContractResolver()
settings.Converters.Add(OptionConverter())
settings.Converters.Add(Newtonsoft.Json.Converters.StringEnumConverter())

let serialize obj = JsonConvert.SerializeObject(obj, settings)
let deserialize<'a> json = JsonConvert.DeserializeObject<'a>(json, settings)
let objectFromJToken<'a> (token:JToken) = token.ToString() |> deserialize<'a>
let objectToJToken obj = obj |> serialize |> JToken.Parse