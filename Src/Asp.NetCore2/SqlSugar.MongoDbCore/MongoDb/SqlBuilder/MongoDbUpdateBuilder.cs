﻿using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;

namespace SqlSugar.MongoDb
{
    public class MongoDbUpdateBuilder : UpdateBuilder
    {
        public override string SqlTemplateBatch
        {
            get
            {
                return @"UPDATE  {1} {2} SET {0} FROM  ${{0}}  ";
            }
        }
        public override string SqlTemplateJoin
        {
            get
            {
                return @"            (VALUES
              {0}

            ) AS T ({2}) WHERE {1}
                 ";
            }
        }

        public override string SqlTemplateBatchUnion
        {
            get
            {
                return ",";
            }
        }
        protected override string ToSingleSqlString(List<IGrouping<int, DbColumnInfo>> groupList)
        {
            var result = BuildUpdateMany(groupList,  this.TableName);
            return result;
        }
        protected override string TomultipleSqlString(List<IGrouping<int, DbColumnInfo>> groupList)
        {
            var result = BuildUpdateMany(groupList,this.TableName);
            return result;
        }
        public string BuildUpdateMany(List<IGrouping<int, DbColumnInfo>> groupList, string tableName)
        {
            var operations = new List<string>();
            List<string> pks = this.PrimaryKeys;

            foreach (var group in groupList)
            {
                var filter = new BsonDocument();
                var setDoc = new BsonDocument();

                foreach (var col in group)
                {
                  
                    if (col.IsPrimarykey || pks.Contains(col.DbColumnName))
                    {
                        filter[col.DbColumnName] = BsonValue.Create(ObjectId.Parse(col.Value?.ToString())); ;
                    }
                    else
                    {
                        var bsonValue = BsonValue.Create(col.Value);
                        setDoc[col.DbColumnName] = bsonValue;
                    }
                }

                var update = new BsonDocument
        {
            { "$set", setDoc }
        };

                var entry = new BsonDocument
        {
            { "filter", filter },
            { "update", update }
        };

                string json = entry.ToJson(new MongoDB.Bson.IO.JsonWriterSettings
                {
                    OutputMode = MongoDB.Bson.IO.JsonOutputMode.Shell // JSON标准格式，带双引号
                });

                operations.Add(json);
            }

            var sb = new StringBuilder();
            sb.Append($"updateMany {tableName} [ ");
            sb.Append(string.Join(", ", operations));
            sb.Append(" ]");

            return sb.ToString();
        }


    }
}
