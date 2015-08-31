﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Tavis.UriTemplates;
using Xunit;

namespace UriTemplateTests
{
    public class ParameterMatchingTests
    {
        

        [Fact]
        public void MatchUriToTemplate()
        {
            var uri = new Uri("http://example.com/foo/bar");

            var sTemplate = "http://example.com/{p1}/{p2}";
            var template = new UriTemplate(sTemplate);

            var x = template.CreateMatchingRegex();

            var match = Regex.IsMatch(uri.AbsoluteUri,x);
            Assert.True(match);
        }

        [Fact]
        public void GetParameters()
        {
            var uri = new Uri("http://example.com/foo/bar");

            var template = new UriTemplate("http://example.com/{p1}/{p2}");

            var x = template.CreateMatchingRegex();
            var regex = new Regex(x);

            var match = regex.Match(uri.AbsoluteUri);

            Assert.Equal("foo",match.Groups["p1"].Value);
            Assert.Equal("bar", match.Groups["p2"].Value);
        }

        [Fact]
        public void GetParametersWithOperators()
        {
            var uri = new Uri("http://example.com/foo/bar");

            var template = new UriTemplate("http://example.com/{+p1}/{p2*}");

            var parameters = template.GetParameters(uri);
            
            Assert.Equal("foo", parameters["p1"]);
            Assert.Equal("bar", parameters["p2"]);
        }

        [Fact]
        public void GetParametersFromQueryString()
        {
            var uri = new Uri("http://example.com/foo/bar?blur=45");

            var template = new UriTemplate("http://example.com/{+p1}/{p2*}{?blur}");

            var parameters = template.GetParameters(uri);

            Assert.Equal("foo", parameters["p1"]);
            Assert.Equal("bar", parameters["p2"]);
            Assert.Equal("45", parameters["blur"]);
        }

        [Fact]
        public void GetParametersFromMultipleQueryString()
        {
            var uri = new Uri("http://example.com/foo/bar?blur=45");

            var template = new UriTemplate("http://example.com/{+p1}/{p2*}{?blur,blob}");

            var parameters = template.GetParameters(uri);

            Assert.Equal("foo", parameters["p1"]);
            Assert.Equal("bar", parameters["p2"]);
            Assert.Equal("45", parameters["blur"]);

        }
        [Fact]
        public void GetParametersFromMultipleQueryStringWithTwoParamValues()
        {
            var uri = new Uri("http://example.com/foo/bar?blur=45&blob=23");

            var template = new UriTemplate("http://example.com/{+p1}/{p2*}{?blur,blob}");

            var parameters = template.GetParameters(uri);

            Assert.Equal("foo", parameters["p1"]);
            Assert.Equal("bar", parameters["p2"]);
            Assert.Equal("45", parameters["blur"]);
            Assert.Equal("23", parameters["blob"]);

        }

    }
}