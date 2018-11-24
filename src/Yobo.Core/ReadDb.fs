module internal Yobo.Core.ReadDb

open FSharp.Data.Sql

[<Literal>]
let private testConnection = " "

[<Literal>]
let schemaPath = "./../../database/yobo.schema"

type Db = SqlDataProvider<
                  ConnectionString = testConnection,
                  DatabaseVendor = Common.DatabaseProviderTypes.MSSQLSERVER,
                  UseOptionTypes = true
                  ,ContextSchemaPath = schemaPath
                  >
