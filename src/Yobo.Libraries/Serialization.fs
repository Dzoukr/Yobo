module Yobo.Libraries.Serialization

open System.Collections.Concurrent
open System.Collections.Generic
open System.Globalization
open System.IO
open Newtonsoft.Json
open Microsoft.FSharp.Reflection
open Newtonsoft.Json.Converters
open Newtonsoft.Json.Serialization
open System.Reflection

module Converters =
    
    module Helpers =
        
        let inline stringEq (a:string) (b:string) =
            a.Equals(b, System.StringComparison.OrdinalIgnoreCase)

        let inline isOptionType (t:System.Type) =
           t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() = typedefof<option<_>>

        let inline isTupleType (t:System.Type) =
           FSharpType.IsTuple t

        let inline isTupleItemProperty (prop:System.Reflection.PropertyInfo) =
            // Item1, Item2, etc. excluding Items[n] indexer. Valid only on tuple types.
            (prop.Name.StartsWith("Item") || prop.Name = "Rest") && (Seq.isEmpty <| prop.GetIndexParameters())

    module Memoized =
        let inline memoize (f: 'key -> 'result) =
            let d = ConcurrentDictionary<'key, 'result>()
            fun key -> d.GetOrAdd(key, f)

        let getUnionCaseFields = memoize FSharpValue.PreComputeUnionReader
        let getUnionTag = memoize FSharpValue.PreComputeUnionTagReader
        let getUnionCasesByTag = memoize (fun t -> FSharpType.GetUnionCases(t) |> Array.map (fun x -> x.Tag, x) |> dict)
        let getUnionCases = memoize FSharpType.GetUnionCases

        let constructUnionCase = memoize FSharpValue.PreComputeUnionConstructor

        let getUnionCaseProperyInfoFields = memoize (fun (case: UnionCaseInfo) -> case.GetFields())

        let findNoFieldsMatchingUnionCaseByNameAndType  =
            memoize <| fun (objectType, caseName) ->
                let cases = getUnionCases objectType
                cases |> Array.tryFind (fun case -> Helpers.stringEq case.Name caseName && (getUnionCaseProperyInfoFields case |> Array.isEmpty))

        let findMatchingUnionCaseByNameAndType  =
            memoize <| fun (objectType, caseName) ->
                let cases = getUnionCases objectType
                cases |> Array.tryFind (fun case -> Helpers.stringEq case.Name caseName)

        let getUnionTagOfValue v =
            let t = v.GetType()
            getUnionTag t v

        let inline getUnionFields v =
            let cases = getUnionCasesByTag (v.GetType())
            let tag = getUnionTagOfValue v
            let case = cases.[tag]
            let unionReader = getUnionCaseFields case
            (case, unionReader v)

        let SomeFieldIdentifier = "Some"

        /// Determine if a given type has a field named 'Some' which would cause
        /// ambiguity if nested under an option type without being boxed
        let hasFieldNamedSome =
            memoize
                (fun (t:System.Type) ->
                    Helpers.isOptionType t // the option type itself has a 'Some' field
                    || (FSharpType.IsRecord t && FSharpType.GetRecordFields t |> Seq.exists (fun r -> Helpers.stringEq r.Name SomeFieldIdentifier))
                    || (FSharpType.IsUnion t && FSharpType.GetUnionCases t |> Seq.exists (fun r -> Helpers.stringEq r.Name SomeFieldIdentifier)))
    
    type CompactUnionJsonConverter(?tupleAsHeterogeneousArray:bool, ?usePropertyFormatterForValues:bool) =
        inherit Newtonsoft.Json.JsonConverter()

        ///  By default tuples are serialized as heterogeneous arrays.
        let tupleAsHeterogeneousArray = defaultArg tupleAsHeterogeneousArray true
        ///  By default formatting is used for values
        let usePropertyFormatterForValues = defaultArg usePropertyFormatterForValues true
        let canConvertMemorised =
            Memoized.memoize
                (fun objectType ->
                    (   // Include F# discriminated unions
                        FSharpType.IsUnion objectType
                        // and exclude the standard FSharp lists (which are implemented as discriminated unions)
                        && not (objectType.GetTypeInfo().IsGenericType && objectType.GetGenericTypeDefinition() = typedefof<_ list>)
                    )
                    // include tuples
                    || tupleAsHeterogeneousArray && FSharpType.IsTuple objectType
                )

        override __.CanConvert(objectType:System.Type) = canConvertMemorised objectType

        override __.WriteJson(writer:JsonWriter, value:obj, serializer:JsonSerializer) =
            let t = value.GetType()

            let convertName =
                match serializer.ContractResolver with
                | :? DefaultContractResolver as resolver -> resolver.GetResolvedPropertyName
                | _ -> id

            // Option type?
            if Helpers.isOptionType t then
                let cases = Memoized.getUnionCases t
                let none, some = cases.[0], cases.[1]

                let case, fields = Memoized.getUnionFields value

                if case = none then
                    () // None is serialized as just null

                // Some _
                else
                    // Handle cases `Some None` and `Some null`
                    let innerType = (Memoized.getUnionCaseProperyInfoFields some).[0].PropertyType
                    let innerValue = fields.[0]
                    if isNull innerValue then
                        writer.WriteStartObject()
                        writer.WritePropertyName(convertName Memoized.SomeFieldIdentifier)
                        writer.WriteNull()
                        writer.WriteEndObject()
                    // Some v with v <> null && v <> None
                    else
                        // Is it nesting another option: `(e.g., "Some (Some ... Some ( ... )))"`
                        // or any other type with a field named 'Some'?
                        if Memoized.hasFieldNamedSome innerType then
                            // Preserved the nested structure through boxing
                            writer.WriteStartObject()
                            writer.WritePropertyName(convertName Memoized.SomeFieldIdentifier)
                            serializer.Serialize(writer, innerValue)
                            writer.WriteEndObject()
                        else
                            // Type is option<'a> where 'a does not have a field named 'Some
                            // (and therfore in particular is NOT an option type itself)
                            // => we can simplify the Json by omitting the `Some` boxing
                            // and serializing the nested object directly
                            serializer.Serialize(writer, innerValue)
            // Tuple
            else if tupleAsHeterogeneousArray && Helpers.isTupleType t then
                let v = FSharpValue.GetTupleFields value
                serializer.Serialize(writer, v)
            // Discriminated union
            else
                let case, fields = Memoized.getUnionFields value

                match fields with
                // Field-less union case
                | [||] when usePropertyFormatterForValues -> writer.WriteValue(convertName case.Name)
                | [||] when not usePropertyFormatterForValues -> writer.WriteValue(case.Name)
                // Case with single field
                | [|onefield|] ->
                    writer.WriteStartObject()
                    writer.WritePropertyName(convertName case.Name)
                    serializer.Serialize(writer, onefield)
                    writer.WriteEndObject()
                // Case with list of fields
                | _ ->
                    writer.WriteStartObject()
                    writer.WritePropertyName(convertName case.Name)
                    serializer.Serialize(writer, fields)
                    writer.WriteEndObject()

        override __.ReadJson(reader:JsonReader, objectType:System.Type, existingValue:obj, serializer:JsonSerializer) =
            let failreadwith s = raise (JsonReaderException s)
            let failreadwithf format = Printf.ksprintf failreadwith format
            // Option type?
            if Helpers.isOptionType objectType then
                let cases = Memoized.getUnionCases objectType
                let caseNone, caseSome = cases.[0], cases.[1]
                let jToken = Linq.JToken.ReadFrom(reader)

                // Json Null maps to `None`
                if jToken.Type = Linq.JTokenType.Null then
                    FSharpValue.MakeUnion(caseNone, [||])

                // Json that is not null must map to `Some _`
                else
                    let nestedType = objectType.GetTypeInfo().GetGenericArguments().[0]

                    // Try to retrieve the 'Some' attribute:
                    // if the specified Json an object of the form `{ "Some" = token }`
                    // then return `Some token`, otherwise returns `None`.
                    let tryGetSomeAttributeValue (jToken:Linq.JToken) =
                        if jToken.Type = Linq.JTokenType.Object then
                            let jObject = jToken :?> Linq.JObject
                            match jObject.TryGetValue (Memoized.SomeFieldIdentifier, System.StringComparison.OrdinalIgnoreCase) with
                            | true, token -> Some token
                            | false, _ -> None
                        else
                            None

                    let nestedValue =
                        match tryGetSomeAttributeValue jToken with
                        | Some someAttributeValue when someAttributeValue.Type = Linq.JTokenType.Null ->
                            // The Json object is { "Some" : null } for type option<'a>
                            // where 'a is nullable => deserialized to `Some null`
                            null

                        | Some someAttributeValue when Memoized.hasFieldNamedSome nestedType ->
                            // Case of Json { "Some" : <obj> } where <obj> is not null
                            // => we just deserialize the nested object recursively
                            someAttributeValue.ToObject(nestedType, serializer)

                        | Some someAttributeValue ->
                            failreadwithf "Unexpected 'Some' Json attribute. Attribute value: %O" someAttributeValue

                        | None when Memoized.hasFieldNamedSome nestedType ->
                            failreadwith "Types with a field named 'Some' and nested under an option type must be boxed under a 'Some' attribute when serialized to Json."

                        | None ->
                            // type is option<'a> where 'a is not an option type and not a
                            // type that would be serialized as a Json object.
                            // i.e. 'a is either a base Json type (e.g. integer or string) or
                            // a Json array.
                            // This means that the Json is not boxed under the `Some` attribute and we can therefore
                            // deserialize the object of type 'a directly without performing any unboxing.
                            jToken.ToObject(nestedType, serializer)

                    Memoized.constructUnionCase caseSome [| nestedValue |]

            // Tuple type?
            else if tupleAsHeterogeneousArray && Helpers.isTupleType objectType then
                match reader.TokenType with
                // JSON is an object with one field per element of the tuple
                | JsonToken.StartObject ->
                    // backward-compat with legacy tuple serialization:
                    // if reader.TokenType is StartObject then we should expecte legacy JSON format for tuples
                    let jToken = Linq.JObject.Load(reader)
                    if isNull jToken then
                        failreadwithf "Expecting a legacy tuple, got null"
                    else
                        let readProperty (prop: PropertyInfo) =
                            match jToken.TryGetValue(prop.Name) with
                            | false,_ ->
                                failreadwithf "Cannot parse legacy tuple value: %O. Missing property: %s" jToken prop.Name
                            | true, jsonProp ->
                                jsonProp.ToObject(prop.PropertyType, serializer)
                        let tupleValues =
                            objectType.GetTypeInfo().DeclaredProperties
                            |> Seq.filter Helpers.isTupleItemProperty
                            |> Seq.map readProperty
                            |> Array.ofSeq
                        System.Activator.CreateInstance(objectType, tupleValues)

                // JSON is an heterogeneous array
                | JsonToken.StartArray ->
                    let tupleType = objectType
                    let elementTypes = FSharpType.GetTupleElements(tupleType)

                    let readElement elementType =
                        let more = reader.Read()
                        if not more then
                            failreadwith "Missing array element in deserialized JSON"

                        let jToken = Linq.JToken.ReadFrom(reader)
                        jToken.ToObject(elementType, serializer)

                    let deserializedAsUntypedArray =
                        elementTypes
                        |> Array.map readElement

                    let more = reader.Read()
                    if reader.TokenType <> JsonToken.EndArray then
                        failreadwith "Expecting end of array token in deserialized JSON"

                    FSharpValue.MakeTuple(deserializedAsUntypedArray, tupleType)
                | _ ->
                    failreadwithf "Expecting a JSON array or a JSON object, got something else: %A" reader.TokenType
            // Discriminated union
            else
                // There are three types of union cases:
                //      | Case1 | Case2 of 'a | Case3 of 'a1 * 'a2 ... * 'an
                // Those are respectively serialized to Json as
                //    "Case1"
                //    { "Case2" : value }
                //    { "Case3" : [v1, v2, ... vn] }

                // Load JObject from stream
                let jToken = Linq.JToken.Load(reader)

                if isNull jToken then
                    null

                // Type1: field-less union case
                elif jToken.Type = Linq.JTokenType.String then
                    let caseName = jToken.ToString()
                    let matchingCase = Memoized.findNoFieldsMatchingUnionCaseByNameAndType (objectType, caseName)
                    match matchingCase with
                    | Some case -> Memoized.constructUnionCase case [||]
                    | None ->
                        let cases = Memoized.getUnionCases objectType
                        failreadwithf "Cannot parse DU field-less value: %O. Expected names: %O" caseName (System.String.Join(", ", cases |> Seq.map(fun c->c.Name)))

                // Type 2 or 3: Case with fields
                elif jToken.Type = Linq.JTokenType.Object then
                    let jObject = jToken :?> Linq.JObject
                    let jObjectProperties = jObject.Properties()
                    if Seq.length jObjectProperties <> 1 then
                        failreadwith "Incorrect Json format for discriminated union. A DU value with fields must be serialized to a Json object with a single Json attribute"

                    let caseProperty = jObjectProperties |> Seq.head
                    /// Lookup the DU case by name
                    let matchingCase = Memoized.findMatchingUnionCaseByNameAndType (objectType, caseProperty.Name)

                    match matchingCase with
                    | None ->
                        failreadwithf "Case with fields '%s' does not exist for discriminated union %s" caseProperty.Name objectType.Name
                    | Some case  ->
                        let propertyInfosForCase = Memoized.getUnionCaseProperyInfoFields case
                        // Type 2: A union case with a single field: Case2 of 'a
                        if propertyInfosForCase.Length = 1 then
                            let fieldType = propertyInfosForCase.[0].PropertyType
                            let field = caseProperty.Value.ToObject(fieldType, serializer)
                            Memoized.constructUnionCase case [|field|]
                        // Type 3: A union case with more than one field: Case3 of 'a1 * 'a2 ... * 'an
                        else
                            // Here there could be an ambiguity:
                            // the Json values are either the fields of the case
                            // or if the array is Use target type to resolve ambiguity
                            let fields =
                                propertyInfosForCase
                                |> Seq.zip caseProperty.Value
                                |> Seq.map (fun (v,t) -> v.ToObject(t.PropertyType, serializer))
                                |> Seq.toArray
                            Memoized.constructUnionCase case fields
                else
                    failreadwithf "Unexpected Json token type %O: %O" jToken.Type jToken

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

module Resolvers =
    type RequireAllPropertiesContractResolver() =
        inherit CamelCasePropertyNamesContractResolver()
        
        override __.CreateProperty(memb, serialization) =
            let prop = base.CreateProperty(memb, serialization)
            let isRequired = not (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() = typedefof<option<_>>)
            if isRequired then prop.Required <- Required.Always
            prop

module Settings =
    let formatting = Formatting.None
    let settings = 
        let settings = JsonSerializerSettings()
        let converters = ResizeArray<JsonConverter>()
        converters.Add(IsoDateTimeConverter(DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffffffZ", DateTimeStyles = DateTimeStyles.AdjustToUniversal))
        converters.Add(Converters.SingleCaseUnionConverter())
        converters.Add(Converters.CompactUnionJsonConverter(true, true))
        settings.Converters |> Seq.iter converters.Add
        settings.MissingMemberHandling <- MissingMemberHandling.Ignore
        settings.NullValueHandling <- NullValueHandling.Ignore
        settings.DateTimeZoneHandling <- DateTimeZoneHandling.Utc
        settings.ContractResolver <- Resolvers.RequireAllPropertiesContractResolver()
        settings.Converters <- converters
        settings
    
    let jsonSerializer =
        let js = JsonSerializer.Create(settings)
        js.Formatting <- formatting
        js

[<RequireQualifiedAccess>]
module Serializer =
    let serialize (o:obj) = JsonConvert.SerializeObject(o, Settings.formatting, Settings.settings)

    let serializeToUtf8Bytes : obj -> byte[] = serialize >> System.Text.Encoding.UTF8.GetBytes

    let serializeToStream (obj:obj) (str:Stream) =
        use writer = new StreamWriter(str, System.Text.UTF8Encoding.UTF8, 1024, true)
        use jsonWriter = new JsonTextWriter(writer)
        Settings.jsonSerializer.Serialize(jsonWriter, obj)
        
    let deserialize<'a> json = JsonConvert.DeserializeObject<'a>(json, Settings.settings)
    let deserializeFromStream<'a> (str:Stream) =
        use streamReader = new StreamReader(str)
        use textReader = new JsonTextReader(streamReader)
        Settings.jsonSerializer.Deserialize<'a>(textReader)