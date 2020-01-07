module Yobo.Libraries.SimpleDapper

open System.Text

type OrderDirection =
    | Asc
    | Desc

type OrderBy = string * OrderDirection
    
type WhereComparison =
    | Eq of obj
    | Ne of obj
    | Gt of obj
    | Lt of obj
    | Ge of obj
    | Le of obj
    | In of obj list
    | NotIn of obj list
    | IsNull
    | IsNotNull
    
type WhereCombination =
    | And
    | Or

type Where =
    | Empty
    | Column of string * WhereComparison
    | Binary of Where * WhereCombination * Where
    static member (+) (a, b) = Binary(a, And, b)
    static member (*) (a, b) = Binary(a, Or, b)

type Pagination =
    | Skip of skip:int
    | SkipTake of skip:int * take:int

let column name whereComp = Where.Column(name, whereComp)

type SelectQuery = {
    Table : string
    Where : Where
    OrderBy : OrderBy list
    Pagination : Pagination
    IgnoredColumns : string list
}

type InsertQuery = {
    Table : string
    Values : obj list
}

type UpdateQuery = {
    Table : string
    Value : obj
    Where : Where
}

type DeleteQuery = {
    Table : string
    Where : Where
}

module private WhereAnalyzer =
    
    type FieldWhereMetadata = {
        Key : string * WhereComparison
        Name : string
        ParameterName : string
    }
    
    let extractWhereParams (meta:FieldWhereMetadata list) =
        let fn (m:FieldWhereMetadata) =
            match m.Key |> snd with
            | Eq p | Ne p | Gt p
            | Lt p | Ge p | Le p -> (m.ParameterName, p) |> Some
            | In p | NotIn p -> (m.ParameterName, p :> obj) |> Some
            | IsNull | IsNotNull -> None
        meta
        |> List.choose fn

    let rec getWhereMetadata (meta:FieldWhereMetadata list) (w:Where)  =
        match w with
        | Empty -> meta
        | Column (field, comp) ->
            let parName =
                meta
                |> List.filter (fun x -> x.Name = field)
                |> List.length
                |> fun l -> sprintf "Where_%s%i" field (l + 1)
            meta @ [{ Key = (field, comp); Name = field; ParameterName = parName }]
        | Binary(w1, _, w2) -> [w1;w2] |> List.fold getWhereMetadata meta      
    
module private Evaluators =
    
    open System.Linq
    
    let evalCombination = function
        | And -> "AND"
        | Or -> "OR"

    let evalOrderDirection = function
        | Asc -> "ASC"
        | Desc -> "DESC"

    let rec evalWhere (meta:WhereAnalyzer.FieldWhereMetadata list) (w:Where) =
        match w with
        | Empty -> ""
        | Column (field, comp) ->
            let fieldMeta = meta |> List.find (fun x -> x.Key = (field,comp)) 
            let withField op = sprintf "%s %s @%s" fieldMeta.Name op fieldMeta.ParameterName
            match comp with
            | Eq _ -> withField "="
            | Ne _ -> withField "<>"
            | Gt _ -> withField ">"
            | Lt _ -> withField "<"
            | Ge _ -> withField ">="
            | Le _ -> withField "<="
            | In _ -> withField "IN"
            | NotIn _ -> withField "NOT IN"
            | IsNull -> sprintf "%s IS NULL" fieldMeta.Name
            | IsNotNull -> sprintf "%s IS NOT NULL" fieldMeta.Name
        | Binary(w1, comb, w2) ->
            match evalWhere meta w1, evalWhere meta w2 with
            | "", fq | fq , "" -> fq
            | fq1, fq2 -> sprintf "(%s %s %s)" fq1 (evalCombination comb) fq2

    let evalOrderBy (xs:OrderBy list) =
        xs
        |> List.map (fun (n,s) -> sprintf "%s %s" n (evalOrderDirection s))
        |> String.concat ", "

    let evalPagination (pag:Pagination) =
        match pag with
        | Skip x when x <= 0 -> ""
        | Skip o -> sprintf "OFFSET %i ROWS" o
        | SkipTake(o,f) -> sprintf "OFFSET %i ROWS FETCH NEXT %i ROWS ONLY" o f
    
    let evalSelectQuery fields meta (q:SelectQuery) =
        let fieldNames = fields |> List.filter (fun x -> not <| q.IgnoredColumns.Contains(x)) |> String.concat ", "
        // basic query
        let sb = StringBuilder(sprintf "SELECT %s FROM %s" fieldNames q.Table)
        // where
        let where = evalWhere meta q.Where
        if where.Length > 0 then sb.Append (sprintf " WHERE %s" where) |> ignore
        // order by
        let orderBy = evalOrderBy q.OrderBy
        if orderBy.Length > 0 then sb.Append (sprintf " ORDER BY %s" orderBy) |> ignore
        // pagination
        let pagination = evalPagination q.Pagination
        if pagination.Length > 0 then sb.Append (sprintf " %s" pagination) |> ignore    
        sb.ToString()
            
    let evalInsertQuery fields (q:InsertQuery) =
        let fieldNames = fields |> String.concat ", " |> sprintf "(%s)"
        let values = fields |> List.map (sprintf "@%s") |> String.concat ", " |> sprintf "(%s)" 
        sprintf "INSERT INTO %s %s VALUES %s" q.Table fieldNames values        

    let evalUpdateQuery fields meta (q:UpdateQuery) =
        // basic query
        let pairs = fields |> List.map (fun x -> sprintf "%s=@%s" x x) |> String.concat ", "
        let sb = StringBuilder(sprintf "UPDATE %s SET %s" q.Table pairs)
        // where
        let where = evalWhere meta q.Where
        if where.Length > 0 then sb.Append (sprintf " WHERE %s" where) |> ignore
        sb.ToString()

    let evalDeleteQuery meta (q:DeleteQuery) =
        // basic query
        let sb = StringBuilder(sprintf "DELETE FROM %s" q.Table)
        // where
        let where = evalWhere meta q.Where
        if where.Length > 0 then sb.Append (sprintf " WHERE %s" where) |> ignore
        sb.ToString()        
    
module private Reflection =        

    let getFields<'a> () =
        FSharp.Reflection.FSharpType.GetRecordFields(typeof<'a>)
        |> Array.map (fun x -> x.Name)
        |> Array.toList

    let getValues r =
        FSharp.Reflection.FSharpValue.GetRecordFields r
        |> Array.toList
        
module private Preparators =
    
    let prepareSelect<'a> (q:SelectQuery) =
        let fields = Reflection.getFields<'a>()
        // extract metadata
        let meta = WhereAnalyzer.getWhereMetadata [] q.Where
        let query = Evaluators.evalSelectQuery fields meta q
        let pars = WhereAnalyzer.extractWhereParams meta |> Map.ofList
        query, pars
        
    let prepareInsert<'a> (q:InsertQuery) =
        let fields = Reflection.getFields<'a>()
        let query = Evaluators.evalInsertQuery fields q
        query, q.Values
    
    let prepareUpdate<'a> (q:UpdateQuery) =
        let fields = Reflection.getFields<'a>()
        let values = Reflection.getValues q.Value
        // extract metadata
        let meta = WhereAnalyzer.getWhereMetadata [] q.Where
        let pars = (WhereAnalyzer.extractWhereParams meta) @ (List.zip fields values) |> Map.ofList
        let query = Evaluators.evalUpdateQuery fields meta q
        query, pars
        
    let prepareDelete (q:DeleteQuery) =
        let meta = WhereAnalyzer.getWhereMetadata [] q.Where
        let pars = (WhereAnalyzer.extractWhereParams meta) |> Map.ofList
        let query = Evaluators.evalDeleteQuery meta q
        query, pars

module Builders =
        
    type InsertBuilder() =
        member __.Yield _ =
            {
                Table = ""
                Values = []
            } : InsertQuery
        
        /// Sets the TABLE name for query            
        [<CustomOperation "table">]
        member __.Table (state:InsertQuery, name) = { state with Table = name }        
        
        /// Sets the list of values for INSERT
        [<CustomOperation "values">]
        member __.Values (state:InsertQuery, values:'a list) = { state with Values = values }        
        
        /// Sets the single value for INSERT
        [<CustomOperation "value">]
        member __.Value (state:InsertQuery, value:'a) = { state with Values = [value] }        
        
    type DeleteBuilder() =
        member __.Yield _ =
            {
                Table = ""
                Where = Where.Empty
            } : DeleteQuery
        
        /// Sets the TABLE name for query              
        [<CustomOperation "table">]
        member __.Table (state:DeleteQuery, name) = { state with Table = name }        
        
        /// Sets the WHERE condition          
        [<CustomOperation "where">]
        member __.Where (state:DeleteQuery, where:Where) = { state with Where = where }
    
    type UpdateBuilder() =
        member __.Yield _ =
            {
                Table = ""
                Value = null
                Where = Where.Empty
            } : UpdateQuery
                    
        /// Sets the TABLE name for query
        [<CustomOperation "table">]
        member __.Table (state:UpdateQuery, name) = { state with Table = name }        
        
        /// Sets the SET of value to UPDATE
        [<CustomOperation "set">]
        member __.Set (state:UpdateQuery, value:'a) = { state with Value = value }
        
        /// Sets the WHERE condition
        [<CustomOperation "where">]
        member __.Where (state:UpdateQuery, where:Where) = { state with Where = where }        
    
    type SelectBuilder() =
        member __.Yield _ =
            {
                Table = ""
                Where = Where.Empty
                OrderBy = []
                Pagination = Skip 0
                IgnoredColumns = []
            } : SelectQuery
        
        /// Sets the TABLE name for query
        [<CustomOperation "table">]
        member __.Table (state:SelectQuery, name) = { state with Table = name }
        
        /// Sets the WHERE condition
        [<CustomOperation "where">]
        member __.Where (state:SelectQuery, where:Where) = { state with Where = where }        
        
        /// Sets the ORDER BY for multiple columns
        [<CustomOperation "orderByMany">]
        member __.OrderByMany (state:SelectQuery, values) = { state with OrderBy = values }        
        
        /// Sets the ORDER BY for single column
        [<CustomOperation "orderBy">]
        member __.OrderBy (state:SelectQuery, colName, direction) = { state with OrderBy = [(colName, direction)] }        
        
        /// Sets the SKIP value for query
        [<CustomOperation "skip">]
        member __.Skip (state:SelectQuery, skip) = { state with Pagination = Pagination.Skip skip }        
        
        /// Sets the SKIP and TAKE value for query
        [<CustomOperation "skipTake">]
        member __.SkipTake (state:SelectQuery, skip, take) = { state with Pagination = Pagination.SkipTake(skip, take) }
        
        /// Ignore column for mapping
        [<CustomOperation "ignore">]
        member __.Ignore (state:SelectQuery, name) = { state with IgnoredColumns = [name] }        
        
        /// Ignore more columns for mapping
        [<CustomOperation "ignoreMany">]
        member __.IgnoreMany (state:SelectQuery, names) = { state with IgnoredColumns = names }        

        
let insert = Builders.InsertBuilder()
let delete = Builders.DeleteBuilder()
let update = Builders.UpdateBuilder()
let select = Builders.SelectBuilder()  

open System
open Dapper

type System.Data.IDbConnection with
    member this.SelectAsync<'a> (q:SelectQuery) =
        let query, pars = q |> Preparators.prepareSelect<'a>
        printfn "%s" query
        this.QueryAsync<'a>(query, pars)
        
    member this.InsertAsync<'a> (q:InsertQuery) =
        let query, values = q |> Preparators.prepareInsert<'a>
        this.ExecuteAsync(query, values)
        
    member this.UpdateAsync<'a> (q:UpdateQuery) =
        let query, pars = q |> Preparators.prepareUpdate<'a>
        this.ExecuteAsync(query, pars)
    
    member this.DeleteAsync (q:DeleteQuery) =
        let query, pars = q |> Preparators.prepareDelete
        this.ExecuteAsync(query, pars)

module FSharp =
    
    type OptionHandler<'T>() =
        inherit SqlMapper.TypeHandler<option<'T>>()

        override __.SetValue(param, value) = 
            let valueOrNull = 
                match value with
                | Some x -> box x
                | None -> null

            param.Value <- valueOrNull    

        override __.Parse value =
            if isNull value || value = box DBNull.Value 
            then None
            else Some (value :?> 'T)
            
    let registerOptionTypes() =
        SqlMapper.AddTypeHandler (OptionHandler<Guid>())
        SqlMapper.AddTypeHandler (OptionHandler<int64>())
        SqlMapper.AddTypeHandler (OptionHandler<int>())
        SqlMapper.AddTypeHandler (OptionHandler<int16>())
        SqlMapper.AddTypeHandler (OptionHandler<float>())
        SqlMapper.AddTypeHandler (OptionHandler<decimal>())
        SqlMapper.AddTypeHandler (OptionHandler<string>())
        SqlMapper.AddTypeHandler (OptionHandler<char>())
        SqlMapper.AddTypeHandler (OptionHandler<DateTime>())
        SqlMapper.AddTypeHandler (OptionHandler<DateTimeOffset>())
        SqlMapper.AddTypeHandler (OptionHandler<bool>())