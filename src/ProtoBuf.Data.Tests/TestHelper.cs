﻿// Copyright 2012 Richard Dingwall - http://richarddingwall.name
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Xunit;

namespace ProtoBuf.Data.Tests
{
    public static class TestHelper
    {
        public static void AssertContentsEqual(DataSet expected, DataSet actual)
        {
            Assert.Equal(actual.Tables.Count, expected.Tables.Count);

            for (var i = 0; i < expected.Tables.Count; i++)
                AssertContentsEqual(expected.Tables[i], actual.Tables[i]);
        }

        public static void AssertContentsEqual(DataTable expected, DataTable actual)
        {
            if (expected == null) throw new ArgumentNullException("expected");

            Assert.NotNull(actual);

            Assert.Equal(actual.Columns.Count, expected.Columns.Count);

            AssertColumnNamesEqual(expected, actual);
            AssertColumnTypesEqual(expected, actual);
            AssertRowValuesEqual(expected, actual);
        }

        static string GetTableNumber(DataTable table)
        {
            if (table.DataSet == null)
                return "";

            return String.Format("{0}", table.DataSet.Tables.IndexOf(table) + 1);
        }

        static void AssertRowValuesEqual(DataTable expected, DataTable actual)
        {
            Assert.Equal(actual.Rows.Count, expected.Rows.Count);

            for (var i = 0; i < expected.Rows.Count; i++)
            {
                var expectedItemArray = expected.Rows[i].ItemArray;
                var actualItemArray = actual.Rows[i].ItemArray;

                for (var j = 0; j < expectedItemArray.Length; j++)
                {
                    if (expectedItemArray[j] is DataTable)
                    {
                        var expectedTable = (DataTable) expectedItemArray[j];
                        var actualTable = (DataTable)expectedItemArray[j];
                        AssertContentsEqual(expectedTable, actualTable);
                    }
                    else if (expectedItemArray[j] is byte[])
                    {
                        var expectedBlob = (byte[])expectedItemArray[j];
                        var actualBlob = actualItemArray[j];
                        AssertArraysEqual(expectedBlob, actualBlob);
                    }
                    else if (expectedItemArray[j] is char[])
                    {
                        var expectedClob = (char[])expectedItemArray[j];
                        var actualClob = actualItemArray[j];
                        AssertArraysEqual(expectedClob, actualClob);
                    }
                    else
                        Assert.Equal(actualItemArray[j], expectedItemArray[j]);
                }
            }
        }

        static void AssertArraysEqual<T>(ICollection<T> sourceArray, object destArray)
        {
            Assert.Equal(destArray, sourceArray);
        }

        static void AssertColumnNamesEqual(DataTable expected, DataTable actual)
        {
            var expectedNames = GetColumnNames(expected);
            var actualNames = GetColumnNames(actual);

            Assert.Equal(actualNames, expectedNames);
        }

        static string[] GetColumnNames(DataTable dataTable)
        {
            return dataTable.Columns.OfType<DataColumn>().Select(c => c.ColumnName).ToArray();
        }

        static Type[] GetColumnTypes(DataTable dataTable)
        {
            return dataTable.Columns.OfType<DataColumn>().Select(c => c.DataType).ToArray();
        }

        static void AssertColumnTypesEqual(DataTable expected, DataTable actual)
        {
            var expectedTypes = GetColumnTypes(expected);
            var actualTypes = GetColumnTypes(actual);

            Assert.Equal(actualTypes, expectedTypes);
        }

        public static DataTable SerializeAndDeserialize(DataTable dataTable)
        {
            return SerializeAndDeserialize(dataTable, new ProtoDataWriterOptions());
        }

        public static DataSet SerializeAndDeserialize(DataSet dataSet, params string[] tables)
        {
            using (var stream = new MemoryStream())
            {
                DataSerializer.Serialize(stream, dataSet);
                stream.Seek(0, SeekOrigin.Begin);
                return DataSerializer.DeserializeDataSet(stream, tables);
            }
        }

        public static DataTable SerializeAndDeserialize(DataTable dataTable, ProtoDataWriterOptions options)
        {
            using (var stream = new MemoryStream())
            {
                DataSerializer.Serialize(stream, dataTable, options);
                stream.Seek(0, SeekOrigin.Begin);
                return DataSerializer.DeserializeDataTable(stream);
            }
        }
    }

}
