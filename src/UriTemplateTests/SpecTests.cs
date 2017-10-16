﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Tavis.UriTemplates;
using Xunit;

namespace UriTemplateTests
{
    public class UriTemplateTests2
    {
        public static IEnumerable<object[]> SpecSamples
        {
            get
            {
                Stream stream = null;

                var suites = new List<Dictionary<string, TestSet>>();

                stream =
                    typeof(UriTemplateTests2).Assembly.GetManifestResourceStream("UriTemplateTests.spec-examples.json");
                suites.Add(CreateTestSuite(new StreamReader(stream).ReadToEnd()));

                stream = typeof(UriTemplateTests2).Assembly.GetManifestResourceStream(
                    "UriTemplateTests.spec-examples-by-section.json");
                suites.Add(CreateTestSuite(new StreamReader(stream).ReadToEnd()));


                foreach (var suite in suites)
                {
                    foreach (var testset in suite.Values)
                    {
                        foreach (var testCase in testset.TestCases)
                        {
                            yield return new object[] {testCase.Template, testCase.Result, testCase};
                        }
                    }
                }
            }
        }


        public static IEnumerable<object[]> ExtendedSamples
        {
            get
            {
                Stream stream = null;

                var suites = new List<Dictionary<string, TestSet>>();

                stream =
                    typeof(UriTemplateTests2).Assembly.GetManifestResourceStream(
                        "UriTemplateTests.extended-tests.json");
                suites.Add(CreateTestSuite(new StreamReader(stream).ReadToEnd()));

                foreach (var suite in suites)
                {
                    foreach (var testset in suite.Values)
                    {
                        foreach (var testCase in testset.TestCases)
                        {
                            yield return new object[] {testCase.Template, testCase.Result, testCase};
                        }
                    }
                }
            }
        }

        public static IEnumerable<object[]> FailureSamples
        {
            get
            {
                Stream stream = null;

                var suites = new List<Dictionary<string, TestSet>>();


                stream =
                    typeof(UriTemplateTests2).Assembly.GetManifestResourceStream(
                        "UriTemplateTests.negative-tests.json");
                suites.Add(CreateTestSuite(new StreamReader(stream).ReadToEnd()));


                foreach (var suite in suites)
                {
                    foreach (var testset in suite.Values)
                    {
                        foreach (var testCase in testset.TestCases)
                        {
                            yield return new object[] {testCase.Template, testCase.Result, testCase};
                        }
                    }
                }
            }
        }

        [Theory]
        [MemberData("SpecSamples")]
        public void SpecSamplesTest(string template, string[] results, TestSet.TestCase testCase)
        {
            var uriTemplate = new UriTemplate(template);

            foreach (var variable in testCase.TestSet.Variables)
            {
                uriTemplate.SetParameter(variable.Key, variable.Value);
            }

            string result = null;
            result = uriTemplate.Resolve();

            Assert.True(results.Contains(result));
        }


        [Theory]
        [MemberData("ExtendedSamples")]
        public void ExtendedSamplesTest(string template, string[] results, TestSet.TestCase testCase)
        {
            var uriTemplate = new UriTemplate(template);

            foreach (var variable in testCase.TestSet.Variables)
            {
                uriTemplate.SetParameter(variable.Key, variable.Value);
            }

            string result = null;
            ArgumentException aex = null;

            try
            {
                result = uriTemplate.Resolve();
            }
            catch (ArgumentException ex)
            {
                aex = ex;
            }

            if (results[0] == "False")
            {
                Assert.NotNull(aex);
            }
            else
            {
                Assert.True(results.Contains(result));
            }
        }


        // Disabled for the moment. [Theory, PropertyData("FailureSamples")]
        public void FailureSamplesTest(string template, string[] results, TestSet.TestCase testCase)
        {
            var uriTemplate = new UriTemplate(template);

            foreach (var variable in testCase.TestSet.Variables)
            {
                uriTemplate.SetParameter(variable.Key, variable.Value);
            }

            string result = null;
            ArgumentException aex = null;

            try
            {
                result = uriTemplate.Resolve();
            }
            catch (ArgumentException ex)
            {
                aex = ex;
            }

            Assert.NotNull(aex);
        }


        private static Dictionary<string, TestSet> CreateTestSuite(string json)
        {
            var token = JObject.Parse(json);

            var testSuite = new Dictionary<string, TestSet>();
            foreach (JProperty levelSet in token.Children())
            {
                testSuite.Add(levelSet.Name, CreateTestSet(levelSet.Name, levelSet.Value));
            }

            return testSuite;
        }

        private static TestSet CreateTestSet(string name, JToken token)
        {
            var testSet = new TestSet
            {
                Name = name
            };

            var variables = token["variables"];

            foreach (JProperty variable in variables)
            {
                ParseVariable(variable, testSet.Variables);
            }

            var testcases = token["testcases"];

            foreach (var testcase in testcases)
            {
                testSet.TestCases.Add(CreateTestCase(testSet, testcase));
            }

            return testSet;
        }

        private static void ParseVariable(JProperty variable, Dictionary<string, object> dictionary)
        {
            if (variable.Value.Type == JTokenType.Array)
            {
                var array = (JArray) variable.Value;
                if (array.Count == 0)
                {
                    dictionary.Add(variable.Name, new List<string>());
                }
                else
                {
                    dictionary.Add(variable.Name, array.Values<string>());
                }
            }
            else if (variable.Value.Type == JTokenType.Object)
            {
                var jvalue = (JObject) variable.Value;
                var dict = new Dictionary<string, string>();
                foreach (var prop in jvalue.Properties())
                {
                    dict[prop.Name] = prop.Value.ToString();
                }

                dictionary.Add(variable.Name, dict);
            }
            else
            {
                if (((JValue) variable.Value).Value == null)
                {
                    dictionary.Add(variable.Name, null);
                }
                else
                {
                    dictionary.Add(variable.Name, variable.Value.ToString());
                }
            }
        }

        private static TestSet.TestCase CreateTestCase(TestSet testSet, JToken testcase)
        {
            var testCase = new TestSet.TestCase(testSet)
            {
                Template = testcase[0].Value<string>()
            };

            if (testcase[1].Type == JTokenType.Array)
            {
                var results = (JArray) testcase[1];
                testCase.Result = results.Select(jv => jv.Value<string>()).ToArray();
            }
            else
            {
                testCase.Result = new string[1];
                testCase.Result[0] = testcase[1].Value<string>();
            }
            return testCase;
        }

        public class TestSet
        {
            public List<TestCase> TestCases = new List<TestCase>();
            public Dictionary<string, object> Variables = new Dictionary<string, object>();
            public string Name { get; set; }
            public int Level { get; set; }

            public class TestCase
            {
                public TestCase(TestSet testSet)
                {
                    TestSet = testSet;
                }

                public TestSet TestSet { get; }

                public string Template { get; set; }
                public string[] Result { get; set; }
            }
        }
    }
}