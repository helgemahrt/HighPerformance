using helgemahrt.HighPerformance.Json;
using System;
using System.IO;
using Xunit;

namespace helgemahrt.HighPerformance.UnitTests.Json
{
    public class JsonTokenizerFixture
    {
        [Fact]
        public void ParsingSimpleStringValue_Works()
        {
            // arrange
            string json = @"{ ""Name"": ""Value"" }";

            JsonTokenizer sut = new JsonTokenizer();

            // act & assert
            AssertTokenType(json, sut, JsonTokenTypeEnum.StartObject);

            AssertPropertyName(json, sut, "Name");
            AssertPropertyValue(json, sut, "Value");

            AssertTokenType(json, sut, JsonTokenTypeEnum.EndObject);
        }

        [Fact]
        public void ParsingSimpleIntegerValue_Works()
        {
            // arrange
            string json = @"{ ""Name"": 12345 }";

            JsonTokenizer sut = new JsonTokenizer();

            // act & assert
            AssertTokenType(json, sut, JsonTokenTypeEnum.StartObject);

            AssertPropertyName(json, sut, "Name");
            AssertPropertyValue(json, sut, "12345");

            AssertTokenType(json, sut, JsonTokenTypeEnum.EndObject);
        }

        [Fact]
        public void ParsingSimpleBooleanValue_Works()
        {
            // arrange
            string json = @"{ ""Name"": true }";

            JsonTokenizer sut = new JsonTokenizer();

            // act & assert
            AssertTokenType(json, sut, JsonTokenTypeEnum.StartObject);

            AssertPropertyName(json, sut, "Name");
            AssertPropertyValue(json, sut, "true");

            AssertTokenType(json, sut, JsonTokenTypeEnum.EndObject);
        }

        [Fact]
        public void ParsingSimpleValuesOnSameLine_Works()
        {
            // arrange
            string json = @"{ ""bool"": true, ""string"": ""value"", ""int"": 1234, ""float"": 1.234 }";

            JsonTokenizer sut = new JsonTokenizer();

            // act & assert
            AssertTokenType(json, sut, JsonTokenTypeEnum.StartObject);

            AssertPropertyName(json, sut, "bool");
            AssertPropertyValue(json, sut, "true");

            AssertPropertyName(json, sut, "string");
            AssertPropertyValue(json, sut, "value");

            AssertPropertyName(json, sut, "int");
            AssertPropertyValue(json, sut, "1234");

            AssertPropertyName(json, sut, "float");
            AssertPropertyValue(json, sut, "1.234");

            AssertTokenType(json, sut, JsonTokenTypeEnum.EndObject);
        }

        [Fact]
        public void ParsingSimpleValuesOnNewLines_Works()
        {
            // arrange
            string json = "{\n \"bool\": true,\n \"string\": \"value\",\n \"int\": 1234,\n \"float\": 1.234\n }";

            JsonTokenizer sut = new JsonTokenizer();

            // act & assert
            AssertTokenType(json, sut, JsonTokenTypeEnum.StartObject);

            AssertPropertyName(json, sut, "bool");
            AssertPropertyValue(json, sut, "true");

            AssertPropertyName(json, sut, "string");
            AssertPropertyValue(json, sut, "value");

            AssertPropertyName(json, sut, "int");
            AssertPropertyValue(json, sut, "1234");

            AssertPropertyName(json, sut, "float");
            AssertPropertyValue(json, sut, "1.234");

            AssertTokenType(json, sut, JsonTokenTypeEnum.EndObject);
        }



        [Fact]
        public void ParsingComplexJsonObject_Works()
        {
            // arrange
            string json = File.ReadAllText("complexJsonObject.json");
            JsonTokenizer sut = new JsonTokenizer();

            // act & assert

            // root object
            AssertTokenType(json, sut, JsonTokenTypeEnum.StartObject);

            AssertPropertyName(json, sut, "Name");
            AssertPropertyValue(json, sut, "Foo");

            AssertPropertyName(json, sut, "Age");
            AssertPropertyValue(json, sut, "123");

            AssertPropertyName(json, sut, "IsHuman");
            AssertPropertyValue(json, sut, "false");

            // tags array
            AssertPropertyName(json, sut, "Tags");
            AssertTokenType(json, sut, JsonTokenTypeEnum.StartArray);
            AssertPropertyName(json, sut, "robot");
            AssertPropertyName(json, sut, "ai");
            AssertPropertyName(json, sut, "replicator");
            AssertPropertyName(json, sut, "42");
            AssertTokenType(json, sut, JsonTokenTypeEnum.EndArray);

            // inventory object
            AssertPropertyName(json, sut, "Inventory");
            AssertTokenType(json, sut, JsonTokenTypeEnum.StartObject);
            AssertPropertyName(json, sut, "Slots");
            AssertPropertyValue(json, sut, "10");

            AssertPropertyName(json, sut, "Weight");
            AssertPropertyValue(json, sut, "1.7");

            // items array
            AssertPropertyName(json, sut, "Items");
            AssertTokenType(json, sut, JsonTokenTypeEnum.StartArray);

            // inventory item
            AssertTokenType(json, sut, JsonTokenTypeEnum.StartObject);
            AssertPropertyName(json, sut, "Name");
            AssertPropertyValue(json, sut, "Burner");
            AssertTokenType(json, sut, JsonTokenTypeEnum.EndObject);

            // inventory item
            AssertTokenType(json, sut, JsonTokenTypeEnum.StartObject);
            AssertPropertyName(json, sut, "Name");
            AssertPropertyValue(json, sut, "Lightsaber");
            AssertTokenType(json, sut, JsonTokenTypeEnum.EndObject);

            AssertTokenType(json, sut, JsonTokenTypeEnum.EndArray);
            AssertTokenType(json, sut, JsonTokenTypeEnum.EndObject);

            AssertTokenType(json, sut, JsonTokenTypeEnum.EndObject);
        }

        private static void AssertTokenType(string json, JsonTokenizer sut, JsonTokenTypeEnum jsonTokenType)
        {
            (JsonTokenTypeEnum jsonTokenType, int startIndex, int length) tokenData = sut.GetNextToken(json);
            Assert.Equal(jsonTokenType, tokenData.jsonTokenType);
        }

        private static void AssertPropertyValue(string json, JsonTokenizer sut, string value)
        {
            (JsonTokenTypeEnum jsonTokenType, int startIndex, int length) tokenData = sut.GetNextToken(json);
            Assert.Equal(JsonTokenTypeEnum.PropertyValue, tokenData.jsonTokenType);
            Assert.True(JsonTokenizer.GetTokenValue(tokenData, json).SequenceEqual(value));
        }

        private static void AssertPropertyName(string json, JsonTokenizer sut, string name)
        {
            (JsonTokenTypeEnum jsonTokenType, int startIndex, int length) tokenData = sut.GetNextToken(json);
            Assert.Equal(JsonTokenTypeEnum.PropertyName, tokenData.jsonTokenType);
            Assert.True(JsonTokenizer.GetTokenValue(tokenData, json).SequenceEqual(name));
        }
    }
}
